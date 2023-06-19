using System.ComponentModel.Design.Serialization;
using System.Reflection;
using RevSharp.Core;
using RevSharp.Core.Models;
using RevSharp.Skidbot.Controllers;
using RevSharp.Skidbot.Helpers;

namespace RevSharp.Skidbot.Reflection;

public class ReflectionInclude
{
    public ReflectionInclude(Client client)
    {
        _client = client;
        Modules = new List<BaseModule>();
    }
    private readonly Client _client;

    private List<BaseModule> Modules { get; set; }
    public async Task Search(Assembly assembly)
    {
        Log.Debug($"[ReflectionInclude] Searching");
        IEnumerable<Type> typesWithAttr = from type in assembly.GetTypes()
            where type.IsDefined(typeof(RevSharpModuleAttribute), false)
                  && type.IsSubclassOf(typeof(BaseModule)) && type != null
            select type;
        foreach (var i in typesWithAttr)
        {
            var instance = (BaseModule)Activator.CreateInstance(i);
            instance.Reflection = this;
            Modules.Add(instance);
            // await InitializeEvents(instance, i);
        }

        foreach (var i in Modules)
            await InitializeEvents(i, i.GetType());
        Log.Debug($"[ReflectionInclude] Init {typesWithAttr.Count()} modules");
    }

    public T? FetchModule<T>() where T : BaseModule
    {
        foreach (var item in Modules)
        {
            var type = item.GetType();
            var genericType = typeof(T);
            if (type.FullName == genericType.FullName)
                return (T)item;
        }

        return null;
    }

    public BaseModule[] FetchModules()
    {
        return Modules.ToArray();
    }
    private async Task InitializeEvents<T>(T item, Type type) where T : BaseModule
    {
        item.Client = _client;
        _client.MessageReceived += async (m) =>
        {
            if (m.AuthorId != _client.CurrentUserId)
            {
                try
                {
                    if (item.BaseCommandName != null && m.AuthorId != _client.CurrentUserId && m.SystemMessage == null && m.Content?.Length > 2)
                    {
                        var commandInfo = CommandHelper.FetchInfo(m);
                        if (commandInfo != null && commandInfo.Command == item.BaseCommandName)
                        {
                            var author = await _client.GetUser(m.AuthorId);
                            if (author != null && author.Bot == null)
                            {
                                await item.CommandReceived(commandInfo, m);

                                var statControl = FetchModule<StatisticController>();
                                var server = await m.FetchServer();
                                var authorName = "<None>";
                                if (author != null)
                                    authorName = $"{author.Username}#{author.Discriminator}";

                                var bch = await _client.GetChannel(m.ChannelId);
                                INamedChannel? channel = null;
                                if (bch is INamedChannel)
                                    channel = (INamedChannel)bch;

                                statControl.CommandCounter.WithLabels(new string[]
                                {
                            server?.Name ?? "<None>",
                            server?.Id ?? "<None>",
                            authorName,
                            author?.Id ?? "<None>",
                            channel?.Name ?? "<None>",
                            bch?.Id ?? "<None>",
                            commandInfo.Command,
                            string.Join(" ", commandInfo.Arguments),
                            item.HelpCategory ?? "<None>"
                                }).Inc();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to run {type.Name}.CommandReceived()\n{e}");
                    await m.Reply(string.Join("\n", new string[]
                    {
                    $"Uncaught exception while running `{type.Name}.CommandReceived()`",
                    "```",
                    e.ToString().Substring(0, Math.Min(e.ToString().Length, 2000)),
                    "```"
                    }));
                }
                try
                {
                    await item.MessageReceived(m);
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to run {type.Name}.MessageReceived()\n{e}");
                    await m.Reply(string.Join("\n", new string[]
                    {
                    $"Uncaught exception while running `{type.Name}.MessageReceived()`",
                    "```",
                    e.ToString().Substring(0, Math.Min(e.ToString().Length, 2000)),
                    "```"
                    }));
                }
            }
        };
        try
        {
            await item.Initialize(this);
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to initialize {type.Name}\n{ex}");
        }
        Log.Debug($"[ReflectionInclude] Init {type.Name}");
    }
}