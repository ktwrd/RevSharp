using RevSharp.Core;
using RevSharp.Core.Models;
using RevSharp.Skidbot.Helpers;

namespace RevSharp.Skidbot.Reflection;

public class BaseModule
{
    /// <summary>
    /// RevSharp Client
    /// </summary>
    public Client Client { get; set; }
    public ReflectionInclude Reflection { get; set; }
    /// <summary>
    /// Called when all module instances are created
    /// </summary>
    /// <param name="reflection">Access to the module creation manager</param>
    public virtual Task Initialize(ReflectionInclude reflection)
    {
        return Task.CompletedTask;
    }

    public virtual Task InitComplete()
    {
        return Task.CompletedTask;
    }
    /// <summary>
    /// Called when the <see cref="RevSharp.Core.Client.MessageReceived"/> event is emitted.
    /// </summary>
    /// <param name="message">Message data</param>
    public virtual Task MessageReceived(Message message)
    {
        return Task.CompletedTask;
    }

    public virtual Task CommandReceived(CommandInfo command, Message message)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Content that is displayed in the Help command
    /// </summary>
    /// <returns>Content for Help command. When `null` this module will be ignored</returns>
    public virtual string? HelpContent()
    {
        return null;
    }

    /// <summary>
    /// Display in the help command
    /// </summary>
    public virtual bool HasHelpContent => false;
    /// <summary>
    /// Internal name for command. Shown as module name in help command
    /// </summary>
    public virtual string? InternalName => null;
    /// <summary>
    /// What category does this module belong to? Defaults to `null` which is the `other` category.
    /// </summary>
    public virtual string? HelpCategory => null;
    public virtual string? BaseCommandName => null;
    public virtual bool WaitForInit => true;
    public virtual PermissionFlag? RequireServerPermission => null;
}