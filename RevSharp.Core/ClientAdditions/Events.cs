using kate.shared.Helpers;
using RevSharp.Core.Helpers;

namespace RevSharp.Core;

public partial class Client
{
    public event MessageDelegate MessageReceived;
    public event VoidDelegate Ready;
    public event VoidDelegate ClientAuthenticated;
    public event StringDelegate ErrorReceived;
    public event MessageDeleteDelegate MessageDeleted;

    internal void OnMessageDeleted(string messageId, string channelId)
    {
        if (MessageDeleted != null)
        {
            MessageDeleted?.Invoke(messageId, channelId);
        }
    }
}