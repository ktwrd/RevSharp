using System.Net;

namespace RevSharp.Core;

internal partial class EndpointFactory
{
    internal string Channel(string id) => $"{BaseUrl}/channels/{WebUtility.UrlEncode(id)}";
    internal string ChannelPermissions(string id, string role = "default") => $"{BaseUrl}/channels/{id}/permissions/{role}";
    internal string ChannelRecipients(string id, string member) => $"{BaseUrl}/channels/{id}/recipients/{member}";
    internal string ChannelSearch(string id) => $"{BaseUrl}/channels/{id}/search";
    internal string ChannelMessages(string id) => $"{BaseUrl}/channels/{id}/messages";
    internal string ChannelMessagesBulk(string id) => $"{BaseUrl}/channels/{id}/messages/bulk";
    internal string ChannelMessage(string id, string message) => $"{BaseUrl}/channels/{id}/messages/{message}";
    internal string ChannelMessageReactions(string id, string message) => $"{BaseUrl}/channels/{id}/messages/{message}/reactions";
    internal string ChannelMessageReactions(string id, string message, string emoji) => $"{BaseUrl}/channels/{id}/messages/{message}/reactions/{emoji}";
    internal string ChannelMembers(string id) => $"{BaseUrl}/channels/{id}/members";
    internal string ChannelJoinCall(string id) => $"{BaseUrl}/channels/{id}/join_call";
    internal string ChannelInvites(string id) => $"{BaseUrl}/channels/{id}/invites";
    internal string ChannelMessageAck(string id, string message) => $"{BaseUrl}/channels/{id}/ack/{message}";
    internal string ChannelCreate() => $"{BaseUrl}/channels/create";
}