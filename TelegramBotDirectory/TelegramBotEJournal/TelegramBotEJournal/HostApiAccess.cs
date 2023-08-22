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
        query["end"] = from.ToString("O");
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
}