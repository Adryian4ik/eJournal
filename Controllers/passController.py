from config import *
from fastapi import Request
from dbModels import Pass
from utils import *


@app.post("/pass", status_code=201)
async def create_pass(pass_: Pass):
    cursor.execute(f"SELECT * FROM 'Pass' WHERE id={pass_.id}")
    if cursor.fetchone():
        raise HTTPException(status_code=409, detail="Пропуск с таким id уже существует")
    command = "INSERT INTO 'Pass' (" + fields(pass_) + ") VALUES (" + values(pass_) + ")"
    cursor.execute(command)
    db.commit()
    return {"detail": "Успешно добавлено", "pass": pass_}


@app.patch("/pass/{id_}")
async def update_pass(pass_: Pass, id_: int):
    cursor.execute(f"SELECT * FROM 'Pass' WHERE id={pass_.id}")
    if not cursor.fetchone():
        raise HTTPException(status_code=404, detail="Пропуска с таким id не существует")
    command = "UPDATE 'Pass' SET " + predicates(pass_) + f" WHERE id = {id_}"
    cursor.execute(command)
    db.commit()
    return {"detail": "Успешно обновлено"}


@app.delete("/pass/{id_}")
async def delete_pass(id_: int):
    cursor.execute(f"DELETE FROM 'Pass' WHERE id = {id_}")
    db.commit()
    return {"detail": "Успешно удалено"}




@app.get("/getpass/all/{studentId}")
async def get_all_pass(studentId: int):
    cursor.row_factory = sqlite3.Row
    cursor.execute(f"SELECT * FROM 'Pass' WHERE studentId={studentId}")
    return {"detail": "Успешно", "result": cursor.fetchall()}


@app.get("/getpass/month/{studentId}/{month}")
async def get_pass_for_a_month(studentId: int, month: int):
    cursor.row_factory = sqlite3.Row
    cursor.execute(f"SELECT * FROM 'Pass' WHERE studentId={studentId} AND month={month}")
    data = cursor.fetchall()
    return {"detail": "Успешно", "Количество пропусков": len(data), "result": data}


@app.get("/getpass/one/{studentId}/{id_}")
async def get_one_path(studentId: int, id_: int):
    cursor.row_factory = sqlite3.Row
    cursor.execute(f"SELECT * FROM 'Pass' WHERE studentId={studentId} AND id={id_}")
    return {"detail": cursor.fetchone()}


@app.get("/getpass/day/{studentId}")
async def get_one_path(studentId: int, req: Request):
    params = req.query_params
    if not (params.get("day", None) and params.get("month", None) and params.get("month", None)):
        raise HTTPException(status_code=400, detail="Необходимо указать дату (day, month, year)")
    cursor.row_factory = sqlite3.Row
    cursor.execute(f"SELECT * FROM 'Pass' WHERE studentId={studentId} AND day={params['day']} AND month={params['month']} AND year={params['year']}")
    return {"detail": cursor.fetchall()}
