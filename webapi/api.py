# Copyright (c) 2021 Inaba
# This software is released under the MIT License.
# http://opensource.org/licenses/mit-license.php

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

app = FastAPI(
  title='Tracker Backend',
  version='0.0.0',
  license_info={
      'name': 'MIT License',
      'url': 'http://opensource.org/licenses/mit-license.php'
  },
  docs_url='/api/docs',
  redoc_url='/api/redoc',
  openapi_url='/api/openapi.json',
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

if __name__ == "__main__":
  import uvicorn
  uvicorn.run("api:app", host="0.0.0.0", port=1234, reload=True)
