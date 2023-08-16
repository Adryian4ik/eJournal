using TelegramBotEJournal.Entities;
using System.Text.Json;

namespace TelegramBotEJournal;

public static class HostApiAccess
{
    private static HttpClient httpClient;
    
    private static readonly string host = "http://45.8.230.244:8000/?command=";

    static HostApiAccess()
    {
        httpClient = new HttpClient(new HttpClientHandler(), true);
    }
    
    
    
    public static async Task<List<Pass>> GetPasses(long studentId, DateTime when)
    {
        using var response = await httpClient.GetAsync($"{host}SELECT * FROM Pass WHERE studentId={studentId} and year={when.Year} and month={when.Month} and day={when.Day}");
        var objResponse = JsonSerializer.Deserialize<List<Pass>>(await response.Content.ReadAsStringAsync());
        
        if ((objResponse?.Count ?? -1) == 0) return new List<Pass>();

        return objResponse.ToList();
    }
    
    public static async Task<Student> GetStudent(long telegramId)
    {
        using var response = await httpClient.GetAsync($"{host}SELECT * FROM students WHERE tgId={telegramId};");
        
        var objResponse = JsonSerializer.Deserialize<Student[]>(await response.Content.ReadAsStringAsync());
        
        if ((objResponse?.Length ?? 0) != 1) return new Student();
        
        return objResponse.First();
    }
}