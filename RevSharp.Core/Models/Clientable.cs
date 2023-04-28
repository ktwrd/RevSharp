namespace RevSharp.Core.Models;

public class Clientable
{
    internal Client? _client { get; private set; }
    protected bool HasClient { get; private set; }

    public Clientable(Client client)
    {
        _client = client;
        HasClient = true;
    }

    public Clientable()
    {
        _client = null;
        HasClient = false;
    }
}