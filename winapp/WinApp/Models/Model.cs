// Copyright (c) 2021 Inaba
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Threading;
using System.Threading.Tasks;

using BeaconWatcherLibrary;

namespace WinApp.Models
{
    public delegate void StateChangedEventHandler(object sender, EventArgs e);

    public delegate void LocationChangeEventHandler(object sender, EventArgs e);

    public enum State
    {
        Offline,
        Ready,
        Runnig,
    }

    public class Model
    {
        public event StateChangedEventHandler StateChanged;

        public event LocationChangeEventHandler LocationChanged;

        private readonly BeaconWatcher beaconWatcher = new BeaconWatcher();

        private bool SignedIn { get; set; } = false;

        public State State
        {
            get
            {
                if (!SignedIn)
                {
                    return State.Offline;
                }
                return CancellationTokenSource is null
                    ? State.Ready : State.Runnig;
            }
        }

        public Location LastLocation { get; private set; }

        public string UserID { get; private set; } = null;

        public string UserName => SignedIn ? beaconWatcher.UserName : null;

        public string UserDescription => SignedIn ? beaconWatcher.UserDescription : null;

        private CancellationTokenSource CancellationTokenSource = null;

        private readonly IRepository Repository = null;

        public Model(IRepository repository)
        {
            Repository = repository;
        }

        public async Task Initialize()
        {
            beaconWatcher.OnUpdateLocation += (s, e) =>
            {
                LastLocation = new Location
                {
                    Timestamp = e.Timestamp.LocalDateTime,
                    Name = e.Location,
                    UUID = e.UUID,
                    Major = e.Major,
                    Minor = e.Minor,
                    RSSI = e.RSSI
                };
                LocationChanged?.Invoke(this, EventArgs.Empty);
            };

            Repository.Load();
            beaconWatcher.InfluxHost = Repository.InfluxHost;
            beaconWatcher.Database = Repository.Database;
            beaconWatcher.Measurement = Repository.Measurement;
            if (Repository.Sampling.HasValue)
                beaconWatcher.Sampling = Repository.Sampling.Value;
            if (Repository.Cycle.HasValue)
                beaconWatcher.Cycle = Repository.Cycle.Value;
            if (Repository.PostInterval.HasValue)
                beaconWatcher.PostInterval = Repository.PostInterval.Value;

            if (!await Startup())
                return;

            await Start();
        }

        private async Task<bool> Startup()
        {
            UserID = Repository.UserID;

            if (string.IsNullOrEmpty(UserID))
            {
                Repository.UserID =
                UserID = Guid.NewGuid().ToString("N");
                Repository.Save();
            }

            return await SignIn(UserID);
        }

        public async Task<bool> SignIn(string id)
        {
            System.Diagnostics.Debug.Print("Signin");

            if (State != State.Offline)
                throw new InvalidCastException();
            if (string.IsNullOrEmpty(id))
                return false;

            if (!await beaconWatcher.SetUser(Repository.ApiHost, id))
                return false;

            if (UserID != beaconWatcher.UserID)
            {
                Repository.UserID =
                UserID = beaconWatcher.UserID;
                Repository.Save();
            }
            SignedIn = true;
            StateChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public async Task<bool> SignUp(string id, string name, string description)
        {
            System.Diagnostics.Debug.Print("Signup");

            if (State != State.Offline)
                throw new InvalidCastException();
            if (string.IsNullOrEmpty(id))
                return false;

            if (!await beaconWatcher.CreateUser(Repository.ApiHost, id, name, description))
                return false;

            return await SignIn(id);
        }

        public void SignOut()
        {
            System.Diagnostics.Debug.Print("Signout");
            if (State != State.Ready)
                throw new InvalidCastException();

            SignedIn = false;
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task Start()
        {
            System.Diagnostics.Debug.Print("Start");

            await beaconWatcher.UpdateBeacons(Repository.ApiHost);

            using var cts = new CancellationTokenSource();
            CancellationTokenSource = cts;
            StateChanged?.Invoke(this, EventArgs.Empty);

            try
            {
                await beaconWatcher.Begin(cts.Token);
            }
            catch (TaskCanceledException)
            {
                System.Diagnostics.Debug.Print("Canceled");
            }
            finally
            {
                CancellationTokenSource = null;
            }
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Stop()
        {
            System.Diagnostics.Debug.Print("Stop");
            CancellationTokenSource?.Cancel();
        }
    }
}
