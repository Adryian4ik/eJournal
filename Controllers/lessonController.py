from config import *
from Models import Lesson
from utils import *


@app.post("/lesson", status_code=201)
async def create_lesson(lesson_: Lesson):
    cursor.execute(f"SELECT * FROM 'Lesson' WHERE id={lesson_.id}")
    if cursor.fetchone():
        raise HTTPException(status_code=409, detail="Пара с таким id уже существует")
    command = "INSERT INTO 'Lesson' (" + fields(lesson_) + ") VALUES (" + values(lesson_) + ")"
    cursor.execute(command)
    db.commit()
    return {"detail": "Успешно добавлено", "lesson": lesson_}


@app.patch("/lesson/{id_}")
async def update_lesson(lesson_: Lesson, id_: int):
    cursor.execute(f"SELECT * FROM 'Lesson' WHERE id={id_}")
    if not cursor.fetchone():
        raise HTTPException(status_code=404, detail="Пары с таким id не существует")
    command = "UPDATE 'Lesson' SET " + predicates(lesson_) + f" WHERE id = {id_}"
    cursor.execute(command)
    db.commit()
    return {"detail": "Успешно обновлено"}

