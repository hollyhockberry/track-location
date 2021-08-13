# location/router.py - API router for CRUD database
# Copyright (c) 2021 Inaba
# This software is released under the MIT License.
# http://opensource.org/licenses/mit-license.php

from fastapi import APIRouter, Depends, Response, HTTPException
from sqlalchemy.exc import NoResultFound
from sqlalchemy.orm import Session
from typing import List

from . import schemas, crud
from database.db import get_db

router = APIRouter(tags=['locations'])

@router.get('/',summary='Read all records',response_model=List[schemas.Location])
async def read_all(db: Session = Depends(get_db)):
  return crud.read_all(db)


@router.get('/{id}',response_model=schemas.Location,responses={404: {}})
async def read(id: int, db: Session = Depends(get_db)):
  location = crud.read(db, id)
  if location is None:
    raise HTTPException(status_code = 404)
  return location


@router.post('/',
description="""
The created record must have a unique combination of UUID, Major and Minor.

ID will be *automatically assigned*.
""",
response_model=schemas.Location,
responses={
  403: {'description': 'Duplicate combinations of UUID, Major and Minor'}
})
async def create(data: schemas.LocationCreate, db: Session = Depends(get_db)):
  try:
    return crud.create(db, data)
  except FileExistsError:
    raise HTTPException(status_code = 403)


@router.put('/{id}',
description=
'The updated record must have a unique combination of UUID and Major and Minor.',
status_code=204,
responses={
  403: {'description': 'Duplicate combinations of UUID, Major and Minor'},
  404: {}})
async def update(id: int, data: schemas.LocationUpdate, db: Session = Depends(get_db)):
  try:
    crud.update(db, id, data)
    return Response(status_code=204)
  except NoResultFound:
    raise HTTPException(status_code = 404)
  except FileExistsError:
    raise HTTPException(status_code = 403)


@router.delete('/{id}',status_code=204,responses={404: {}})
async def delete(id: int, db: Session = Depends(get_db)):
  try:
    crud.delete(db, id)
    return Response(status_code=204)
  except NoResultFound:
    raise HTTPException(status_code = 404)
