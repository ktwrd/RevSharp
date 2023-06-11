namespace RevSharp.Core.Models;

public interface IClientable : IFetchable
{
    public Task<bool> Fetch();
}