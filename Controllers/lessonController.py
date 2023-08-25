from typing import Annotated
from fastapi import Depends
from config import *
from Models import Lesson
from utils import *


@app.post("/lesson", status_code=201)
async def create_lesson(lesson_: Lesson, token: Annotated[str, Depends(oauth2_scheme)]):
    group_id(decode_token(token), cursor)
    lastId['Lesson'] += 1
    lesson_.id = lastId['Lesson']

    command = "INSERT INTO 'Lesson' (" + fields(lesson_) + ") VALUES (" + values(lesson_) + ")"
    cursor.execute(command)
    db.commit()
    return {"status": "Success", "detail": lesson_}


@app.patch("/lesson/{id_}")
async def update_lesson(lesson_: Lesson, id_: int, token: Annotated[str, Depends(oauth2_scheme)]):
    group_id(decode_token(token), cursor)
    cursor.execute(f"SELECT * FROM 'Lesson' WHERE id={id_}")
    if not cursor.fetchone():
        raise HTTPException(status_code=404, detail="Wrong id")
    command = "UPDATE 'Lesson' SET " + predicates(lesson_) + f" WHERE id = {id_}"
    cursor.execute(command)
    db.commit()
    return {"status": "Success"}
