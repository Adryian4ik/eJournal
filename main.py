from config import *
import Controllers


@app.on_event("startup")
async def startup_event():
    print("Приложение запущено! Выполняем начальную инициализацию...")


@app.on_event("shutdown")
async def shutdown_event():
    close(db, cursor)
