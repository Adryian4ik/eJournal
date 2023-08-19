from config import *
from fastapi import Depends
from utils import *
from Models import Student, Array, Login, TgIdUpdate
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


@app.get("/studentTg/{id_}")
async def get_student(id_: int):
    command = f"SELECT * FROM 'Student' WHERE tgId = {id_}\n"
    cursor.row_factory = sqlite3.Row
    cursor.execute(command)
    data = cursor.fetchone()
    if not data:
        raise HTTPException(status_code=400, detail="Некорректный tgId студента")
    return {"detail": data}


@app.post("/student", status_code=201)
async def create_student(student_: Student, token: Annotated[str, Depends(oauth2_scheme)]):
    bossGroupId = group_id(decode_token(token), cursor)
    student_.groupId = bossGroupId
    cursor.execute(f"SELECT * FROM 'Student' WHERE id={student_.id}")
    if cursor.fetchone():
        raise HTTPException(status_code=409, detail="Студент с таким id уже существует")
    command = "INSERT INTO 'Student' (" + fields(student_) + ") VALUES (" + values(student_) + ")"
    cursor.execute(command)
    db.commit()
    return {"detail": "success", "student": student_}


@app.post("/students", status_code=201)
async def create_students(items: Array, token: Annotated[str, Depends(oauth2_scheme)]):
    bossGroupId = group_id(decode_token(token), cursor)

    errors = []
    for student_ in dict(items)['array']:
        cursor.execute(f"SELECT * FROM 'Student' WHERE id={student_.id}")
        if cursor.fetchone():
            errors.append(student_.id)
        else:
            student_.groupId = bossGroupId
            command = "INSERT INTO 'Student' (" + fields(student_) + ") VALUES (" + values(student_) + ")"
            cursor.execute(command)
    db.commit()
    if not errors:
        return {"detail": "Успешно создано"}
    else:
        return {"detail": f"Некоторые идентификаторы заняты: {str(errors)}"}


@app.patch("/student/")
async def update_student(student_: Student, token: Annotated[str, Depends(oauth2_scheme)]):
    bossGroupId = group_id(decode_token(token), cursor)
    student_.groupId = bossGroupId

    cursor.execute(f"SELECT groupId FROM 'Student' WHERE id={student_.id}")
    checkId = cursor.fetchone()[0]
    if not checkId:
        raise HTTPException(status_code=404, detail="Студента с таким id не существует")

    if bossGroupId != checkId[0]:
        raise HTTPException(status_code=403, detail="Нет доступа")

    command = "UPDATE 'Student' SET " + predicates(student_) + f" WHERE id = {student_.id}"
    cursor.execute(command)
    db.commit()
    return {"message": "Успешно обновлено", "detail": student_}


@app.patch("/students/")
async def update_students(items: Array, token: Annotated[str, Depends(oauth2_scheme)]):
    bossGroupId = group_id(decode_token(token), cursor)

    errors, not_access = [], []
    for student in dict(items)['array']:
        student.groupId = bossGroupId
        cursor.execute(f"SELECT groupId FROM 'Student' WHERE id={student.id}")
        checkId = cursor.fetchone()[0]
        if not checkId:
            errors.append(student.id)
        elif bossGroupId != checkId:
            not_access.append(student.id)
            errors.append(student.id)

        command = "UPDATE 'Student' SET " + predicates(student) + f" WHERE id = {student.id}"
        cursor.execute(command)
    db.commit()
    if not errors:
        return {"message": "Успешно обновлено", "detail": items}
    else:
        return {"detail": f"Не удалось обновить: {str(errors)}\n"
                          f"{f'Нет доступа к {str(not_access)}' if not_access else ''}"}


@app.post("/student/tg/")
async def set_tgId(data: TgIdUpdate):
    groupId = decode_token(data.token)
    cursor.execute(f"SELECT * FROM 'Student' WHERE groupId={groupId} AND id={data.id}")
    if not cursor.fetchone():
        raise HTTPException(status_code=400, detail="Incorrect token or id")

    cursor.execute(f"update Student set tgId={data.tgId} where id={data.id}")
    db.commit()
    return {"detail": "TgId успешно добавлен"}


@app.delete("/student/")
async def delete_student(student_: Student, token: Annotated[str, Depends(oauth2_scheme)]):
    bossGroupId = group_id(decode_token(token), cursor)

    cursor.execute(f"SELECT groupId From 'Student' WHERE id = {student_.id}")
    if bossGroupId != cursor.fetchone()[0]:
        raise HTTPException(status_code=403, detail="Нет доступа")

    cursor.execute(f"DELETE FROM 'Student' WHERE id = {student_.id}")
    db.commit()
    return {"detail": "Успешно удалено"}


@app.delete("/students")
async def delete_students(items: Array, token: Annotated[str, Depends(oauth2_scheme)]):
    bossGroupId = group_id(decode_token(token), cursor)

    for student_ in dict(items)['array']:
        cursor.execute(f"SELECT groupId From 'Student' WHERE id = {student_.id}")
        if bossGroupId == cursor.fetchone()[0]:
            cursor.execute(f"DELETE FROM 'Student' WHERE id = {student_.id}")
    db.commit()
    return {"detail": "Успешно удалено"}


@app.get("/groupinfo")
async def get_group_info(token: Annotated[str, Depends(oauth2_scheme)]):
    bossGroupId = group_id(decode_token(token), cursor)

    command = f"SELECT * FROM 'Student' WHERE groupId=={bossGroupId}\n"
    cursor.row_factory = sqlite3.Row

    cursor.execute(command)
    data = cursor.fetchall()
    if len(data) == 0:
        raise HTTPException(status_code=404, detail="Нет студентов такой группы")
    return {"detail": "success", "students": data}


@app.post("/auth/login")
async def authorization(login_: Login):
    decoded = decode_token(login_.token)
    cursor.execute(f"SELECT id, name, bossId FROM 'Group' WHERE name='{decoded}'")
    data = cursor.fetchone()
    if data and data[2] == login_.id:
        return {
            "detail": "success",
            "groupId": data[0],
            "token": login_.token,
            "groupName": data[1],
            "bossId": login_.id
        }
    else:
        raise HTTPException(status_code=401, detail="Error")


@app.get("/auth/me")
async def get_me(token: Annotated[str, Depends(oauth2_scheme)]):
    decoded = decode_token(token)
    cursor.execute(f"SELECT id, bossId FROM 'Group' WHERE name='{decoded}'")
    data = cursor.fetchone()
    if not data:
        raise HTTPException(status_code=401, detail="Error")
    else:
        return {
            "detail": "success",
            "groupId": data[0],
            "token": token,
            "groupName": decoded,
            "bossId": data[1]
        }

