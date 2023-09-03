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
    
    [JsonPropertyName("subgroupStudents")]
    public long[] Students { get; set; }

#endregion

    public Lesson() : this(-1, EmptyField, EmptyField, -1, null) {}

    public Lesson(long id, string name, string lessonType, int lessonNumber)
    {
        ID = id;
        Name = (string)name.Clone();
        LessonType = (string)lessonType.Clone();
        LessonNumber = lessonNumber;
    }
    
    public Lesson(long id, string name, string lessonType, int lessonNumber, long[] studentsInGroup)
    {
        ID = id;
        Name = (string)name.Clone();
        LessonType = (string)lessonType.Clone();
        LessonNumber = lessonNumber;
        if (studentsInGroup is not null)
            Students = (long[])studentsInGroup.Clone();
        else 
            Students = null;
    }

    public override string ToString()
    {
        string lessonTime = LessonNumber switch
        {
            1 => "8:30-9:50",
            2 => "10:10-11:30",
            3 => "11:40-13:00",
            4 => "13:30-14:50",
            5 => "15:00-16:20",
            6 => "16:30-17:50",
            7 => "18:00-19:20",
            8 => "19:30-20:50",
        };
        return $"{lessonTime} {LessonType}:\n{Name}";
    }
}