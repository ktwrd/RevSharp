namespace RevSharp.Core.Models;

public interface IFetchable
{
    public Task<bool> Fetch(Client client);
}
public interface ISnowflake
{
    /// <summary>
    /// Unique Id
    /// </summary>
    string Id { get; }
}