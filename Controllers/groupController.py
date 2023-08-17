from config import *
from dbModels import Group
from utils import *


@app.post("/group", status_code=201)
async def create_group(group_: Group):
    cursor.execute(f"SELECT * FROM 'Group' WHERE id={group_.id}")
    if cursor.fetchone():
        raise HTTPException(status_code=409, detail="Группа с таким id уже существует")
    command = "INSERT INTO 'Group' (" + fields(group_) + ") VALUES (" + values(group_) + ")"
    cursor.execute(command)
    db.commit()
    return {"detail": "Успешно добавлено", "group": group_}


@app.patch("/group/{id_}")
async def update_group(group_: Group, id_: int):
    cursor.execute(f"SELECT * FROM 'Group' WHERE id={group_.id}")
    if not cursor.fetchone():
        raise HTTPException(status_code=404, detail="Группы с таким id не существует")
    command = "UPDATE 'Group' SET " + predicates(group_) + f" WHERE id = {id_}"
    cursor.execute(command)
    db.commit()
    return {"detail": "Успешно обновлено"}


@app.delete("/group/{id_}")
async def delete_group(id_: int):
    cursor.execute(f"DELETE FROM 'Group' WHERE id = {id_}")
    db.commit()
    return {"detail": "Успешно удалено"}
