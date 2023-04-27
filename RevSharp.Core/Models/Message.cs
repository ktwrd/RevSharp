using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class Message : ISnowflake
{
    [JsonPropertyName("_id")]
    public string Id { get; set; }
    [JsonPropertyName("nonce")]
    public string? Nonce { get; set; }
    [JsonPropertyName("channel")]
    public string ChannelId { get; set; }
    [JsonPropertyName("author")]
    public string AuthorId { get; set; }
    [JsonPropertyName("content")]
    public string? Content { get; set; }
    [JsonPropertyName("system")]
    public SystemMessage? SystemMessage { get; set; }
    [JsonPropertyName("attachments")]
    public File[]? Attachments { get; set; }
    [JsonPropertyName("edited")]
    public string? EditedAt { get; set; }
    [JsonPropertyName("embeds")]
    public BaseEmbed[]? Embeds { get; set; }
    [JsonPropertyName("mentions")]
    public string[]? MentionIds { get; set; }
    [JsonPropertyName("replies")]
    public string[]? MessageReplyIds { get; set; }
    [JsonPropertyName("reactions")]
    public Dictionary<string, string[]> Reactions { get; set; }
    [JsonPropertyName("interactions")]
    public Interactions Interactions { get; set; }
    [JsonPropertyName("Masquerade")]
    public Masquerade? Masquerade { get; set; }

    public async Task<bool> Acknowledge(Client client)
    {
        var response = await client.PutAsync($"/channels/{ChannelId}/ack/{Id}");
        return response.StatusCode == HttpStatusCode.NoContent;
    }
    public async Task<bool> Delete(Client client)
    {
        var response = await client.DeleteAsync($"/channels/{ChannelId}/messages/{Id}");
        return response.StatusCode == HttpStatusCode.NoContent;
    }
    
    public async Task<bool> Fetch(Client client)
    {
        var response = await client.GetAsync($"/channels/{ChannelId}/messages/{Id}");
        if (response.StatusCode != HttpStatusCode.OK)
            return false;

        var stringContent = response.Content.ReadAsStringAsync().Result;
        var data = JsonSerializer.Deserialize<Message>(stringContent, Client.SerializerOptions);
        if (data == null)
            return false;
        
        Id = data.Id;
        Nonce = data.Nonce;
        ChannelId = data.ChannelId;
        AuthorId = data.AuthorId;
        Content = data.Content;
        SystemMessage = data.SystemMessage;
        Attachments = data.Attachments;
        EditedAt = data.EditedAt;
        Embeds = EmbedHelper.GetEmbedsFromMessage(stringContent);
        MentionIds = data.MentionIds;
        MessageReplyIds = data.MessageReplyIds;
        Reactions = data.Reactions;
        Interactions = data.Interactions;
        Masquerade = data.Masquerade;
        return true;
    }
    public static Message? ParseMessage(string content)
    {
        var data = JsonSerializer.Deserialize<Message>(content, Client.SerializerOptions);
        if (data == null)
            return null;
        data.Embeds = EmbedHelper.GetEmbedsFromMessage(content);
        return data;
    }

}

public class Interactions
{
    [JsonPropertyName("reactions")]
    public string[] Reactions { get; set; }
    [JsonPropertyName("restrict_reactions")]
    public bool RestrictReactions { get; set; }
}

public class Masquerade
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("avatar")]
    public string Avatar { get; set; }
    [JsonPropertyName("colour")]
    public string Colour { get; set; }
}

public class SystemMessage
{
    public string Type { get; set; }
    public string? Content { get; set; }
    public string? Id { get; set; }
    public string? By { get; set; }
    public string? Name { get; set; }
    public string? From { get; set; }
    public string? To { get; set; }
}

public enum SystemMessageType
{
    text,
    user_added,
    user_remove,
    user_joined,
    user_left,
    user_kicked,
    channel_renamed,
    channel_description_changed,
    channel_icon_changed,
    channel_ownership_changed
}