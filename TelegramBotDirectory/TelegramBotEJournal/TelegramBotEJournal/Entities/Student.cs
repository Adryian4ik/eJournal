namespace TelegramBotEJournal.Entities;

public class Student
{
    public long CreditBookNumber;
    public long TelegramID;
    public long GroupID;
    public string Surname;
    public string Firstname;
    public string Patronymic;

    public override string ToString()
    {
        return
            $"Credit Book Number: {CreditBookNumber}\nTelegram ID: {TelegramID}\nGroup ID: {GroupID}\nSurname: {Surname}\nFirstname: {Firstname}\nPatronymic: {Patronymic}";
    }
}