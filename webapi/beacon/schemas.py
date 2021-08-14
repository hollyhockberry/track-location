# beacon/schemas.py
# Copyright (c) 2021 Inaba
# This software is released under the MIT License.
# http://opensource.org/licenses/mit-license.php

from pydantic import BaseModel
from typing import Optional

class Beacon(BaseModel):
  userid: str
  uuid: str
  major: int
  minor: int

  class Config:
    orm_mode = True


class BeaconCreate(BaseModel):
  uuid: str
  major: int
  minor: int


class BeaconUpdate(BaseModel):
  uuid: Optional[str] = None
  major: Optional[int] = None
  minor: Optional[int] = None
