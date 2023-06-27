using RevSharp.Core.Models;
using RevSharp.Xenia.Controllers;
using RevSharp.Xenia.Helpers;

namespace RevSharp.Xenia.Reflection;

public abstract class CommandModule : BaseModule
{
    public virtual Task CommandReceived(CommandInfo info, Message message)
    {
        return Task.CompletedTask;
    }

    internal override void InitEvents()
    {
        base.InitEvents();
        var errorController = Reflection.FetchModule<ErrorReportController>();
        var type = this.GetType();
        Client.MessageReceived += async (m) =>
        {
            if (m.AuthorId == Client.CurrentUserId || m.IsSystemMessage)
                return;
            try
            {
                await Client_HandleCommand(m);
            }
            catch (Exception e)
            {
                if (errorController != null)
                    await errorController.Report(e, m, $"Failed to run {type.Name}.Client_HandleCommand");
                else
                    Log.Error($"Failed to run {type.Name}.Client_HandleCommand()\n{e}");
                try
                {
                    await m.Reply(
                        string.Join(
                            "\n", new string[]
                            {
                                $"Uncaught exception while running `{type.Name}.CommandReceived()`", "```",
                                e.ToString().Substring(0, Math.Min(e.ToString().Length, 2000)), "```"
                            }));
                    await ReportError(e, m, $"Uncaught exception while running `{type.Name}.CommandReceived()`");
                }
                catch { }
            }
        };
    }
    private async Task Client_HandleCommand(Message message)
    {
        var errorController = Reflection.FetchModule<ErrorReportController>();
        var type = this.GetType();

        bool hasContent = message.Content?.Length > 0;
        bool notSelf = message.AuthorId != Client.CurrentUserId;
        bool startsWithPrefix = (message.Content ?? "").StartsWith(Reflection.Config.Prefix);
        if (BaseCommandName == null || !notSelf || !hasContent || !startsWithPrefix)
            return;

        var commandInfo = CommandHelper.FetchInfo(Reflection, message);
        if (commandInfo == null || commandInfo.Command != BaseCommandName)
            return;
         
        var author = await Client.GetUser(message.AuthorId, forceUpdate: false);
        if (author == null || author.IsBot)
            return;

        var server = await message.FetchServer(false);
        if (ServerOnly && server == null)
        {
            await message.Reply($"This command can only be used in servers.");
            return;
        }
        if (server != null && RequirePermission != null)
        {
            var flag = (PermissionFlag)RequirePermission;
            var member = await server.GetMember(author.Id, false);
            if (member != null)
            {
                var hasPerm = await member.HasPermission(Client, flag, forceUpdate: false);
                if (!hasPerm)
                {
                    await message.Reply($"Missing server permission `{flag}`");
                    return;
                }
            }
        }
        try
        {
            await CommandReceived(commandInfo, message);
        }
        catch (Exception ex)
        {
            if (errorController != null)
                await errorController.Report(ex, message, $"Failed to run {type.Name}.CommandReceived");
            else
                Log.Error($"Failed to run {type.Name}.CommandReceived()\n{ex}");
            await message.Reply(string.Join("\n", new string[]
            {
                $"Uncaught exception while running `{type.Name}.CommandReceived()`",
                "```",
                ex.ToString().Substring(0, Math.Min(ex.ToString().Length, 2000)),
                "```"
            }));
            return;
        }

        var bch = await Client.GetChannel(message.ChannelId);
        INamedChannel? channel = null;
        if (bch is INamedChannel namedChannel)
            channel = namedChannel;
        Reflection.OnCommandExecuteTrigger(
            server,
            author,
            bch,
            channel,
            commandInfo,
            this);
    }

    /// <summary>
    /// Content that is displayed in the Help command
    /// </summary>
    /// <returns>Content for Help command. When `null` this module will be ignored</returns>
    public abstract string? HelpContent();
    
    /// <summary>
    /// Display in the help command
    /// </summary>
    public abstract bool HasHelpContent { get; }
    /// <summary>
    /// What category does this module belong to? Defaults to `null` which is the `other` category.
    /// </summary>
    public abstract string? HelpCategory { get; }
    /// <summary>
    /// Base command name to call <see cref="CommandReceived(CommandInfo, Message)"/>
    /// </summary>
    public abstract string? BaseCommandName { get; }
    /// <summary>
    /// What server permission does this module require. When `null`, no server permissions will be checked.
    /// </summary>
    public virtual PermissionFlag? RequirePermission => null;
    /// <summary>
    /// When enabled, the <see cref="CommandReceived(CommandInfo, Message)"/> method will only be called when a message is sent in a server.
    /// </summary>
    public virtual bool ServerOnly => false;
}