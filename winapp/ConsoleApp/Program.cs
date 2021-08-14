// Copyright (c) 2021 Inaba
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Reflection;
using System.Threading.Tasks;
using System.IO;

namespace ConsoleApp
{
    using BeaconWatcherLibrary;

    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length < 5)
            {
                Console.WriteLine("Usage:");
                var f = Assembly.GetEntryAssembly().Location;
                var n = Path.GetFileNameWithoutExtension(f);
                Console.WriteLine($"{n} InfluxDBHost Database Measurement TrackerAPI UserID");
                Console.WriteLine();
                Console.WriteLine($"ex: {n} http://localhost:8086 database measurement http://localhost/api");
                return;
            }

            var influxHost = args[0];
            var database = args[1];
            var measurement = args[2];
            var apiUrl = args[3];
            var userID = args[4];

            var watcher = new BeaconWatcher
            {
                InfluxHost = influxHost,
                Database = database,
                Measurement = measurement,
            };

            watcher.OnUpdateLocation += (s, e) =>
            {
                Console.WriteLine($"{e.Timestamp}: {e.Location} RSSI({e.RSSI})");
            };

            if (!await watcher.SetUser(apiUrl, userID)) {
                throw new Exception();
            }

            await watcher.UpdateBeacons(apiUrl);
            await watcher.Begin(null);
        }
    }
}
