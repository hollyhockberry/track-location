# user/crud.py - CRUD functions
# Copyright (c) 2021 Inaba
# This software is released under the MIT License.
# http://opensource.org/licenses/mit-license.php

from sqlalchemy.orm import Session
from sqlalchemy.exc import NoResultFound

from . import model, schemas

def create(db: Session, id: str, data: schemas.UserCreate):
  item = model.User(
    id = id,
    name = data.name,
    description = data.description
  )
  db.add(item)
  db.commit()
  db.refresh(item)
  return item


def read_all(db: Session):
  return db.query(model.User).all()


def read(db: Session, id: str):
  return db.query(model.User).filter(model.User.id == id).first()


def update(db: Session, id: str, data: schemas.UserUpdate):
  item = db.query(model.User).get(id)
  if item is None:
    raise NoResultFound
  if data.name != None:
    item.name = data.name
  if data.description != None:
    item.description = data.description
  db.commit()


def delete(db: Session, id: str):
  item = db.query(model.User).get(id)
  if item is None:
    raise NoResultFound
  db.delete(item)
  db.commit()
