using System.Text.Json.Serialization;

namespace TelegramBotEJournal.Entities;

public class Student
{
    private static readonly string EmptyField = "Не указано";
    
    #region fields

    [JsonIgnore]
    public long ID => CreditBookNumber;
    
    [JsonPropertyName("id")]
    public long CreditBookNumber { get; set; }
    
    [JsonPropertyName("tgId")]
    public long TelegramID { get; set; }
    
    [JsonPropertyName("groupId")]
    public long GroupID { get; set; }
    
    [JsonPropertyName("lastname")]
    public string Lastname { get; set; }
    
    [JsonPropertyName("firstname")]
    public string Firstname { get; set; }
    
    [JsonPropertyName("patronymic")]
    public string Patronymic { get; set; }
    
    #endregion
    
    public Student() : this(-1, -1, -1, EmptyField, EmptyField, EmptyField) { }
    
    public Student(long creditBookNumber, long tgID, long groupID, string surname, string firstname, string patronymic)
    {
        CreditBookNumber = creditBookNumber;
        TelegramID = tgID;
        GroupID = groupID;

        Lastname = CheckString(surname);

        Firstname = CheckString(firstname);

        Patronymic = CheckString(patronymic);

        string CheckString(string field)
        {
            if (!string.IsNullOrWhiteSpace(field)) return (string)field.Clone();
            else return EmptyField;
        }
    }
    
    public override string ToString()
    {
        return $"""
             Номер зачётной книжки: {CreditBookNumber}
             Группа: {GroupID}
             Фамилия: {Lastname}
             Имя: {Firstname}
             Отчество: {Patronymic}
             """;
    }
}