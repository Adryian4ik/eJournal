from config import *
from fastapi import Depends
from utils import *
from dbModels import Student, Array
from typing import Annotated


@app.get("/student/{id_}")
async def get_student(id_: int):
    command = f"SELECT * FROM 'Student' WHERE id = {id_}\n"
    cursor.row_factory = sqlite3.Row
    cursor.execute(command)
    data = cursor.fetchone()
    if not data:
        raise HTTPException(status_code=400, detail="Некорректный id студента")
    return {"detail": data}


@app.post("/student", status_code=201)
async def create_student(student_: Student, token: Annotated[str, Depends(oauth2_scheme)]):
    check_access(check_token(token), cursor)

    cursor.execute(f"SELECT * FROM 'Student' WHERE id={student_.id}")
    if cursor.fetchone():
        raise HTTPException(status_code=409, detail="Студент с таким id уже существует")
    command = "INSERT INTO 'Student' (" + fields(student_) + ") VALUES (" + values(student_) + ")"
    cursor.execute(command)
    db.commit()
    return {"detail": "success", "student": student_}


@app.post("/students", status_code=201)
async def create_students(items: Array, token: Annotated[str, Depends(oauth2_scheme)]):
    checkId = check_access(check_token(token), cursor)

    if not checkId:
        raise HTTPException(status_code=403, detail="Нет доступа")

    errors = []
    for elem in dict(items)['array']:
        cursor.execute(f"SELECT * FROM 'Student' WHERE id={elem.id}")
        if cursor.fetchone():
            errors.append(elem.id)
        else:
            command = "INSERT INTO 'Student' (" + fields(elem) + ") VALUES (" + values(elem) + ")"
            cursor.execute(command)
    db.commit()
    if not errors:
        return {"detail": "Успешно создано"}
    else:
        return {"detail": f"Некоторые идентификаторы заняты: {str(errors)}"}


@app.patch("/student/")
async def update_student(student_: Student, token: Annotated[str, Depends(oauth2_scheme)]):
    checkId = check_access(check_token(token), cursor)

    cursor.execute(f"SELECT groupId FROM 'Student' WHERE id={student_.id}")
    checkData = cursor.fetchone()
    if not checkData:
        raise HTTPException(status_code=404, detail="Студента с таким id не существует")

    if checkId != checkData[0]:
        raise HTTPException(status_code=403, detail="Нет доступа")

    command = "UPDATE 'Student' SET " + predicates(student_) + f" WHERE id = {student_.id}"
    cursor.execute(command)
    db.commit()
    return {"detail": "Успешно обновлено"}


@app.patch("/students")
async def update_students(items: Array, token: Annotated[str, Depends(oauth2_scheme)]):
    checkId = check_access(check_token(token), cursor)

    errors, not_access = [], []
    for elem in dict(items)['array']:
        cursor.execute(f"SELECT groupId FROM 'Student' WHERE id={elem.id}")
        checkData = cursor.fetchone()[0]
        if not checkData:
            errors.append(elem.id)
        if checkId != checkData:
            not_access.append(elem.id)
            errors.append(elem.id)

        command = "UPDATE 'Student' SET " + predicates(elem) + f" WHERE id = {elem.id}"
        cursor.execute(command)
    db.commit()
    if not errors:
        return {"detail": "Успешно обновлено"}
    else:
        return {"detail": f"Не удалось обновить: {str(errors)}\n"
                          f"{f'Нет доступа к {str(not_access)}' if not_access else ''}"}


@app.delete("/student/")
async def delete_student(student_: Student, token: Annotated[str, Depends(oauth2_scheme)]):
    checkId = check_access(check_token(token), cursor)
    if checkId != student_.groupId:
        raise HTTPException(status_code=403, detail="Нет доступа")

    cursor.execute(f"DELETE FROM 'Student' WHERE id = {student_.id}")
    db.commit()
    return {"detail": "Успешно удалено"}


@app.delete("/students")
async def delete_students(items: Array, token: Annotated[str, Depends(oauth2_scheme)]):
    checkId = check_access(check_token(token), cursor)

    for elem in dict(items)['array']:
        if checkId == elem.groupId:
            cursor.execute(f"DELETE FROM 'Student' WHERE id = {elem.id}")
    db.commit()
    return {"detail": "Успешно удалено"}


@app.get("/groupinfo")
async def get_group_info(token: Annotated[str, Depends(oauth2_scheme)]):
    checkId = check_access(check_token(token), cursor)

    command = f"SELECT * FROM 'Student' WHERE groupId=={checkId}\n"
    cursor.row_factory = sqlite3.Row

    cursor.execute(command)
    data = cursor.fetchall()
    if len(data) == 0:
        raise HTTPException(status_code=404, detail="Нет студентов такой группы")
    return {"type": "answer", "data": data}
