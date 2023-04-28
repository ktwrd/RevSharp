using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

/// <summary>
/// A channel that can have messages in it
/// </summary>
public class MessageableChannel : BaseChannel
{
    internal Dictionary<string, Message> MessageCache { get; private set; }
    
    #region Constructors
    public MessageableChannel()
        : base(null, "")
    {}
    public MessageableChannel(string id)
        : base(null, id)
    {}
    internal MessageableChannel(Client? client, string id)
        : base(client, id)
    {
        MessageCache = new Dictionary<string, Message>();
        MessageReceived += MessageReceivedHandle;
    }
    #endregion

    private void MessageReceivedHandle(Message message)
    {
        if (MessageCache.ContainsKey(message.Id))
        {
            MessageCache[message.Id] = message;
            return;
        }
        
        MessageCache.Add(message.Id, message);
    }
}