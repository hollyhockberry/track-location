// beacon/main.cpp - iBeacon advertiser
// Based on 'BLE_iBeacon.ino' from https://github.com/h2zero/NimBLE-Arduino
//
// Copyright (c) 2021 Inaba
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
//
// Send out iBeacon advertisements periodically.
//  1. Send out iBeacon advertisements for ADVERTISING_SEC.
//  2. Deep sleep for SLEEP_TIME_SEC.
//  3. wakeup (back to 1.)
//
// Setup:
//  Configure parameters with an interactive interface via Serial
//  Enter the setup mode in any of the following ways.
//  - Hold the button (connected to PIN_BUTTON) at reboot.
//  - Preferences are empty.
//  - Serial port is available at reboot.

#include <Arduino.h>
#include "NimBLEDevice.h"
#include "NimBLEBeacon.h"

#include "beaconpreferences.h"

namespace {

BeaconPreferences preferences;

const int PIN_BUTTON = 39;
const uint32_t ADVERTISING_SEC = 10;
const uint64_t SLEEP_TIME_SEC = 10;

void setBeacon(BLEAdvertising* advertising, const BeaconPreferences& pref) {
  BLEBeacon oBeacon = BLEBeacon();
  // fake Apple 0x004C LSB (ENDIAN_CHANGE_U16!)
  oBeacon.setManufacturerId(0x4C00);
  oBeacon.setProximityUUID(BLEUUID(pref.UUID()));
  oBeacon.setMajor(pref.Major());
  oBeacon.setMinor(pref.Minor());
  BLEAdvertisementData oAdvertisementData = BLEAdvertisementData();
  BLEAdvertisementData oScanResponseData = BLEAdvertisementData();

  oAdvertisementData.setFlags(0x04);  // BR_EDR_NOT_SUPPORTED 0x04

  std::string strServiceData = "";
  strServiceData += static_cast<char>(26);
  strServiceData += static_cast<char>(0xFF);
  strServiceData += oBeacon.getData();
  oAdvertisementData.addData(strServiceData);

  advertising->setAdvertisementData(oAdvertisementData);
  advertising->setScanResponseData(oScanResponseData);

  advertising->setAdvertisementType(BLE_GAP_CONN_MODE_NON);
}

}  // namespace

void setup() {
  Serial.begin(115200);
  ::pinMode(PIN_BUTTON, INPUT);

  if (!preferences.Load()) {
    return;
  }
  if (::digitalRead(PIN_BUTTON) == LOW || Serial.available()) {
    return;
  }

  preferences.Show();
  BLEDevice::init("");
  BLEAdvertising* advertising = BLEDevice::getAdvertising();
  setBeacon(advertising, preferences);

  advertising->start();
  ::delay(ADVERTISING_SEC*1000);
  advertising->stop();

  ::esp_deep_sleep(SLEEP_TIME_SEC*1000*1000LL);
  for (;;) {}
}

void loop() {
  preferences.Run();
}
