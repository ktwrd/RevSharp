using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public partial class Message
{
    public Task<bool> Edit(string content, SendableEmbed[]? embeds = null)
    {
        return Edit(Client, content, embeds);
    }
    public Task<bool> Edit(Client client, string content, SendableEmbed[]? embeds = null)
    {
        return Edit(client, new EditMessageData()
        {
            Content = content,
            Embeds = embeds ?? Array.Empty<SendableEmbed>()
        });
    }

    public Task<bool> Edit(EditMessageData data)
    {
        return Edit(Client, data);
    }
    public async Task<bool> Edit(Client client, EditMessageData data)
    {
        if (AuthorId != client.CurrentUser.Id)
            throw new Exception("Message Author doesn't match current user");
        var response = await client.PatchAsync(
            $"/channels/{ChannelId}/messages/{Id}",
            JsonContent.Create(data, options: Client.SerializerOptions));
        if (response.StatusCode != HttpStatusCode.OK)
            throw new Exception($"Message Edit resulted in response code of {response.StatusCode}");

        var stringContent = response.Content.ReadAsStringAsync().Result;
        var parsed = Message.Parse(stringContent);
        if (parsed == null)
            return false;
        Message.Inject(parsed, this);
        return true;
    }
}

public class EditMessageData
{
    [JsonPropertyName("content")]
    public string Content { get; set; }
    [JsonPropertyName("embeds")]
    public SendableEmbed[] Embeds { get; set; }
}