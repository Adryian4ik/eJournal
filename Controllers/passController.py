from typing import Annotated

from config import *
from fastapi import Depends, Request
from Models import Pass, AddDocModel
from utils import *


@app.post("/pass", status_code=201)
async def create_pass(pass_: Pass, token: Annotated[str, Depends(oauth2_scheme)]):
    groupBossId = group_id(decode_token(token), cursor)
    cursor.execute(f"select groupId from student "
                   f"where id={pass_.studentId}")
    checkId = cursor.fetchone()[0]
    if not checkId:
        raise HTTPException(status_code=404)
    if checkId != groupBossId:
        raise HTTPException(status_code=403)
    lastId['Pass'] += 1
    pass_.id = lastId['Pass']
    command = "INSERT INTO 'Pass' (" + fields(pass_) + ") VALUES (" + values(pass_) + ")"
    cursor.execute(command)
    db.commit()
    return {"status": "Success", "detail": pass_}


@app.patch("/pass/{studentId_}")
async def add_document(studentId_: int, data: AddDocModel, token: Annotated[str, Depends(oauth2_scheme)]):
    group_id(decode_token(token), cursor)
    if not (data.start and data.end) and not (data.date and data.scheduleId):
        raise HTTPException(status_code=400, detail="Incorrect data")

    command = (f"update Pass set type={data.type}" +
               (f", documentId={data.docId}" if data.docId else "") +
               (f" where '{data.start}' <= date AND date <= '{data.end}' " if data.start and data.end else
                f" where id='{data.passId}' ") +
               f"and studentId={studentId_}")
    cursor.execute(command)
    db.commit()
    return {"status": "Success"}


@app.delete("/pass/{id_}")
async def delete_pass(id_: int, token: Annotated[str, Depends(oauth2_scheme)]):
    bossGroupId = group_id(decode_token(token), cursor)
    cursor.execute(f"SELECT Student.groupId FROM 'Pass' JOIN Student ON Pass.studentId = Student.id "
                   f"WHERE Pass.id={id_}")
    checkId = cursor.fetchone()[0]
    if not checkId or bossGroupId != checkId:
        raise HTTPException(status_code=403)
    cursor.execute(f"DELETE FROM 'Pass' WHERE id = {id_}")
    db.commit()
    return {"status": "Success"}


@app.get("/pass/{studentId}")
async def get_pass(studentId: int, req: Request):
    params = dict(req.query_params)
    if not (params.get('start') and params.get('end')) and not params.get('id'):
        raise HTTPException(status_code=400, detail="Enter ('start' and 'end') or 'id' params")

    cursor.row_factory = sqlite3.Row
    command = (f"select Pass.id, Pass.studentId, S.lastname, S.firstname, S.patronymic, S.tgId, "
               f"Pass.date, S2.numberOfLesson, L.name LessonName, Pass.type typeId, pt.name type, Pass.documentId "
               f"from Pass "
               f"join pass_type pt on pass.type = pt.id "
               f"join Student S on pass.studentId = S.id "
               f"join Schedule S2 on pass.scheduleId = S2.id "
               f"join Lesson L on S2.lessonId = L.id "
               f"WHERE Pass.studentId={studentId} ")
    if params.get('id'):
        command += f"and Pass.id={params['id']}"
    else:
        command += f"and '{params['start']}' <= date and date <= '{params['end']}'"

    cursor.execute(command)
    return {"detail": cursor.fetchall()}


@app.get("/pass")
async def get_pass_by_period(req: Request, token: Annotated[str, Depends(oauth2_scheme)]):
    bossGroupId = group_id(decode_token(token), cursor)
    params = dict(req.query_params)
    if not params.get('start') or not params.get('end'):
        raise HTTPException(status_code=400, detail="Enter 'start' and 'end' params")
    start, end = params['start'], params['end']

    if not params.get('groupId') and not params.get('studentId'):
        raise HTTPException(status_code=400, detail="Enter 'groupId' or 'studentId' parameter")

    if params.get('groupId') and bossGroupId != int(params['groupId']):
        raise HTTPException(status_code=403)

    command = (f"select Pass.id, Pass.studentId, S.lastname, S.firstname, S.patronymic, S.tgId, "
               f"Pass.date, S2.numberOfLesson, L.name LessonName, Pass.type typeId, pt.name type, Pass.documentId "
               f"from Pass "
               f"join pass_type pt on pass.type = pt.id "
               f"join Student S on pass.studentId = S.id "
               f"join Schedule S2 on pass.scheduleId = S2.id "
               f"join Lesson L on S2.lessonId = L.id "
               f"where '{start}' <= date and date <= '{end}' ")
    command = command + (f"and S.groupId={params['groupId']} " if params.get('groupId') else
                         (f"and Pass.studentId={params['studentId']}" if params.get('studentId') else ""))
    cursor.row_factory = sqlite3.Row
    cursor.execute(command)
    data = cursor.fetchall()
    return {"count": len(data), "detail": data}
