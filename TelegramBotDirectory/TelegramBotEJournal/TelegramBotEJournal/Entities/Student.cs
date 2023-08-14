namespace TelegramBotEJournal.Entities;

public class Student
{
    private static readonly string EmptyField = "Не указано";
    
    public long CreditBookNumber { get; set; }
    public long TelegramID { get; set; }
    public long GroupID { get; set; }
    public string Surname { get; set; }
    public string Firstname { get; set; }
    public string Patronymic { get; set; }
    
    public Student() : this(-1, -1, -1, EmptyField, EmptyField, EmptyField) { }
    
    public Student(long creditBookNumber, long tgID, long groupID, string surname, string firstname, string patronymic)
    {
        CreditBookNumber = creditBookNumber;
        TelegramID = tgID;
        GroupID = groupID;
        
        if (!string.IsNullOrWhiteSpace(surname))
            Surname = (string)surname.Clone();
        else Surname = EmptyField;
        
        if (!string.IsNullOrWhiteSpace(Firstname))
            Firstname = (string)firstname.Clone();
        else Firstname = EmptyField;

        if (!string.IsNullOrWhiteSpace(Patronymic))
            Patronymic = (string)patronymic.Clone();
        else Patronymic = EmptyField;
    }
    
    public override string ToString()
    {
        return
            $"Номер зачётной книжки: {CreditBookNumber}\nTelegram ID: {TelegramID}\nГруппа: {GroupID}\nФамилия: {Surname}\nИмя: {Firstname}\nОтчество: {Patronymic}";
    }
}