using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using TelegramBotEJournal.Entities;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace TelegramBotEJournal;

public static class HostApiAccess
{
    private class ResponseObject<T>
    {
        [JsonPropertyName("detail")]
        public T detail { get; set; }
    }
    
    private static HttpClient httpClient;
    
    private static readonly Uri host = new Uri("http://45.8.230.244");
    
    static HostApiAccess()
    {
        httpClient = new HttpClient(new HttpClientHandler(), true);
    }
    
    public static async Task<Student> GetStudentByTgID(long telegramId)
    {
        using var response = await httpClient.GetAsync(new Uri(host, $"/studentTg/{telegramId}"));

        if (!response.IsSuccessStatusCode) return new Student();
        
        var objResponse = JsonSerializer.Deserialize<ResponseObject<Student>>(await response.Content.ReadAsStringAsync());
        
        if (objResponse is null && objResponse?.detail is null) return new Student();
        
        return objResponse.detail;
    }
    
    public static async Task<List<Pass>> GetPassesByStudentID(long studentId, DateOnly from, DateOnly to)
    {
        var builder = new UriBuilder(host);
        builder.Path = "/pass";

        var query = HttpUtility.ParseQueryString(builder.Query);
        query["start"] = from.ToString("O");
        query["end"] = to.ToString("O");
        query["id"] = $"{studentId}";

        builder.Query = query.ToString();

        var response = await httpClient.GetAsync(builder.ToString());
        
        if (response.StatusCode != HttpStatusCode.OK) return new List<Pass>();
        
        var objResponse = JsonSerializer.Deserialize<ResponseObject<List<Pass>>>(await response.Content.ReadAsStringAsync())?.detail;
        
        if ((objResponse?.Count ?? 0) == 0) 
            return new List<Pass>();

        return objResponse;
    }

    public static async Task<bool> SetStudentTgID(string studentID, string token, long tgID)
    {
        using var response = await httpClient.PostAsJsonAsync(new Uri(host, "/studentTg"), new { id = studentID, token = token, tgId = tgID });

        if (response.StatusCode == HttpStatusCode.OK) return true;
        return false;
    }
    
    public static async Task<Dictionary<string, Dictionary<string, Lesson[][]>>?> GetStudentSchedule(long studentGroupID)
    {
        using var response = await httpClient.GetAsync(new Uri(host, $"/schedule/{studentGroupID}"));

        if (response.StatusCode != HttpStatusCode.OK) return null;

        var responseStr = await response.Content.ReadAsStringAsync();
        var objResponse = JsonSerializer.Deserialize<ResponseObject<Dictionary<string, Dictionary<string, Lesson[][]>>>>(await response.Content.ReadAsStringAsync())?.detail;

        if (objResponse is null) return null;
        
        return objResponse;
    }
}

/*

{"detail":{"Верхняя":{"Понедельник":[[{"id":1,"LessonName":"Теория вероятностей и математическая статистика","week":"Верхняя","day":"Понедельник","numberOfLesson":1,"LessonType":"Лекция","subgroup":1,"dates":null,"auditorium":null,"subgroupStudents":[210582]},{"id":8,"LessonName":"Теория вероятностей и математическая статистика","week":"Верхняя","day":"Понедельник","numberOfLesson":1,"LessonType":"Лекция","subgroup":1,"dates":null,"mask":0,"auditorium":null}],{"id":2,"LessonName":"Теория вероятностей и математическая статистика","week":"Верхняя","day":"Понедельник","numberOfLesson":2,"LessonType":"Лабораторная","subgroup":0,"dates":null,"mask":null,"auditorium":null},{"id":3,"LessonName":"Теория вероятностей и математическая статистика","week":"Верхняя","day":"Понедельник","numberOfLesson":3,"LessonType":"Лабораторная","subgroup":0,"dates":null,"mask":null,"auditorium":null}],"Вторник":[[{"id":4,"LessonName":"Теория вероятностей и математическая статистика","week":"Верхняя","day":"Вторник","numberOfLesson":1,"LessonType":"Лабораторная","subgroup":0,"dates":null,"mask":null,"auditorium":null},{"id":9,"LessonName":"Теория вероятностей и математическая статистика","week":"Верхняя","day":"Вторник","numberOfLesson":1,"LessonType":"Лекция","subgroup":1,"dates":["1234-22-22","5678-88-99"],"auditorium":null,"subgroupStudents":[210577,210582]}],{"id":5,"LessonName":"Теория вероятностей и математическая статистика","week":"Верхняя","day":"Вторник","numberOfLesson":2,"LessonType":"Лабораторная","subgroup":0,"dates":null,"mask":null,"auditorium":null}],"Среда":[],"Четверг":[[{"id":12,"LessonName":"Веб-технологии","week":"Верхняя","day":"Четверг","numberOfLesson":1,"LessonType":"Лабораторная","subgroup":1,"dates":["2023-08-24"],"auditorium":"111","subgroupStudents":[210582]},{"id":14,"LessonName":"Базы данных","week":"Верхняя","day":"Четверг","numberOfLesson":1,"LessonType":"Лабораторная","subgroup":1,"dates":["2023-08-24"],"auditorium":"112","subgroupStudents":[210577]}]],"Пятница":[],"Суббота":[],"Воскресение":[]},"Нижняя":{"Понедельник":[{"id":6,"LessonName":"Математический анализ","week":"Нижняя","day":"Понедельник","numberOfLesson":1,"LessonType":"Практическая","subgroup":0,"dates":null,"mask":null,"auditorium":null},{"id":7,"LessonName":"Математический анализ","week":"Нижняя","day":"Понедельник","numberOfLesson":2,"LessonType":"Лекция","subgroup":0,"dates":null,"mask":null,"auditorium":null}],"Вторник":[],"Среда":[],"Четверг":[],"Пятница":[],"Суббота":[],"Воскресение":[]}}}

*/