namespace RevSharp.Core.Models;

public partial class Message
{
    public async Task AddReaction(Client client, string emoji)
    {
        var response = await client.PutAsync($"/channels/{ChannelId}/messages/{Id}/reactions/{emoji}");
        if ((int)response.StatusCode != 204)
            throw new Exception($"Failed to add reaction, server responded with {response.StatusCode}");

        Reactions.TryAdd(emoji, Array.Empty<string>());
        Reactions[emoji] = Reactions[emoji].Concat(new string[]
        {
            client.CurrentUser.Id
        }).ToArray();
    }

    public async Task RemoveReactions(Client client, string emoji)
    {
        var response = await client.DeleteAsync($"/channels/{ChannelId}/messages/{Id}/reactions/{emoji}");
        if ((int)response.StatusCode != 204)
            throw new Exception($"Failed to remove reactions, server responded with {response.StatusCode}");

        Reactions.Remove(emoji);
    }

    public async Task RemoveAllReactions(Client client)
    {
        var response = await client.DeleteAsync($"/channels/{ChannelId}/messages/{Id}/reactions");
        if ((int)response.StatusCode != 204)
            throw new Exception($"Failed to remove all reactions, server responded with {response.StatusCode}");

        Reactions = new Dictionary<string, string[]>();
    }
}