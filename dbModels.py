from pydantic import BaseModel


class Test(BaseModel):
    name: str
    description: str | None = None
    price: float
    tax: float | None = None
    test: list | None = None


class Student(BaseModel):
    id: int
    tgId: int | None = None
    groupId: int | None = None
    lastname: str | None = None
    firstname: str | None = None
    patronymic: str | None = None


class Group(BaseModel):
    id: int
    bossId: int | None = None
    name: str | None = None


class Lesson(BaseModel):
    id: int
    teacherId: int | None = None
    name: str | None = None
    semester: int | None = None
    type: int | None = None


class Pass(BaseModel):
    id: int
    studentId: int | None = None
    day: int | None = None
    month: int | None = None
    year: int | None = None
    lessonNumber: int | None = None
    lessonId: int | None = None
    type: int | None = None
    documentId: int | None = None


class Schedule(BaseModel):
    groupId: int | None = None
    lessonId: int | None = None
    dayOfWeek: int | None = None
    week: int | None = None
    numberOfLesson: int | None = None


class Array(BaseModel):
    array: list[Student]
