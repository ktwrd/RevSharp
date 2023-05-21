using RevSharp.Core.Models;
using RevSharp.Core.Models.WebSocket;

namespace RevSharp.Core.Helpers;

public delegate void GenericDelegate<T>(
    T content);
public delegate void ReadyMessageDelegate(
    ReadyMessage message,
    string json);

public delegate void UserIdDelegate(
    string userId);

public delegate void MemberIdDelegate(
    string serverId,
    string userId);

#region Message
public delegate void MessageDelegate(
    Message message);
public delegate void MessageDeleteDelegate(
    string messageId,
    string channelId);
public delegate void MessageReactedDelegate(
    string userId,
    string react,
    string messageId);
#endregion

#region Channels
public delegate void ChannelUpdateDelegate(
    BaseChannel previous,
    BaseChannel current);
public delegate void ChannelDelegate(
    BaseChannel channel);

public delegate void ChannelIdDelegate(
    string channelId);
public delegate void ChannelTypingDelegate(
    string channelId,
    string userId);
#endregion

public delegate void ServerDelegate(
    Server server);