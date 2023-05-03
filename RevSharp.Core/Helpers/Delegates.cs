﻿using RevSharp.Core.Models;
using RevSharp.Core.Models.WebSocket;

namespace RevSharp.Core.Helpers;

public delegate void GenericDelegate<T>(T content);
public delegate void MessageDelegate(Message message);
public delegate void ReadyMessageDelegate(ReadyMessage message, string json);
public delegate void ChannelUpdateDelegate(BaseChannel previous, BaseChannel current);
public delegate void MessageDeleteDelegate(string messageId, string channelId);
public delegate void MessageReactedDelegate(string userId, string react, string messageId);