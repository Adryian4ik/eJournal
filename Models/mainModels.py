from pydantic import BaseModel


class Student(BaseModel):
    id: int
    tgId: int | None = None
    groupId: int
    lastname: str | None = None
    firstname: str | None = None
    patronymic: str | None = None


class Group(BaseModel):
    id: int
    bossId: int | None = None
    name: str


class Lesson(BaseModel):
    id: int
    name: str


class Pass(BaseModel):
    id: int
    studentId: int
    date: str
    scheduleId: int
    type: int
    documentId: int | None = None


class Schedule(BaseModel):
    id: int
    groupId: int
    lessonId: int
    dayOfWeek: int
    week: int
    numberOfLesson: int
    type: int


