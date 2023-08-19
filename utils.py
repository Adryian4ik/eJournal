import cryptography.fernet
from config import cipher_suite
from fastapi import HTTPException


def get_value(value):
    if isinstance(value, str):
        return f"'{value}', " if value else ""
    else:
        return f"{value}, " if value else ""


def predicate(name, value):
    if isinstance(value, str):
        return f"{name} = '{value}', " if value else ""
    else:
        return f"{name} = {value}, " if value else ""


def field(name, value):
    if value:
        return f'{name}, ' if value else ""
    else:
        return ""


def fields(Class):
    dictionary = dict(Class)
    result = ""
    for key in dictionary:
        result = result + field(key, dictionary[key])
    return result[:-2]


def values(Class):
    dictionary = dict(Class)
    result = ""
    for key in dictionary:
        result = result + get_value(dictionary[key])
    return result[:-2]


def predicates(Class):
    dictionary = dict(Class)
    result = ""
    for key in dictionary:
        if key != 'id':
            result += predicate(key, dictionary[key])
    return result[:-2]


def decode_token(token):
    try:
        decoded = cipher_suite.decrypt(token).decode()
    except cryptography.fernet.InvalidToken:
        raise HTTPException(status_code=401, detail="Не корректный токен")
    return decoded


def group_id(decoded, cursor):
    cursor.execute(f"SELECT id FROM 'Group' WHERE name='{decoded}'")
    checkId = cursor.fetchone()
    if not checkId:
        raise HTTPException(status_code=403, detail="Нет доступа")
    return checkId[0]
