
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
        JsonElement? objResponse;
        using (var response = await httpClient.GetAsync($"{host}SELECT * FROM Pass WHERE studentId={studentId} and year={when.Year} and month={when.Month} and day={when.Day};"))
        {
            objResponse = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
        }
        
        if (objResponse is null) return new List<Pass>();

        using var responseEnumerator = objResponse.Value.EnumerateArray();
        
        List<Pass> result = new List<Pass>();
        List<long> longList = new List<long>();
        foreach (var responseArray in responseEnumerator)
        {
            using (var i = responseArray.EnumerateArray())
            {
                longList.Clear();
                foreach(var item in i)
                    longList.Add(item.GetInt64());
                
                result.Add(new Pass(longList[0], longList[1], (byte)longList[2], (byte)longList[3], longList[4], (byte)longList[5], longList[6], (byte)longList[7], longList[8]));
            }
        }

        return result;
    }

    public static async Task<long> GetStudentIDFromTelegramID(long telegramId)
    {
        JsonElement? objResponse;
        using (var response = await httpClient.GetAsync($"{host}SELECT id FROM students WHERE tgId={telegramId};"))
        {
            objResponse = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
        }

        if (objResponse is null) return -1;
        using var responseArray = objResponse.Value.EnumerateArray();
        responseArray.MoveNext();
        using var arrayArray = responseArray.Current.EnumerateArray();
        arrayArray.MoveNext();
        return arrayArray.Current.GetInt64();
    }
    
    public static async Task<Student> GetStudent(long telegramId)
    {
        using var response = await httpClient.GetAsync($"{host}SELECT * FROM students WHERE tgId={telegramId};");
        
        var objResponse = JsonSerializer.Deserialize<JsonElement?>(await response.Content.ReadAsStringAsync());
        
        if (objResponse is null) return new Student();

        using var responseEnumerator2 = objResponse.Value.EnumerateArray();
            
        responseEnumerator2.MoveNext();
        if (responseEnumerator2.Current.ValueKind == JsonValueKind.Null) return new Student();
        var responseEnumerator = responseEnumerator2.Current.EnumerateArray();
        
        Student result = new Student();
        responseEnumerator.MoveNext();
        result.CreditBookNumber = responseEnumerator.Current.GetInt64();
        responseEnumerator.MoveNext();
        result.TelegramID = responseEnumerator.Current.GetInt64();
        responseEnumerator.MoveNext();
        result.GroupID = responseEnumerator.Current.GetInt64();
        responseEnumerator.MoveNext();
        result.Surname = responseEnumerator.Current.GetString();
        responseEnumerator.MoveNext();
        result.Firstname = responseEnumerator.Current.GetString();
        responseEnumerator.MoveNext();
        result.Patronymic = responseEnumerator.Current.GetString();
        
        responseEnumerator.Dispose();
        
        return result;
    }
}