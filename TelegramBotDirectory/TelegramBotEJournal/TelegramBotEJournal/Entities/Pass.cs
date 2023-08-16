using System.Text.Json.Serialization;

namespace TelegramBotEJournal.Entities;

public class Pass
{
    #region fields

    [JsonPropertyName("id")]
    public long ID { get; set; }
    
    [JsonPropertyName("studentId")]
    public long StudentID { get; set; }
    
    [JsonPropertyName("day")]
    public byte Day { get; set; }
    
    [JsonPropertyName("month")]
    public byte Month { get; set; }
    
    [JsonPropertyName("year")]
    public long Year { get; set; }
    
    [JsonPropertyName("lessonNumber")]
    public byte LessonNumber { get; set; }
    
    [JsonPropertyName("lessonId")]
    public long LessonID { get; set; }
    
    [JsonPropertyName("type")]
    public byte PassType { get; set; }
    
    [JsonPropertyName("documnentId")]
    public long DocumentID { get; set; }

#endregion

    public Pass() : this(-1, -1, 0, 0, 0, 0, -1, 255, -1) { }
    
    public Pass(long id, long studentId, byte day, byte month, long year, byte lessonNumber, long lessonId, byte passType, long documentId)
    {
        this.ID = id;
        StudentID = studentId;
        this.Day = day;
        this.Month = month;
        this.Year = year;
        this.LessonNumber = lessonNumber;
        LessonID = lessonId;
        this.PassType = passType;
        DocumentID = documentId;
    }

    public override string ToString()
    {
        return $"[{Day}:{Month}:{Year}]\nЗанятие: {LessonID}\nТип пропуска: {(PassType == 0 ? "Неуважительный" : (PassType == 1 ? "Уважительный" : (PassType == 2 ? "По справке" : "Not defined")))}";
    }
}