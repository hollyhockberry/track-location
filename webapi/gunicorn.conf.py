from os import getenv
from multiprocessing import cpu_count

bind = '0.0.0.0:' + str(getenv('PORT', 8000))
worker_class = 'uvicorn.workers.UvicornWorker'
workers = cpu_count()

loglevel = "debug"
accesslog = None
