using System.Text.Json.Serialization;

namespace TelegramBotEJournal.Entities;

public class Lesson
{
    private static readonly string EmptyField = "Не указано";
    
    #region fields

    [JsonPropertyName("id")]
    public long ID { get; set; }
    
    [JsonPropertyName("LessonName")]
    public string Name { get; set; }
    
    [JsonPropertyName("LessonType")]
    public string LessonType { get; set; }
    
    [JsonPropertyName("numberOfLesson")]
    public int LessonNumber { get; set; }

#endregion

    public Lesson() : this(-1, EmptyField, EmptyField, -1) {}

    public Lesson(long id, string name, string lessonType, int lessonNumber)
    {
        ID = id;
        Name = (string)name.Clone();
        LessonType = (string)lessonType.Clone();
        LessonNumber = lessonNumber;
    }

    public override string ToString()
    {
        return $"{LessonNumber} {LessonType}:\n{Name}";
    }
}