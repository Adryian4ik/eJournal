using System.Text.Json.Serialization;

namespace TelegramBotEJournal.Entities;

public class Lesson
{
    #region fields

    [JsonPropertyName("id")]
    public long ID { get; set; }
    
    [JsonPropertyName("teacherId")]
    public long TeacherID { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("semestr")]
    public byte Semester { get; set; }
    
    [JsonPropertyName("type")]
    public int LessonType { get; set; }

#endregion

    public Lesson(long id, long teacherId, string name, byte semester, int lessonType)
    {
        ID = id;
        TeacherID = teacherId;
        Name = name;
        Semester = semester;
        LessonType = lessonType;
    }
}