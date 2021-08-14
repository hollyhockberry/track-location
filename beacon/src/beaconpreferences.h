// beacon/beaconpreferences.h
//
// Copyright (c) 2021 Inaba
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

#ifndef BEACONPREFERENCES_H__

#include <WString.h>

class BeaconPreferences {
  String _uuid;
  uint16_t _major, _minor;
  uint32_t _advertising_sec;
  uint64_t _sleeptime_sec;

 public:
  BeaconPreferences();

  // properties
  const char* UUID() const {
    return _uuid.c_str();
  }
  uint16_t Major() const {
    return _major;
  }
  uint16_t Minor() const {
    return _minor;
  }
  uint32_t AdvertisingSeconds() const {
    return _advertising_sec;
  }
  uint64_t SleepTime() const {
    return _sleeptime_sec;
  }

  bool Load();
  void Run();

  void Show() const;
};

#endif  // BEACONPREFERENCES_H__

