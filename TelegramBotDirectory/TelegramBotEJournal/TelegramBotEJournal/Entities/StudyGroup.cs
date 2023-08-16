using System.Text.Json.Serialization;

namespace TelegramBotEJournal.Entities;

public class StudyGroup
{
    #region fields

    [JsonPropertyName("id")]
    public long ID { get; set; }
    
    [JsonPropertyName("bossId")]
    public long GroupLeaderID { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }

#endregion

    public StudyGroup() : this(-1, -1, "Не указано") {}
    
    public StudyGroup(long id, long groupLeaderId, string groupName)
    {
        ID = id;
        GroupLeaderID = groupLeaderId;
        Name = (string)groupName.Clone();
    }

    public override string ToString()
    {
        return $"{Name}";
    }
}