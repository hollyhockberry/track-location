# user/schemas.py
# Copyright (c) 2021 Inaba
# This software is released under the MIT License.
# http://opensource.org/licenses/mit-license.php

from pydantic import BaseModel
from typing import Optional

class User(BaseModel):
  id: str
  name: str
  description: str

  class Config:
    orm_mode = True
  

class UserCreate(BaseModel):
  name: str
  description: str


class UserRead(BaseModel):
  description: str


class UserUpdate(BaseModel):
  name: Optional[str] = None
  description: Optional[str] = None
