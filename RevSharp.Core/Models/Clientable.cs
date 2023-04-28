namespace RevSharp.Core.Models;

public class Clientable
{
    internal Client? _client { get; private set; }

    public Clientable(Client client)
    {
        _client = client;
    }

    public Clientable()
    {
        _client = null;
    }
}