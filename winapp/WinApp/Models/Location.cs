// Copyright (c) 2021 Inaba
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;

namespace WinApp.Models
{
    public class Location
    {
        public DateTime Timestamp { get; set; }

        public string Name { get; set; }

        public string UUID { get; set; }

        public int Major { get; set; }

        public int Minor { get; set; }

        public int RSSI { get; set; }
    }
}
