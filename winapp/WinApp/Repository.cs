// Copyright (c) 2021 Inaba
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.IO;
using System.Reflection;
using System.Text.Json;

namespace WinApp
{
    sealed class Repository : Models.IRepository
    {
        public string InfluxHost { get; private set; }

        public string Database { get; private set; }

        public string Measurement { get; private set; }

        public string ApiHost { get; private set; }

        public string UserID
        {
            get => Settings.Default.UserID;
            set => Settings.Default.UserID = value;
        }

        public void Load()
        {
            using var stream = Assembly
                .GetExecutingAssembly()
                .GetManifestResourceStream("WinApp.Settings.json");
            using var sr = new StreamReader(stream);

            var config = JsonSerializer.Deserialize<Config>(sr.ReadToEnd());
            InfluxHost = config.InfluxHost;
            Database = config.Database;
            Measurement = config.Measurement;
            ApiHost = config.ApiHost;
        }

        public void Save() => Settings.Default.Save();
    }

    class Config
    {
        public string InfluxHost { get; set; }

        public string Database { get; set; }

        public string Measurement { get; set; }

        public string ApiHost { get; set; }
    }
}
