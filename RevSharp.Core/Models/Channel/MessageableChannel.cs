using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

/// <summary>
/// A channel that can have messages in it
/// </summary>
public class MessageableChannel : NamedChannel, IMessageableChannel
{
    /// <summary>
    /// Id of the last message sent in this channel
    /// </summary>
    [JsonPropertyName("last_message_id")]
    public string LastMessageId { get; set; }
    
    
    internal Dictionary<string, Message> MessageCache { get; private set; }
    
    #region Constructors
    public MessageableChannel()
        : this(null, "")
    {}
    public MessageableChannel(string id)
        : this(null, id)
    {}
    internal MessageableChannel(Client? client, string id)
        : base(client, id)
    {
        MessageCache = new Dictionary<string, Message>();
        MessageReceived += MessageReceivedHandle;
    }
    #endregion

    protected override void ClientInit()
    {
        base.ClientInit();
        if (Client == null)
            return;

        MessageReceived += MessageReceivedHandle;
    }

    private void MessageReceivedHandle(Message message)
    {
        if (message == null)
            return;
        if (MessageCache.ContainsKey(message.Id))
        {
            MessageCache[message.Id] = message;
            return;
        }
        
        MessageCache.Add(message.Id, message);
    }
}