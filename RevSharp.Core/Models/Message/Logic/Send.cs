using System.Net;
using System.Text.Json;
using kate.shared.Helpers;

namespace RevSharp.Core.Models;

public partial class Message
{
    public static async Task<Message?> Send(
        Client client,
        string channelId,
        DataMessageSend data)
    {
        if (client == null)
        {
            throw new Exception("Client is null");
        }
        data.Nonce = GeneralHelper.GenerateUID();
        var content = JsonSerializer.Serialize(data, Client.SerializerOptions);
        var response = await client.PostAsync($"/channels/{channelId}/messages", new StringContent(content));
        if (response.StatusCode != HttpStatusCode.OK)
            throw new Exception($"Failed to send message in channel {channelId}");

        var textContent = response.Content.ReadAsStringAsync().Result;
        var output = JsonSerializer.Deserialize<Message>(textContent, Client.SerializerOptions);
        return output;
    }
    public static Task<Message?> Send(
        Client client,
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
}