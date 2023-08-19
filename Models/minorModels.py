from .mainModels import *


class Array(BaseModel):
    array: list[Student]


class Login(BaseModel):
    token: str
    groupName: str


class PassModel(BaseModel):
    start: str
    end: str


class TgIdUpdate(BaseModel):
    id: int
    token: str
    tgId: int


class AddDocModel(BaseModel):
    start: str | None = None
    end: str | None = None
    date: str | None = None
    scheduleId: int | None = None
    docId: int
    type: int
