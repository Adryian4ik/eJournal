from .mainModels import *


class Array(BaseModel):
    array: list[Student]


class Login(BaseModel):
    token: str
    id: int


class TgIdUpdate(BaseModel):
    id: int
    token: str
    tgId: int


class AddDocModel(BaseModel):
    start: str | None = None
    end: str | None = None
    passId: int | None = None
    docId: int | None = None
    type: int
