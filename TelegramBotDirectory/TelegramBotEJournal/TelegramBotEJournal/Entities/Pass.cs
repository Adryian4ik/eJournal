using System.Text.Json.Serialization;

namespace TelegramBotEJournal.Entities;

public class Pass
{
    private static readonly string EmptyField = "Не указано";
    
    public enum PassTypeEnum : byte
    {
        Other = 0,
        Disrespectful = 1,
        Respectful = 2,
        ByApplication = 3,
    }
    
    #region fields

    [JsonPropertyName("id")]
    public long ID { get; set; }
    
    [JsonPropertyName("lastname")]
    public string Lastname { get; set; }

    [JsonPropertyName("firstname")]
    public string Firstname { get; set; }
    
    [JsonPropertyName("patronymic")]
    public string Patronymic { get; set; }
    
    [JsonPropertyName("tgId")]
    public long TelegramID { get; set; }
    
    [JsonPropertyName("date")] 
    public DateOnly Date { get; set; }
    
    [JsonPropertyName("numberOfLesson")]
    public byte LessonNumber { get; set; }
    
    [JsonPropertyName("type")]
    public PassTypeEnum PassType { get; set; }
    
    [JsonPropertyName("name")]
    public string LessonName { get; set; }
    
    [JsonPropertyName("documentId")]
    public long DocumentID { get; set; }

#endregion
    
    public Pass() : this(-1, EmptyField, EmptyField, EmptyField, -1, new DateOnly(), 0, 0, EmptyField, -1) { }
    
    public Pass(long id, string lastname, string firstname, string patronymic, long telegramId, DateOnly date, byte lessonNumber, PassTypeEnum passType, string lessonName, long documentId)
    {
        this.ID = id;
        Lastname = (string)lastname.Clone();
        Firstname = (string)firstname.Clone();
        Patronymic = (string)patronymic.Clone();
        TelegramID = telegramId;
        Date = date;
        LessonNumber = lessonNumber;
        this.PassType = passType;
        LessonName = (string)lessonName.Clone();
        DocumentID = documentId;
    }

    public override string ToString()
    {
        return $"[{Date.ToString("DD:MM:yyyy")}]\nЗанятие: {LessonName}\nНомер пары: {LessonNumber}\nТип пропуска: {PassType switch 
        {
            PassTypeEnum.Disrespectful => "Неуважительный",
            PassTypeEnum.Respectful => $"Уважительный\nДокумент: {DocumentID}",
            PassTypeEnum.ByApplication => $"По заявлению\nДокумент: {DocumentID}",
            _ => "",
        }}";
    }
}