using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevSharp.Skidbot.Controllers;

public class AuthentikPaginationResponse<T>
{
    [JsonPropertyName("pagination")]
    public AuthentikPagination Pagination { get; set; }
    [JsonPropertyName("results")]
    public T[] Results { get; set; }
}

public class AuthentikPagination
{
    [JsonPropertyName("next")]
    public int Next { get; set; }
    [JsonPropertyName("previous")]
    public int Previous { get; set; }
    [JsonPropertyName("count")]
    public int Count { get; set; }
    [JsonPropertyName("current")]
    public int CurrentIndex { get; set; }
    [JsonPropertyName("total_pages")]
    public int TotalPages { get; set; }
    [JsonPropertyName("start_index")]
    public int StartIndex { get; set; }
    [JsonPropertyName("end_index")]
    public int EndIndex { get; set; }
}
public partial class AuthentikController
{
    public async Task<AuthentikPaginationResponse<AuthentikUserResponse>?> GetUsers(string username)
    {
        var response = await GetAsync($"core/users/?username={username}");
        var stringContent = response.Content.ReadAsStringAsync().Result;
        if (response.StatusCode != HttpStatusCode.OK)
            return null;

        var data = JsonSerializer.Deserialize<AuthentikPaginationResponse<AuthentikUserResponse>>(
            stringContent, Program.SerializerOptions);
        return data;
    }

    public async Task<AuthentikUserResponse?> GetUser(string id)
    {
        var response = await GetAsync($"core/users/{id}/");
        if (response.StatusCode != HttpStatusCode.OK)
            return null;

        var stringContent = response.Content.ReadAsStringAsync().Result;
        var data = JsonSerializer.Deserialize<AuthentikUserResponse>(stringContent, Program.SerializerOptions);
        return data;
    }

    public async Task<bool> DeleteUser(string id)
    {
        var response = await DeleteAsync($"core/users/{id}/");
        return response.StatusCode == HttpStatusCode.NoContent;
    }
}

public class AuthentikMinimalUser
{
    [JsonPropertyName("pk")]
    public int Id { get; set; }
    [JsonPropertyName("username")]
    public string Username { get; set; }
    [JsonPropertyName("name")]
    public string DisplayName { get; set; }
    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }
    [JsonPropertyName("last_login")]
    public string? LastLogin { get; set; }
    [JsonPropertyName("email")]
    public string Email { get; set; }
    [JsonPropertyName("attributes")]
    public Dictionary<string, object> Attributes { get; set; }
    [JsonPropertyName("uid")]
    public string Uid { get; set; }
}