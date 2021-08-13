// Users.cs - Database schema definitions for User
// Copyright (c) 2021 Inaba
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.Text.Json.Serialization;

namespace BeaconWatcherLibrary.Schemas
{
    class User
    {
        [JsonPropertyName("id")]
        public string ID { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }

    class UserCreate
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}
