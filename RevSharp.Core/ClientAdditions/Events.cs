using kate.shared.Helpers;
using RevSharp.Core.Helpers;
using RevSharp.Core.Models;

namespace RevSharp.Core;

public partial class Client
{
    
    
    public event VoidDelegate Ready;
    internal void OnReady()
    {
        Ready?.Invoke();
    }
    
    public event VoidDelegate ClientAuthenticated;
    internal void OnClientAuthenticated()
    {
        ClientAuthenticated?.Invoke();
    }
    
    public event StringDelegate ErrorReceived;
    internal void OnErrorReceived(string error)
    {
        ErrorReceived?.Invoke(error);
    }

}