# search/router.py - API router for CRUD database
# Copyright (c) 2021 Inaba
# This software is released under the MIT License.
# http://opensource.org/licenses/mit-license.php

from fastapi import APIRouter, Depends, HTTPException
import influxdb
from influxdb.exceptions import InfluxDBClientError
from sqlalchemy.orm import Session
from typing import List
from datetime import datetime
from dateutil import tz

from . import schemas
from user import crud as user
from location import crud as location
from database.db import get_db

router = APIRouter(tags=['search'])

client = influxdb.InfluxDBClient(
  host='localhost',
  port=8086,
  database='track_location')

measurement = 'sample'
default_period = '10m'

def get_usename(db: Session, userid: str):
  u = user.read(db, userid)
  return u.name if u != None else None


def get_location(db: Session, uuid: str, major: int, minor: int):
  l = location.read_by_uuids(db, uuid, major, minor)
  return l.location if l != None else None


def point_to_dict(db: Session, point, userid: str):
  if type(point) is list:
    return point_to_dict(db, point[0], userid) if point != [] else None

  name = get_usename(db, userid)
  location = get_location(db, point['uuid'], point['major'], point['minor'])

  return schemas.Point(
    time= point['time'],
    name= name,
    location= location) if name != None and location != None else None


def today_am00():
  today = datetime.today()
  utc = datetime(today.year,today.month,today.day,
    tzinfo=tz.gettz('Asia/Tokyo')).astimezone(tz.gettz('UTC'))
  return f"{utc:%Y-%m-%dT%H:%S:%MZ}"


def query_period(period: str):
  if period == 'today':
    return f'time >= \'{today_am00()}\''
  return f'time > now() - {period}'


def resultset_to_list(db: Session, results: influxdb.resultset.ResultSet):
  points = [point_to_dict(db, list(results[key]), key['user'])
              for _, key in results.keys()]
  return [point for point in points if point is not None]

period_description = """

__How to specify period?__

"__period__" specifies the measurement period.  
It is specified by the InfluxQL duration literal _(e.g. 5m 2d)_
 and "__today__".  
The "__today__" specifies the period from 0:00 AM (JST) to the present day.

For more information on duration literals:  
https://docs.influxdata.com/influxdb/v1.8/query_language/spec/#durations
"""

@router.get('/',
  summary=' ',
  description=f'Get a list of the last positions of all users in the last {default_period}.',
  response_model=List[schemas.Point])
async def search_default(db: Session = Depends(get_db)):
  return await search(default_period, db)

responses = {
  400: { 'description': 'InfluxDBClientError' }
}

@router.get('/{period}',
  summary=' ',
  description='Get a list of locations of all users for a specified period of time.' + period_description,
  response_model=List[schemas.Point],
  responses=responses)
async def search(period: str, db: Session = Depends(get_db)):
  try:
    sql = f'SELECT * FROM {measurement} '\
      'WHERE '\
      f'{query_period(period)} '\
      'GROUP BY "user" '\
      'ORDER BY time DESC '\
      'LIMIT 1'
    return resultset_to_list(db, client.query(sql))
  except InfluxDBClientError:
    raise HTTPException(status_code = 400, detail='InfluxDBClientError')


@router.get('/byuser/{id}',
  summary=' ',
  description='Get the last position of the specified user.' + period_description,
  response_model=schemas.Point,
  responses={**responses, 404: {}})
async def get_byuser(
  id: str, period: str = default_period, db: Session = Depends(get_db)):
  try:
    sql = f'SELECT * FROM {measurement} '\
      'WHERE '\
      f'{query_period(period)} AND '\
      f'"user"=\'{id}\' '\
      'GROUP BY "user" '\
      'ORDER BY time DESC '\
      'LIMIT 1'
    l = resultset_to_list(db, client.query(sql))
    if l == []:
      raise HTTPException(status_code = 404)
    return l[0]
  except InfluxDBClientError:
    raise HTTPException(status_code = 400, detail='InfluxDBClientError')


@router.post('/byuserdescription',
  summary=' ',
  description='Get the last location of the user with the same "description".'\
    + period_description,
  response_model=List[schemas.Point],
  responses=responses)
async def post_byuser(
  data: schemas.UserDescription, period: str = default_period,
  db: Session = Depends(get_db)):
  try:
    users = [u.id for u in user.read_by_description(db, data.description)]
    sql = f'SELECT * FROM {measurement} '\
      'WHERE '\
      f'{query_period(period)} '\
      'GROUP BY "user" '\
      'ORDER BY time DESC '\
      'LIMIT 1'
    r = client.query(sql)
    points = [point_to_dict(db, list(r[key]), key['user'])
                for _, key in r.keys() if key['user'] in users]
    return [point for point in points if point is not None]
  except InfluxDBClientError:
    raise HTTPException(status_code = 400, detail='InfluxDBClientError')


@router.get('/bylocation/{id}',
  summary=' ',
  description='Get a list of users near a specified location.'\
    + period_description,
  response_model=List[schemas.Point],
  responses=responses)
async def get_bylocation(
  id: int, period: str = default_period, db: Session = Depends(get_db)):
  try:
    l = location.read(db, id)
    if l is None:
      raise HTTPException(status_code = 404)

    sql = f'SELECT * FROM {measurement} '\
      'WHERE '\
      f'{query_period(period)} AND '\
      f'"uuid"=\'{l.uuid}\' AND '\
      f'"major"={l.major} AND '\
      f'"minor"={l.minor} '\
      'GROUP BY "user" '\
      'ORDER BY time DESC '\
      'LIMIT 1'
    return resultset_to_list(db, client.query(sql))
  except InfluxDBClientError:
    raise HTTPException(status_code = 400, detail='InfluxDBClientError')
