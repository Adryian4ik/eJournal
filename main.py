from fastapi import Request
from config import *
import Controllers
from utils import *


@app.on_event("startup")
async def startup_event():
    print("Приложение запущено! Выполняем начальную инициализацию...")


@app.get("/")
async def api_data(request: Request):
    params = request.query_params
    if params.get("command", None):
        cursor.row_factory = sqlite3.Row
        print(str(params["command"]))
        cursor.execute(str(params["command"]))
        db.commit()
        return cursor.fetchall()
    return "error"


@app.on_event("shutdown")
async def shutdown_event():
    close(db, cursor)


@app.get("/isdatavalid")
async def is_elder(request: Request):
    params = request.query_params
    if params.get("id", None) and params.get("code", None):
        group = cipher_suite.decrypt(params["code"]).decode()
        cursor.row_factory = sqlite3.Row
        print(group)
        cursor.execute("SELECT * FROM 'Group' WHERE bossId=={0} AND name==\"{1}\"".format(params["id"], group))
        db.commit()
        db_answer = cursor.fetchall()
        print(db_answer)
        if len(db_answer) != 0:
            return {"type": "answer", "answer": True, "groupName": db_answer[0]["name"], "groupId": db_answer[0]["id"]}
    return {"type": "error"}


@app.get("/getgroupinfo")
async def get_group_info(request: Request):
    params = request.query_params
    if params.get("groupId", None):
        gId = int(params["groupId"])
        command = f"SELECT * FROM 'Student' WHERE groupId=={gId}\n"
        cursor.row_factory = sqlite3.Row
        cursor.execute(command)
        data = cursor.fetchall()
        if len(data) == 0:
            raise HTTPException(status_code=404, detail="Нет студентов такой группы")
        print(data)
        return {"type": "answer", "data": data}
    raise HTTPException(status_code=400, detail="Не указана группа")
