# location/crud.py - CRUD functions
# Copyright (c) 2021 Inaba
# This software is released under the MIT License.
# http://opensource.org/licenses/mit-license.php

from sqlalchemy.orm import Session
from sqlalchemy.exc import NoResultFound

from . import model, schemas

def same_entry(db, uuid, major, minor):
  return db.query(model.Location)\
    .filter(model.Location.uuid == uuid)\
    .filter(model.Location.major == major)\
    .filter(model.Location.minor == minor)\
    .all()


def create(db: Session, data: schemas.LocationCreate):
  if same_entry(db, data.uuid, data.major, data.minor) != []:
    raise FileExistsError

  item = model.Location(
    location = data.location,
    uuid = data.uuid,
    major = data.major,
    minor = data.minor
  )
  db.add(item)
  db.commit()
  db.refresh(item)
  return item


def read_all(db: Session):
  return db.query(model.Location).all()


def read(db: Session, id: int):
  return db.query(model.Location).filter(model.Location.id == id).first()


def read_by_uuids(db: Session, uuid: str, major: int, minor: int):
  return db.query(model.Location)\
    .filter(model.Location.uuid == uuid)\
    .filter(model.Location.major == major)\
    .filter(model.Location.minor == minor)\
    .first()


def update(db: Session, id: int, data: schemas.LocationUpdate):
  item = db.query(model.Location).get(id)
  if item is None:
    raise NoResultFound
  uuid = data.uuid if data.uuid is not None else item.uuid
  major = data.major if data.major is not None else item.major
  minor = data.minor if data.minor is not None else item.minor

  same = same_entry(db, uuid, major, minor)
  if same != [] and same[0].id != id:
    raise FileExistsError

  item.location = data.location if data.location is not None else item.location
  item.uuid = uuid
  item.major = major
  item.minor = minor
  db.commit()


def delete(db: Session, id: int):
  item = db.query(model.Location).get(id)
  if item is None:
    raise NoResultFound
  db.delete(item)
  db.commit()
