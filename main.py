from config import *
import Controllers


@app.on_event("startup")
async def startup_event():
    print("Приложение запущено! Выполняем начальную инициализацию...")
    lastId['Lesson'] = cursor.execute("select max(id) from 'lesson'").fetchone()[0]
    lastId['Pass'] = cursor.execute("select max(id) from 'Pass'").fetchone()[0]
    lastId['Schedule'] = cursor.execute("select max(id) from 'Schedule'").fetchone()[0]
    if not lastId['Lesson']:
        lastId['Lesson'] = 0
    if not lastId['Pass']:
        lastId['Pass'] = 0
    if not lastId['Schedule']:
        lastId['Schedule'] = 0


@app.on_event("shutdown")
async def shutdown_event():
    close(db, cursor)
