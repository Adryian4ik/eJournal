import cryptography.fernet as Fernet
from fastapi import FastAPI
from fastapi.security import OAuth2PasswordBearer
from dbProcess import *


oauth2_scheme = OAuth2PasswordBearer(tokenUrl="token")
cipher_suite = Fernet.Fernet(b'3h64pUxWKFCRNgZ9hto2SfL6JwgmrWryRTIBEGfL3mU=')
lastId = {'Lesson': 0, 'Pass': 0, 'Schedule': 0}
app = FastAPI()
db, cursor = start_db()
