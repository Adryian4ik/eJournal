from typing import Annotated

from config import *
from fastapi import Depends
from Models import Pass, PassModel, AddDocModel
from utils import *


@app.post("/pass", status_code=201)
async def create_pass(pass_: Pass, token: Annotated[str, Depends(oauth2_scheme)]):
    group_id(decode_token(token), cursor)
    cursor.execute(f"SELECT * FROM 'Pass' WHERE id={pass_.id}")
    if cursor.fetchone():
        raise HTTPException(status_code=409, detail="Пропуск с таким id уже существует")
    command = "INSERT INTO 'Pass' (" + fields(pass_) + ") VALUES (" + values(pass_) + ")"
    cursor.execute(command)
    db.commit()
    return {"detail": "Успешно добавлено", "pass": pass_}


@app.patch("/pass/{studentId_}")
async def add_doc(studentId_: int, data: AddDocModel, token: Annotated[str, Depends(oauth2_scheme)]):
    group_id(decode_token(token), cursor)
    if not (data.start and data.end) and not (data.date and data.scheduleId):
        raise HTTPException(status_code=400, detail="Некорректные данные")
    if data.start and data.end:
        command = (f"update Pass "
                   f"set type={data.type}, documentId={data.docId} "
                   f"where '{data.start}' <= date AND date <= '{data.end}' and "
                   f"studentId={studentId_}")
    else:
        command = (f"update Pass "
                   f"set type={data.type}, documentId={data.docId} "
                   f"where date='{data.date}' and scheduleId={data.scheduleId} and "
                   f"studentId={studentId_}")
    cursor.execute(command)
    db.commit()
    return {"detail": "success"}


@app.delete("/pass/{id_}")
async def delete_pass(id_: int, token: Annotated[str, Depends(oauth2_scheme)]):
    bossGroupId = group_id(decode_token(token), cursor)
    cursor.execute(f"SELECT Student.groupId FROM 'Pass' JOIN Student ON Pass.studentId = Student.id "
                   f"WHERE Pass.id={id_}")
    checkId = cursor.fetchone()[0]
    if not checkId or bossGroupId != checkId:
        raise HTTPException(status_code=403, detail="Нет доступа")
    cursor.execute(f"DELETE FROM 'Pass' WHERE id = {id_}")
    db.commit()
    return {"detail": "Успешно удалено"}


@app.get("/pass/{studentId}/{id_}")
async def get_one_pass(studentId: int, id_: int):
    cursor.row_factory = sqlite3.Row
    cursor.execute(f"select Pass.id, S.lastname, S.firstname, S.patronymic, S.tgId, "
                   f"Pass.date, S2.numberOfLesson, L.name, Pass.type, Pass.documentId "
                   f"from Pass "
                   f"join pass_type pt on pass.type = pt.id "
                   f"join Student S on pass.studentId = S.id "
                   f"join Schedule S2 on pass.scheduleId = S2.id "
                   f"join main.Lesson L on S2.lessonId = L.id "
                   f"WHERE studentId={studentId} AND Pass.id={id_}")
    return {"detail": cursor.fetchone()}


@app.get("/pass/")
async def get_pass_by_period(passModel: PassModel):
    command = (f"select Pass.id, S.lastname, S.firstname, S.patronymic, S.tgId, "
               f"Pass.date, S2.numberOfLesson, L.name, pt.name type, Pass.documentId "
               f"from Pass "
               f"join pass_type pt on pass.type = pt.id "
               f"join Student S on pass.studentId = S.id "
               f"join Schedule S2 on pass.scheduleId = S2.id "
               f"join main.Lesson L on S2.lessonId = L.id "
               f"where '{passModel.start}' <= date AND date <= '{passModel.end}'")

    cursor.row_factory = sqlite3.Row
    cursor.execute(command)
    data = cursor.fetchall()
    return {"count": len(data), "detail": data}
