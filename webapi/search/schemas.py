# search/schemas.py
# Copyright (c) 2021 Inaba
# This software is released under the MIT License.
# http://opensource.org/licenses/mit-license.php

from pydantic import BaseModel
from typing import Optional

class Point(BaseModel):
  time: str
  name: str
  location: str

  class Config:
    orm_mode = True,
    schema_extra = {
      'example' : {
        'time': 'RFC3339 formatted UTC time',
        'name': 'user name',
        'location': 'location',
      }
    }


class UserDescription(BaseModel):
  description: str
