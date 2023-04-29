using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class UserMutualResponse
{
    [JsonPropertyName("users")]
    public string[] UserIds { get; set; }
    [JsonPropertyName("servers")]
    public string[] ServerIds { get; set; }
    
    [JsonIgnore]
    public User[] Users { get; set; }
    [JsonIgnore]
    public Server[] Servers { get; set; }

    public async Task<bool> Fetch(Client client, string userId)
    {
        var response = await client.GetAsync($"/users/{userId}/mutual");
        if (response.StatusCode != HttpStatusCode.OK)
            return false;

        var stringContent = response.Content.ReadAsStringAsync().Result;
        var data = JsonSerializer.Deserialize<UserMutualResponse>(stringContent, Client.SerializerOptions);
        if (data == null)
            return false;

        UserIds = data.UserIds;
        ServerIds = data.UserIds;

        var userList = new List<User>();
        foreach (var i in UserIds)
        {
            var u = new User(i);
            if (await u.Fetch(client))
                userList.Add(u);
        }
        Users = userList.ToArray();

        var serverList = new List<Server>();
        foreach (var i in ServerIds)
        {
            var s = new Server(i);
            if (await s.Fetch(client))
                serverList.Add(s);
        }
        Servers = serverList.ToArray();
        
        return true;
    }
}