using RevSharp.Core;
using RevSharp.Core.Models;

namespace RevSharp.ReBot.Reflection;

public class BaseModule
{
    public Client Client { get; set; }
    public ReflectionInclude Reflection { get; set; }
    public virtual Task Initialize(ReflectionInclude reflection)
    {
        return Task.CompletedTask;
    }
    public virtual Task MessageReceived(Message message)
    {
        return Task.CompletedTask;
    }
}