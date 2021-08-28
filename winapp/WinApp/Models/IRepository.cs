// Copyright (c) 2021 Inaba
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

namespace WinApp.Models
{
    public interface IRepository
    {
        string InfluxHost { get; }

        string Database { get; }

        string Measurement { get; }

        int? Sampling { get => null; }

        int? Cycle { get => null; }

        int? PostInterval { get => null; }

        string ApiHost { get; }

        string UserID { get; set; }

        void Load();

        void Save();
    }
}
