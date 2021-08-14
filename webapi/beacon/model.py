# beacon/model.py
# Copyright (c) 2021 Inaba
# This software is released under the MIT License.
# http://opensource.org/licenses/mit-license.php

from sqlalchemy import Column, Integer, String

from database.db import Base

class Beacon(Base):
  __tablename__ = 'beacons'
  userid = Column(String, primary_key=True)
  uuid = Column(String(36))
  major = Column(Integer)
  minor = Column(Integer)
