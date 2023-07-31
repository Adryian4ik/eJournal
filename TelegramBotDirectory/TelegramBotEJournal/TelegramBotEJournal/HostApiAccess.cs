
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
    
    public static async Task<Student> GetStudent(long telegramId)
    {
        var response = await httpClient.GetAsync($"{host}SELECT * FROM students WHERE tgId={telegramId};");
        //Console.WriteLine(await response.Content.ReadAsStringAsync());
        JsonElement? objResponse = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
        response.Dispose();
        if (objResponse is null) return null;
        var responseEnumerator2 = objResponse.Value.EnumerateArray();
        responseEnumerator2.MoveNext();
        if (responseEnumerator2.Current.ValueKind == JsonValueKind.Null) return null;
        var responseEnumerator = responseEnumerator2.Current.EnumerateArray();
        responseEnumerator2.Dispose();
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