namespace RevSharp.Core.Models;

public interface INamedChannel : IBaseChannel
{
    public string Name { get; set; }
    public string? Description { get; set; }
}