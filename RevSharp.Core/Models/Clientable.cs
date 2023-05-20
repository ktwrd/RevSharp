namespace RevSharp.Core.Models;

/// <summary>
/// A useful base class that is used for things that interact with <see cref="Client"/> and that have methods that need to interact with <see cref="Client"/>.
/// </summary>
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

    /// <summary>
    /// Called when <see cref="Client"/> is set to something that isn't `null`
    /// </summary>
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