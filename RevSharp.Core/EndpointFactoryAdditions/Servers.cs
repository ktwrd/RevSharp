namespace RevSharp.Core;

internal partial class EndpointFactory
{
    internal string ServerCreate() => $"{BaseUrl}/servers/create";

    internal string ServerMember(string id, string member) => $"{BaseUrl}/servers/{id}/members/{member}";
    internal string ServerMembers(string id, bool excludeOffline) =>
        $"{BaseUrl}/servers/{id}/members?exclude_offline={excludeOffline}";

    internal string ServerBan(string id, string member) => $"{BaseUrl}/servers/{id}/bans/{member}";
    internal string ServerBans(string id) => $"{BaseUrl}/servers/{id}/bans";

    internal string ServerRoles(string id) => $"{BaseUrl}/servers/{id}/roles";
    internal string ServerRole(string id, string role) => $"{BaseUrl}/servers/{id}/roles/{role}";

    internal string ServerPermissions(string id, string role = "default") => $"{BaseUrl}/servers/{id}/permissions/{role}";

    internal string ServerInvites(string id) => $"{BaseUrl}/servers/{id}/invites";
    internal string ServerEmojis(string id) => $"{BaseUrl}/servers/{id}/emojis";

    internal string ServerChannelCreate(string id) => $"{BaseUrl}/servers/{id}/channels";

    internal string ServerAck(string id) => $"{BaseUrl}/servers/{id}/ack";

    internal string ServerLeave(string id, bool leaveSilently = false) =>
        $"{BaseUrl}/servers/{id}?leave_silently={leaveSilently}";

}