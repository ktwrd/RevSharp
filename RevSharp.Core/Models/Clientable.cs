namespace RevSharp.Core.Models;

public class Clientable
{
    internal Client? Client { get; private set; }
    
    public Clientable(Client client)
    {
        Client = client;
    }

    public Clientable()
    {
        _client = null;
        Client = null;
    }
}