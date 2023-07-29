import sqlite3

def start_db():
    db = sqlite3.connect("DataBase.db")
    sql = db.cursor()
    return (db, sql)


def close(db, cursor):
    cursor.close()
    db.close()


