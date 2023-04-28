namespace RevSharp.Core.Models;

public class Clientable
{
    internal Client? Client { get; private set; }
    
    internal Clientable(Client client)
    {
        Client = client;
    }

    public Clientable()
    {
        Client = null;
    }
}