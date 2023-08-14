from dbProcess import *
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
        print(str(params["command"]))
        cursor.execute(str(params["command"]))
        db.commit()
        return cursor.fetchall()
        # try:
        #     cursor.execute(params["command"])
        #     db.commit()
        #     return cursor.fetchall()
        # except:
        #     print("error")
    return "error"





@app.on_event("shutdown")
async def shutdown_event():
    close(db, cursor)


@app.get("/isdatavalid")
async def is_elder(request: fastapi.Request):
    params = request.query_params




    if params.get("id", None) or params.get("code", None):
        group = cipher_suite.decrypt(params["code"]).decode()
        print(group)
        cursor.execute("SELECT * FROM Grupp WHERE bossId=={0} AND name==\"{1}\"".format(params["id"], group))
        db.commit()
        db_answer = cursor.fetchall()
        print(db_answer)
        if len(db_answer) != 0:
            return {"type": "answer", "answer": True, "groupName": db_answer[0][2]}
        # except:
        #     print("error")
    return {"type":"error"}


# def main():
#     db, cursor = start_db()
#
#     cursor.execute("""CREATE TABLE IF NOT EXISTS users(
#     id integer,
#     name TEXT
#     );""")
#     db.commit()
#
#     # cursor.execute(""" INSERT INTO  users(id, name) VALUES (0, 'ADRYIAN');""")
#     # db.commit()
#
#     cursor.execute("""SELECT * FROM users""")
#     print(cursor.fetchall())
#
#
#
#
#     close(db, cursor)
#
#
# if __name__=="__main__":
#     main()
