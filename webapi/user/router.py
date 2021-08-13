# user/router.py - API router for CRUD database
# Copyright (c) 2021 Inaba
# This software is released under the MIT License.
# http://opensource.org/licenses/mit-license.php

from fastapi import APIRouter, Depends, Response, HTTPException
from sqlalchemy.exc import NoResultFound, IntegrityError
from sqlalchemy.orm import Session
from typing import List

from . import schemas, crud
from database.db import get_db

router = APIRouter(tags=['users'])

@router.get('/',summary='Read all records',response_model=List[schemas.User])
async def read_all(db: Session = Depends(get_db)):
  return crud.read_all(db)


@router.get('/{id}',response_model=schemas.User,responses={404: {}})
async def read(id: str, db: Session = Depends(get_db)):
  user = crud.read(db, id)
  if user is None:
    raise HTTPException(status_code = 404)
  return user


@router.post('/',
  summary='Read records with the same description',
  response_model=List[schemas.User])
async def read_by_description(data: schemas.UserRead, db: Session = Depends(get_db)):
  user = crud.read_by_description(db, data.description)
  if user is None:
    raise HTTPException(status_code = 404)
  return user


@router.post('/{id}',status_code=204,responses={400: {}})
async def create(id: str, data: schemas.UserCreate, db: Session = Depends(get_db)):
  try:
    crud.create(db, id, data)
    return Response(status_code=204)
  except IntegrityError:
    raise HTTPException(status_code = 400)


@router.put('/{id}', status_code=204,responses={404: {}})
async def update(id: str, data: schemas.UserUpdate, db: Session = Depends(get_db)):
  try:
    crud.update(db, id, data)
    return Response(status_code=204)
  except NoResultFound:
    raise HTTPException(status_code = 404)


@router.delete('/{id}', status_code=204,responses={404: {}})
async def delete(id: str, db: Session = Depends(get_db)):
  try:
    crud.delete(db, id)
    return Response(status_code=204)
  except NoResultFound:
    raise HTTPException(status_code = 404)
