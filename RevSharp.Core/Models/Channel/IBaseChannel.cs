namespace RevSharp.Core.Models;

public interface IBaseChannel : ISnowflake, IFetchable, IClientable
{
    public string Id { get; set; }
    public string ChannelType { get; set; }
}