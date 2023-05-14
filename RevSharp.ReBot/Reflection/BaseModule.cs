using RevSharp.Core;
using RevSharp.Core.Models;

namespace RevSharp.ReBot.Reflection;

public class BaseModule
{
    public Client Client { get; set; }
    public virtual Task Initialize()
    {
        return Task.CompletedTask;
    }
    public virtual Task MessageReceived(Message message)
    {
        return Task.CompletedTask;
    }
}