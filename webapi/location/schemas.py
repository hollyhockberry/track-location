# location/schemas.py
# Copyright (c) 2021 Inaba
# This software is released under the MIT License.
# http://opensource.org/licenses/mit-license.php

from pydantic import BaseModel
from typing import Optional

class Location(BaseModel):
  id: int
  location: str
  uuid: str
  major: int
  minor: int

  class Config:
    orm_mode = True


class LocationCreate(BaseModel):
  location: str
  uuid: str
  major: int
  minor: int


class LocationUpdate(BaseModel):
  location: Optional[str] = None
  uuid: Optional[str] = None
  major: Optional[int] = None
  minor: Optional[int] = None
