# user/model.py
# Copyright (c) 2021 Inaba
# This software is released under the MIT License.
# http://opensource.org/licenses/mit-license.php

from sqlalchemy import Column, String

from database.db import Base

class User(Base):
  __tablename__ = 'users'
  id = Column(String, primary_key=True)
  name = Column(String)
  description = Column(String)
