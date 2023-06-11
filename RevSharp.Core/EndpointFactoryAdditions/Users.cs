namespace RevSharp.Core;

internal partial class EndpointFactory
{
    internal string User(string id) => $"{BaseUrl}/users/{id}";
    internal string UserFriend() => $"{BaseUrl}/users/friend";
    internal string UserDirectMessageChannels() => $"{BaseUrl}/users/dms";
    internal string UsernameChange() => $"{BaseUrl}/users/@me/username";

    internal string UserProfile(string id) => $"{BaseUrl}/users/{id}/profile";
    internal string UserMutual(string id) => $"{BaseUrl}/users/{id}/mutual";
    internal string UserFriendState(string id) => $"{BaseUrl}/users/{id}/friend";
    internal string UserFlags(string id) => $"{BaseUrl}/users/{id}/flags";
    internal string UserDirectMessage(string id) => $"{BaseUrl}/users/{id}/dm";
    internal string UserDefaultAvatar(string id) => $"{BaseUrl}/users/{id}/default_avatar";
    internal string UserBlock(string id) => $"{BaseUrl}/users/{id}/block";
}