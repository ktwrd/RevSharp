namespace RevSharp.Core.Models.WebSocket;

public class TypingSendEvent : ChannelIdEvent
{
    public TypingSendEvent(string channel)
        : base("BeginTyping", channel)
    {
    }
}