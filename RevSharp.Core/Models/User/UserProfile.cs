using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class UserProfile : IUserProfile
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }
    [JsonPropertyName("background")]
    public File? Background { get; set; }

    public static async Task<UserProfile?> Fetch(Client client, string userId)
    {
        var response = await client.GetAsync($"/users/{userId}/profile");
        if (response.StatusCode != HttpStatusCode.OK)
            return null;

        var stringContent = response.Content.ReadAsStringAsync().Result;
        var data = JsonSerializer.Deserialize<UserProfile>(stringContent, Client.SerializerOptions);
        return data;
    }
}