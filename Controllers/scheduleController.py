from typing import Annotated
from fastapi import Depends
from config import *
from Models import Schedule
from utils import *


@app.get("/schedule/{groupId_}")
async def get_schedule(groupId_: int):
    command = (f"SELECT S.id, L.name LessonName, week.name week, day.name day, "
               f"S.numberOfLesson, lt.name LessonType, S.subgroup, S.dates "
               f"FROM Schedule S "
               f"join lesson_type lt on lt.id = S.type "
               f"join Lesson L on L.id = S.lessonId "
               f"join day on S.dayOfWeek = day.id "
               f"join week on S.week = week.id "
               f"WHERE groupId={groupId_} "
               f"ORDER BY week, dayOfWeek, numberOfLesson, subgroup ")
    cursor.execute(command)
    cursor.row_factory = sqlite3.Row
    data = cursor.fetchall()

    result = {"Верхняя": {"Понедельник": [], "Вторник": [], "Среда": [], "Четверг": [],
                          "Пятница": [], "Суббота": [], "Воскресение": []},
              "Нижняя": {"Понедельник": [], "Вторник": [], "Среда": [], "Четверг": [],
                         "Пятница": [], "Суббота": [], "Воскресение": []}}
    for elem in data:
        elem = dict(elem)
        if elem['dates']:
            elem['dates'] = elem['dates'].split(',')
        if (len(result[elem['week']][elem['day']]) > 0 and
                not isinstance(result[elem['week']][elem['day']][-1], list) and
                result[elem['week']][elem['day']][-1]['numberOfLesson'] == elem['numberOfLesson']):
            result[elem['week']][elem['day']][-1] = [result[elem['week']][elem['day']][-1], elem]
        else:
            result[elem['week']][elem['day']].append(elem)

    return {"detail": result}


@app.post("/schedule")
async def create_schedule(schedule_: Schedule, token: Annotated[str, Depends(oauth2_scheme)]):
    bossGroupId = group_id(decode_token(token), cursor)
    lastId['Schedule'] += 1

    schedule_.groupId = bossGroupId
    if isinstance(schedule_.dates, list):
        schedule_.dates = ",".join(schedule_.dates)
    schedule_.id = lastId['Schedule']

    command = f"INSERT INTO 'Schedule' ({fields(schedule_)}) VALUES ({values(schedule_)})"
    cursor.execute(command)
    db.commit()
    return {"detail": schedule_}


@app.patch("/schedule/{id_}")
async def update_lesson(id_: int, schedule_: Schedule,
                        token: Annotated[str, Depends(oauth2_scheme)]):
    bossGroupId = group_id(decode_token(token), cursor)

    schedule_.id, schedule_.groupId = id_, bossGroupId
    if isinstance(schedule_.dates, list):
        schedule_.dates = ",".join(schedule_.dates)

    cursor.execute(f"SELECT groupId FROM Schedule WHERE id={id_}")
    checkId = cursor.fetchone()[0]
    if not checkId:
        raise HTTPException(status_code=404, detail="Wrong Id")
    elif checkId != bossGroupId:
        raise HTTPException(status_code=403, detail="Нет доступа")

    command = "UPDATE 'Schedule' SET " + predicates(schedule_) + f" WHERE id = {id_}"
    cursor.execute(command)
    db.commit()
    return {"detail": schedule_}


@app.delete("/schedule/{id_}")
async def delete_lesson(id_: int,
                        token: Annotated[str, Depends(oauth2_scheme)]):
    bossGroupId = group_id(decode_token(token), cursor)
    cursor.execute(f"SELECT groupId FROM 'Schedule' WHERE id={id_}")
    checkId = cursor.fetchone()[0]
    if checkId != bossGroupId:
        raise HTTPException(status_code=403, detail="Нет доступа")
    cursor.execute(f"DELETE FROM 'Schedule' WHERE id={id_}")
    db.commit()
    return {"status": "success"}
