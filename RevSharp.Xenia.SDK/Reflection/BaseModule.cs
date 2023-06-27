using RevSharp.Core;
using RevSharp.Core.Models;
using RevSharp.Xenia.Controllers;
using RevSharp.Xenia.Helpers;

namespace RevSharp.Xenia.Reflection;

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

    internal virtual void InitEvents()
    {
        var errorController = Reflection.FetchModule<ErrorReportController>();
        var type = this.GetType();
        Client.MessageReceived += async (m) =>
        {
            try
            {
                await MessageReceived(m);
            }
            catch (Exception ex)
            {
                if (errorController != null)
                    await errorController.Report(ex, m, $"Failed to run {type.Name}.MessageReceived");
                else
                    Log.Error($"Failed to run {type.Name}.MessageReceived()\n{ex}");
                try
                {
                    await m.Reply(string.Join("\n", new string[]
                    {
                        $"Uncaught exception while running `{type.Name}.MessageReceived()`",
                        "```",
                        ex.ToString().Substring(0, Math.Min(ex.ToString().Length, 2000)),
                        "```"
                    }));
                    await ReportError(ex, m, $"Uncaught exception while running `{type.Name}.MessageReceived()`");
                }
                catch
                { }
            }
        };
    }

    public async Task ReportError(Exception exception, Message? message, string content)
    {
        var controller = Reflection.FetchModule<ErrorReportController>();
        if (controller == null)
            return;

        await controller.Report(exception, message, content);
    }
    public Task ReportError(Exception exception, Message message) => ReportError(exception, message, "");
    public Task ReportError(Exception exception) => ReportError(exception, null, "");

    /// <summary>
    /// Wait for <see cref="Initialize(ReflectionInclude)"/> to complete before calling the next module to init.
    /// </summary>
    public virtual bool WaitForInit => true;
}