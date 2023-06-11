namespace RevSharp.Core.Models;

public interface IBaseChannel : ISnowflake, IFetchable, IClientable
{
    public string ChannelType { get; set; }
}