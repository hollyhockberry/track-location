# beacon/router.py - API router for CRUD database
# Copyright (c) 2021 Inaba
# This software is released under the MIT License.
# http://opensource.org/licenses/mit-license.php

from fastapi import APIRouter, Depends, Response, HTTPException
from sqlalchemy.exc import NoResultFound, IntegrityError
from sqlalchemy.orm import Session
from typing import List

from . import schemas, crud
from database.db import get_db

router = APIRouter(tags=['beacons'])

@router.get('/',summary='Read all records',response_model=List[schemas.Beacon])
async def read(db: Session = Depends(get_db)):
  return crud.read(db)


@router.post('/{id}',
description=
'The created record must have a unique combination of UUID, Major and Minor.',
status_code=204,
responses={
  403: {'description': 'Duplicate combinations of UUID, Major and Minor'},
  400: {}
})
async def create(id: str, data: schemas.BeaconCreate, db: Session = Depends(get_db)):
  try:
    crud.create(db, id, data)
    return Response(status_code=204)
  except IntegrityError:
    raise HTTPException(status_code = 400)
  except FileExistsError:
    raise HTTPException(status_code = 403)


@router.put('/{id}',
description=
'The updated record must have a unique combination of UUID and Major and Minor.',
status_code=204,
responses={
  403: {'description': 'Duplicate combinations of UUID, Major and Minor'},
  404: {}})
async def update(id: str, data: schemas.BeaconUpdate, db: Session = Depends(get_db)):
  try:
    crud.update(db, id, data)
    return Response(status_code=204)
  except NoResultFound:
    raise HTTPException(status_code = 404)
  except FileExistsError:
    raise HTTPException(status_code = 403)


@router.delete('/{id}',status_code=204,responses={404: {}})
async def delete(id: str, db: Session = Depends(get_db)):
  try:
    crud.delete(db, id)
    return Response(status_code=204)
  except NoResultFound:
    raise HTTPException(status_code = 404)
