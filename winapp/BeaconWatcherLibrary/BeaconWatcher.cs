// Copyright (c) 2021 Inaba
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client;
using Vibrant.InfluxDB.Client.Rows;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Storage.Streams;

namespace BeaconWatcherLibrary
{
    using Schemas;

    public class BeaconWatcher
    {
        public class UpdateLocationEventArgs : EventArgs
        {
            public DateTimeOffset Timestamp { get; set; }

            public string Location { get; set; }

            public string UUID { get; set; }

            public int Major { get; set; }

            public int Minor { get; set; }

            public int RSSI { get; set; }
        }

        public delegate void UpdateLocationHandler(object sender, UpdateLocationEventArgs e);

        public event UpdateLocationHandler OnUpdateLocation;

        /// <summary>
        /// InfluxDB Host address
        /// </summary>
        public string InfluxHost { get; set; } = "http://localhost:8086";

        /// <summary>
        /// InfluxDB Database name
        /// </summary>
        public string Database { get; set; } = null;

        /// <summary>
        /// InfluxDB Measurement
        /// </summary>
        public string Measurement { get; set; } = null;

        /// <summary>
        /// API host address
        /// </summary>
        public string ApiHost { get; set; } = "http://localhost/api";

        public string UserID { get; private set; }

        public string UserName { get; private set; }

        public string UserDescription { get; private set; }

        /// <summary>
        /// Beacon detection period. (seconds)
        /// </summary>
        public int Sampling { get; set; } = 20;

        /// <summary>
        /// Beacon detection start cycle. (seconds) 
        /// </summary>
        public int Cycle { get; set; } = 120;

        /// <summary>
        /// Interval to post when the same beacon is detected. (seconds)
        /// </summary>
        public int PostInterval { get; set; } = 10 * 60;

        public bool PostUserBeacons { get; set; } = true;

        private Dictionary<string, string> Locations { get; set; } = null;

        private Dictionary<string, string> UserBeacons { get; set; } = null;

        private readonly (int skip, int take)[] uuid_index;

        public BeaconWatcher()
        {
            var chars = new int[] { 4, 2, 2, 2, 6 };
            uuid_index = chars
                .Select((v, i) => chars.Take(i).Sum())
                .Zip(chars)
                .ToArray();
        }

        /// <summary>
        /// Create new user.
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="name">User name</param>
        /// <param name="description">User description</param>
        /// <returns>success or failure</returns>
        /// <exception cref="WebException"></exception>
        /// <exception cref="UriFormatException"></exception>
        public async Task<bool> CreateUser(
            string id, string name, string description)
        {
            try
            {
                var reqest = (HttpWebRequest)WebRequest.Create($"{ApiHost}/user/{id}");
                reqest.ContentType = "application/json";
                reqest.Method = "POST";
                using (var writer = new StreamWriter(reqest.GetRequestStream()))
                {
                    await writer.WriteAsync(
                        JsonSerializer.Serialize(new UserCreate
                        {
                            Name = name,
                            Description = description
                        }));
                }
                using var response = (HttpWebResponse)reqest.GetResponse();
                return response.StatusCode == HttpStatusCode.NoContent;
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    var code = ((HttpWebResponse)e.Response).StatusCode;
                    if (code == HttpStatusCode.BadRequest)
                        return false;
                }
                throw;
            }
        }

        /// <summary>
        /// Set the user ID by querying the database.
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>success or failure</returns>
        /// <exception cref="WebException"></exception>
        /// <exception cref="UriFormatException"></exception>
        public async Task<bool> SetUser(string id)
        {
            try
            {
                var reqest = WebRequest.Create($"{ApiHost}/user/{id}");
                using var response = reqest.GetResponse().GetResponseStream();
                using var reader = new StreamReader(response);
                var json = await reader.ReadToEndAsync();
                var user = JsonSerializer.Deserialize<User>(json);
                UserID = id;
                UserName = user.Name;
                UserDescription = user.Description;
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    var code = ((HttpWebResponse)e.Response).StatusCode;
                    if (code == HttpStatusCode.NotFound)
                        return false;
                }
                throw;
            }
            return true;

        }

        private async Task UpdateBeacons(string apiHost)
        {
            await Task.WhenAll(
                UpdateLocations(apiHost),
                UpdateUserBeacons(apiHost));
        }

        private async Task UpdateLocations(string apiHost)
        {
            var reqest = WebRequest.Create($"{apiHost}/location");
            using var response = reqest.GetResponse().GetResponseStream();
            using var reader = new StreamReader(response);
            var json = await reader.ReadToEndAsync();
            var locations = JsonSerializer.Deserialize<Location[]>(json);

            Locations = locations
                .ToDictionary(
                    l => $"{l.UUID.ToUpper()}:{l.Major:X4}:{l.Minor:X4}",
                    l => l.Name);
        }

        private async Task UpdateUserBeacons(string apiHost)
        {
            var reqest = WebRequest.Create($"{apiHost}/beacon");
            using var response = reqest.GetResponse().GetResponseStream();
            using var reader = new StreamReader(response);
            var json = await reader.ReadToEndAsync();
            var beacons = JsonSerializer.Deserialize<Schemas.Beacon[]>(json);

            UserBeacons = beacons
                .ToDictionary(
                    l => $"{l.UUID.ToUpper()}:{l.Major:X4}:{l.Minor:X4}",
                    l => l.User);
        }

        public async Task Begin(CancellationToken? cancellationToken)
        {
            var founds = new List<Beacon>();
            Beacon posted = null;
            var beacons = new List<string>();

            var watcher = new BluetoothLEAdvertisementWatcher();
            watcher.SignalStrengthFilter.SamplingInterval
                = TimeSpan.FromSeconds(5);

            watcher.Received += (s, e) =>
            {
                var manufactureerdata =
                    e.Advertisement.ManufacturerData.FirstOrDefault();

                if (manufactureerdata?.CompanyId != 0x004C ||
                    manufactureerdata?.Data.Length != 23)
                    return;

                // iBeacon frame (Manufacturer specific data)
                // |len|type|CompanyID|Beacon type|UUID|Major|Minor|Tx|
                // | 2 | 1  | 2       | 2         | 16 | 2   | 2   |1 |
                //                    | > Advertisement.ManufacturerData
                using var reader = DataReader.FromBuffer(manufactureerdata.Data);
                var data = new byte[manufactureerdata.Data.Length];
                reader.ReadBytes(data);
                var uuid = UUID(data.Skip(2).Take(16)).ToUpper();
                var major = ToUInt16(data.Skip(18).Take(2));
                var minor = ToUInt16(data.Skip(20).Take(2));

                var key = $"{uuid}:{major:X4}:{minor:X4}";
                if (Locations?.ContainsKey(key) == true)
                {
                    founds.Add(new Beacon
                    {
                        Timestamp = e.Timestamp,
                        Location = Locations[key],
                        UUID = uuid,
                        Major = major,
                        Minor = minor,
                        RSSI = e.RawSignalStrengthInDBm
                    });
                    Debug.Print(founds.Last().ToString());
                }
                if (PostUserBeacons && (UserBeacons?.ContainsKey(key) == true))
                {
                    beacons.Add(UserBeacons[key]);
                    Debug.Print($"Beacon: {beacons.Last()}");
                }
            };

            await UpdateBeacons(ApiHost);
            var lastupdate = DateTime.Now;

            cancellationToken?.ThrowIfCancellationRequested();

            while (true)
            {
                var begin = DateTime.Now;
                Debug.Print($"{DateTime.Now}: Begin");

                if ((DateTime.Now - lastupdate).TotalHours > 8)
                {
                    await UpdateBeacons(ApiHost);
                    lastupdate = DateTime.Now;
                }

                founds.Clear();
                watcher.Start();
                await Delay(Sampling * 1000);
                watcher.Stop();
                Debug.Print($"{DateTime.Now}: End");

                if (await Post(founds.Max(), beacons.Distinct()))
                {
                    beacons.Clear();
                }

                var wait = Cycle - (int)((DateTime.Now - begin).TotalSeconds);
                if (wait > 0) await Delay(wait * 1000);

                Task Delay(int msec) => cancellationToken is null
                    ? Task.Delay(msec) : Task.Delay(msec, cancellationToken.Value);
            }

            async Task<bool> Post(Beacon beacon, IEnumerable<string> users)
            {
                if (beacon is null)
                    return false;
                Debug.Print($"Nearest: {beacon}");
                var diff = beacon.Timestamp - posted?.Timestamp;

                if (beacon.Equals(posted) && diff != null &&
                        PostInterval > diff.Value.TotalSeconds)
                    return false;

                Debug.Print("Post!");
                using var client = new InfluxClient(new Uri(InfluxHost));
                DynamicInfluxRow Row(string user)
                {
                    var row = new DynamicInfluxRow();
                    row.Tags.Add("user", user);
                    row.Fields.Add("uuid", beacon.UUID);
                    row.Fields.Add("major", beacon.Major);
                    row.Fields.Add("minor", beacon.Minor);
                    return row;
                }
                var rows = new List<DynamicInfluxRow> { Row(UserID) };
                rows.AddRange(users.Select(user => Row(user)));

                try
                {
                    await client.WriteAsync(Database, Measurement, rows);
                    posted = beacon;

                    OnUpdateLocation?.Invoke(
                        this,
                        new UpdateLocationEventArgs
                        {
                            Timestamp = beacon.Timestamp,
                            Location = beacon.Location,
                            UUID = beacon.UUID,
                            Major = beacon.Major,
                            Minor = beacon.Minor,
                            RSSI = beacon.RSSI
                        });
                }
                catch (InfluxException)
                {
                }

                return true;
            }
        }

        private class Beacon : IComparable<Beacon>, IEquatable<Beacon>
        {
            public DateTimeOffset Timestamp { get; set; }

            public string Location { get; set; }

            public string UUID { get; set; }

            public int Major { get; set; }

            public int Minor { get; set; }

            public int RSSI { get; set; }

            public int CompareTo([AllowNull] Beacon other)
                => RSSI.CompareTo(other?.RSSI ?? int.MinValue);

            public bool Equals([AllowNull] Beacon other)
                => other != null &&
                   UUID == other.UUID &&
                   Major == other.Major &&
                   Minor == other.Minor;

            public override string ToString()
                => $"{Timestamp}: {Location} :{UUID}, " +
                   $"{Major:X4}, {Minor:X4}, {RSSI}";
        }

        private string UUID(IEnumerable<byte> data)
        {
            var uuid = uuid_index
                .Select(t => data.Skip(t.skip).Take(t.take))
                .Select(d => d.Select(v => $"{v:X2}"))
                .Select(d => string.Join(null, d));
            return string.Join('-', uuid);
        }

        private int ToUInt16(IEnumerable<byte> data)
            => BitConverter.ToUInt16(BitConverter.IsLittleEndian
                ? data.Take(2).Reverse().ToArray()
                : data.ToArray(), 0);
    }
}
