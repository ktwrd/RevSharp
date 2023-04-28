using System.Net;
using System.Text.Json;

namespace RevSharp.Core.Models;

public partial class Message
{
    
    public async Task<bool> Acknowledge(Client client)
    {
        var response = await client.PutAsync($"/channels/{ChannelId}/ack/{Id}");
        return response.StatusCode == HttpStatusCode.NoContent;
    }
    public Task<bool> Delete(Client client)
    {
        return Delete(client, ChannelId, Id);
    }
    public Task<Message?> Reply(Client client, DataMessageSend data, bool mention = true)
    {
        data.Replies = new[]
        {
            new Reply(this, mention)
        };
        return Send(client, ChannelId, data);
    }
    public static async Task<Message?> Send(Client client, string channelId, DataMessageSend data)
    {
        var content = JsonSerializer.Serialize(data, Client.SerializerOptions);
        var response = await client.PostAsync($"/channels/{channelId}/messages", new StringContent(content));
        if (response.StatusCode != HttpStatusCode.OK)
            throw new Exception($"Failed to send message in channel {channelId}");

        var textContent = response.Content.ReadAsStringAsync().Result;
        var output = JsonSerializer.Deserialize<Message>(textContent, Client.SerializerOptions);
        return output;
    }
    
    #region Internal Static

    internal static async Task<bool> Delete(Client client, string channelId, string messageId)
    {
        var response = await client.DeleteAsync($"/channels/{channelId}/messages/{messageId}");
        return response.StatusCode == HttpStatusCode.NoContent;
    }
    #endregion

    
    #region Overloads
    public Task<Message?> Reply(
        Client client,
        string? content,
        bool mention = true,
        SendableEmbed[]? embeds = null,
        Masquerade? masquerade = null,
        Interactions[]? interactions = null,
        string[]? attachments = null)
    {
        var data = new DataMessageSend()
        {
            Replies = new []
            {
                new Reply(this, mention)
            },
            Content = content,
            Embeds = embeds,
            Masquerade = masquerade,
            Interactions = interactions,
            Attachments = attachments
        };
        return Reply(client, data);
    }
    public static Task<Message?> Send(Client client,
        string channelId,
        string? content,
        Reply[]? replies,
        SendableEmbed[]? embeds,
        Masquerade? masquerade,
        Interactions[]? interactions,
        string[]? attachments)
    {
        var data = new DataMessageSend()
        {
            Content = content,
            Attachments = attachments,
            Replies = replies,
            Embeds = embeds,
            Masquerade = masquerade,
            Interactions = interactions
        };
        return Send(client, channelId, data);
    }
    #endregion
}