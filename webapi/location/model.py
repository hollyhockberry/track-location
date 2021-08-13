# location/model.py
# Copyright (c) 2021 Inaba
# This software is released under the MIT License.
# http://opensource.org/licenses/mit-license.php

from sqlalchemy import Column, Integer, String

from database.db import Base

class Location(Base):
  __tablename__ = 'locations'
  id = Column(Integer, primary_key=True, autoincrement=True)
  location = Column(String)
  uuid = Column(String(36))
  major = Column(Integer)
  minor = Column(Integer)
