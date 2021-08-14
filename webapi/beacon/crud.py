# beacon/crud.py - CRUD functions
# Copyright (c) 2021 Inaba
# This software is released under the MIT License.
# http://opensource.org/licenses/mit-license.php

from sqlalchemy.orm import Session
from sqlalchemy.exc import NoResultFound

from . import model, schemas

def same_entries(db, uuid, major, minor):
  return db.query(model.Beacon)\
    .filter(model.Beacon.uuid == uuid)\
    .filter(model.Beacon.major == major)\
    .filter(model.Beacon.minor == minor)\
    .all()


def create(db: Session, id: str, data: schemas.BeaconCreate):
  if same_entries(db, data.uuid, data.major, data.minor) != []:
    raise FileExistsError

  item = model.Beacon(
    userid = id,
    uuid = data.uuid,
    major = data.major,
    minor = data.minor
  )
  db.add(item)
  db.commit()
  db.refresh(item)
  return item


def read(db: Session):
  return db.query(model.Beacon).all()


def update(db: Session, id: str, data: schemas.BeaconUpdate):
  item = db.query(model.Beacon).get(id)
  if item is None:
    raise NoResultFound

  uuid = data.uuid if data.uuid is not None else item.uuid
  major = data.major if data.major is not None else item.major
  minor = data.minor if data.minor is not None else item.minor

  same = same_entries(db, uuid, major, minor)
  if same != [] and same[0].userid != id:
    raise FileExistsError
  
  item.uuid = uuid
  item.major = major
  item.minor = minor
  db.commit()


def delete(db: Session, id: str):
  item = db.query(model.Beacon).get(id)
  if item is None:
    raise NoResultFound
  db.delete(item)
  db.commit()
