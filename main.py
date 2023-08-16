from dbProcess import *
from dbModels import *
from utils import *
import fastapi
import cryptography.fernet as Fernet

db, cursor = start_db()

cipher_suite = Fernet.Fernet( b'3h64pUxWKFCRNgZ9hto2SfL6JwgmrWryRTIBEGfL3mU=')
app = fastapi.FastAPI()


@app.on_event("startup")
async def startup_event():
    print("Приложение запущено! Выполняем начальную инициализацию...")


@app.get("/")
async def api_data(request: fastapi.Request):
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
async def is_elder(request: fastapi.Request):
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
async def api_data(request: fastapi.Request):
    params = request.query_params
    if params.get("groupId", None):
        gId = int(params["groupId"])
        cursor.row_factory = sqlite3.Row
        cursor.execute("""SELECT * FROM Student WHERE groupId=={0}""".format(gId))
        data = cursor.fetchall()
        # db.commit()
        print(data)
        return {"type": "answer", "data": data}
    return {"type": "error"}


@app.post("/student")
async def create_student(student_: Student):
    command = 'INSERT INTO "Student" (' + fields(student_) + ') VALUES (' + values(student_) + ')'
    print(command)
    # cursor.execute(command)
    # db.commit()
    return {"msg": "success", "student": student_}


@app.post("/students")
async def create_students(items: Array):
    for elem in dict(items)['array']:
        # print(elem)
        command = 'INSERT INTO "Student" (' + fields(elem) + ') VALUES (' + values(elem) + ')'
        # print(command)
        cursor.execute(command)
    db.commit()
    return {"msg": "success"}


@app.patch("/student/{id}")
async def update_student(student_: Student, id: int):
    command = 'UPDATE "Student" SET ' + predicates(student_) + f' WHERE id = {id}'
    print(command)
    cursor.execute(command)
    db.commit()
    return {"msg": "success"}


@app.patch("/students")
async def update_students(items: Array):
    for elem in dict(items)['array']:
        command = 'UPDATE "Student" SET ' + predicates(elem) + f' WHERE id = {elem.id}'
        # print(command)
        cursor.execute(command)
    db.commit()
    return {"msg": "success"}


@app.delete("/student/{id}")
async def delete_student(id: int):
    cursor.execute(f'DELETE FROM "Student" WHERE id = {id}')
    db.commit()
    return {"msg": "success"}


@app.delete("/students")
async def delete_students(items: Array):
    for elem in dict(items)['array']:
        cursor.execute(f'DELETE FROM "Student" WHERE id = {elem.id}')
    db.commit()
    return {"msg": "success"}

@app.get("/lesson/today")
async def get_lessons(req : fastapi.Request):
    params = req.query_params


@app.post("/lesson")
async def create_lesson(lesson_: Lesson):

    command = 'INSERT INTO "Lesson" (' + fields(lesson_) + ') VALUES (' + values(lesson_) + ')'
    print(command)
    cursor.execute(command)
    db.commit()
    return {"msg": "success", "lesson": lesson_}


@app.patch("/lesson/{id}")
async def update_lesson(lesson_: Lesson, id: int):
    command = 'UPDATE "Lesson" SET ' + predicates(lesson_) + f' WHERE id = {id}'
    print(command)
    cursor.execute(command)
    db.commit()
    return {"msg": "success"}


@app.delete("/lesson/{id}")
async def delete_lesson(id: int):
    cursor.execute(f'DELETE FROM "Lesson" WHERE id = {id}')
    db.commit()
    return {"msg": "success"}


@app.post("/pass")
async def create_pass(pass_: Pass):

    command = 'INSERT INTO "Pass" (' + fields(pass_) + ') VALUES (' + values(pass_) + ')'
    print(command)
    cursor.execute(command)
    db.commit()
    return {"msg": "success", "pass": pass_}


@app.patch("/pass/{id}")
async def update_pass(pass_: Pass, id: int):
    command = 'UPDATE "Pass" SET ' + predicates(pass_) + f' WHERE id = {id}'
    print(command)
    cursor.execute(command)
    db.commit()
    return {"msg": "success"}


@app.delete("/pass/{id}")
async def delete_pass(id: int):
    cursor.execute(f'DELETE FROM "Pass" WHERE id = {id}')
    db.commit()
    return {"msg": "success"}


@app.post("/group")
async def create_group(group_: Group):
    command = 'INSERT INTO "Group" (' + fields(group_) + ') VALUES (' + values(group_) + ')'
    print(command)
    cursor.execute(command)
    db.commit()
    return {"msg": "success", "group": group_}


@app.patch("/group/{id}")
async def update_group(group_: Group, id: int):
    command = 'UPDATE "Group" SET ' + predicates(group_) + f' WHERE id = {id}'
    print(command)
    cursor.execute(command)
    db.commit()
    return {"msg": "success"}


@app.delete("/group/{id}")
async def delete_group(id: int):
    cursor.execute(f'DELETE FROM "Group" WHERE id = {id}')
    db.commit()
    return {"msg": "success"}



@app.post("/test")
async def test(item: Array):
    temp = dict(item)
    for key in temp:
        print(key, temp[key])
    return item


@app.get("/getallpass/{studentId}")
async def get_all_pass(studentId: int):
    cursor.row_factory = sqlite3.Row
    cursor.execute(f'SELECT * FROM "Pass" WHERE studentId={studentId}')
    return {"msg": "success", "result": cursor.fetchall()}


