namespace RevSharp.Core.Models;

public interface IMessageableChannel : IBaseChannel
{
    public string LastMessageId { get; set; }
}