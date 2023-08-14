from dbProcess import *
import fastapi


db, cursor = start_db()

app = fastapi.FastAPI()

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
        cursor.execute("SELECT * FROM Grupp WHERE bossId=={0} AND name=={1}".format(params["id"], params["code"]))
        db.commit()
        print(cursor.fetchall())
        if cursor.fetchall() != []:
            return {"type": "answer", "answer": True, "groupName": cursor.fetchall()[3]}
        else:
            return {"type": "answer", "answer": False}
        # try:
        #     cursor.execute(params["command"])
        #     db.commit()
        #     return cursor.fetchall()
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