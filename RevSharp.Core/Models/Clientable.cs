namespace RevSharp.Core.Models;

public class Clientable
{
    private Client? _client = null;
    internal Client? Client
    {
        get => _client;
        set
        {
            if (value != null)
                ClientInit();
            _client = value;
        }
    }

    protected virtual void ClientInit()
    {
        // throw new NotImplementedException();
    }
    
    internal Clientable(Client? client)
    {
        Client = client;
    }

    public Clientable()
    {
        Client = null;
    }
}