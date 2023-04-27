using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using RevSharp.Core.Models;

namespace RevSharp.Core.Controllers;

public class BotController : BaseController
{
    internal BotController(Client client)
        : base(client)
    {
    }

    public async Task<Bot?> Create(string name)
    {
        if (client.CurrentUser.Bot != null)
            throw new Exception("Can only create bots if logged in as user");

        var response = await client.PostAsync($"/bots/create", new Dictionary<string, object>()
        {
            { "name", name }
        });
        var stringContent = response.Content.ReadAsStringAsync().Result;
        if (response.StatusCode != HttpStatusCode.OK)
            return null;

        var data = JsonSerializer.Deserialize<Bot>(stringContent, Client.SerializerOptions);
        return data;
    }

    public async Task<FetchBotResponse?> Fetch(string id)
    {
        var response = await client.GetAsync($"/bots/{id}");
        if (response.StatusCode != HttpStatusCode.OK)
            return null;

        var stringContent = response.Content.ReadAsStringAsync().Result;
        var data = JsonSerializer.Deserialize<FetchBotResponse>(stringContent, Client.SerializerOptions);
        if (data == null)
            return null;

        await data.User.Fetch(client);
        return data;
    }

    public async Task<OwnedBotsResponse?> FetchOwned()
    {
        var response = await client.GetAsync($"/bots/@me");
        if (response.StatusCode != HttpStatusCode.OK)
            return null;

        var stringContent = response.Content.ReadAsStringAsync().Result;
        var data = JsonSerializer.Deserialize<OwnedBotsResponse>(stringContent, Client.SerializerOptions);
        return data;
    }

    public async Task<bool> Invite(string botId, string serverId)
    {
        var response = await client.PostAsync($"/bots/{botId}/invite", new Dictionary<string, object>()
        {
            { "server", serverId }
        });
        return response.StatusCode == HttpStatusCode.NoContent;
    }

    public Task<bool> Invite(Bot bot, Server server)
        => Invite(bot.Id, server.Id);

    public async Task<PublicBot?> FetchPublic(string id)
    {
        var response = await client.GetAsync($"/bots/{id}/invite");
        if (response.StatusCode != HttpStatusCode.OK)
            return null;
        var stringContent = response.Content.ReadAsStringAsync().Result;
        var data = JsonSerializer.Deserialize<PublicBot>(stringContent, Client.SerializerOptions);
        return data;
    }
}

public class FetchBotResponse
{
    [JsonPropertyName("bot")]
    public Bot Bot { get; set; }
    [JsonPropertyName("user")]
    public User User { get; set; }
}

public class OwnedBotsResponse
{
    [JsonPropertyName("bots")]
    public Bot[] Bots { get; set; }
    [JsonPropertyName("users")]
    public User[] Users { get; set; }
}