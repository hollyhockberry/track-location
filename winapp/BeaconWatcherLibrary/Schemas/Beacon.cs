// Location.cs - Database schema definitions for Location
// Copyright (c) 2021 Inaba
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.Text.Json.Serialization;

namespace BeaconWatcherLibrary.Schemas
{
    class Beacon
    {
        [JsonPropertyName("userid")]
        public string User { get; set; }

        [JsonPropertyName("uuid")]
        public string UUID { get; set; }

        [JsonPropertyName("major")]
        public int Major { get; set; }

        [JsonPropertyName("minor")]
        public int Minor { get; set; }
    }
}
