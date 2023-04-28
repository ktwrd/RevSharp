using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

/// <summary>
/// A channel that can have messages in it
/// </summary>
public class MessageableChannel : BaseChannel, Clientable, ISnowflake
{
    internal Dictionary<string, Message> MessageCache { get; private set; }
    
    #region Constructors
    public MessageableChannel()
        : this(null, "")
    {}
    public MessageableChannel(string id)
        : this(null, id)
    {}

    internal MessageableChannel(Client client, string id)
        : base(client, id)
    {
        MessageCache = new Dictionary<string, Message>();
    }
    #endregion
}