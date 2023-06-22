using System.ComponentModel.Design.Serialization;
using System.Reflection;
using RevSharp.Core;
using RevSharp.Core.Models;
using RevSharp.Skidbot.Helpers;

namespace RevSharp.Skidbot.Reflection;

public class ReflectionInclude
{
    public ReflectionInclude(Client client)
    {
        _client = client;
        Modules = new List<BaseModule>();
        LoadedInstanceNames = new List<string>();
        LoadedAssemblyNames = new List<string>();
    }
    private readonly Client _client;

    private List<BaseModule> Modules { get; set; }
    private List<string> LoadedInstanceNames { get; set; }
    private List<string> LoadedAssemblyNames { get; set; }
    public async Task Search(Assembly assembly)
    {
        LoadedAssemblyNames.Add(assembly.FullName.Split(',')[0]);
        Log.Debug($"[ReflectionInclude] Searching");
        IEnumerable<Type> typesWithAttr = from type in assembly.GetTypes()
            where type.IsDefined(typeof(RevSharpModuleAttribute), false)
                  && type.IsSubclassOf(typeof(BaseModule)) && type != null
            select type;
        var localMods = new List<BaseModule>();
        foreach (var i in typesWithAttr)
        {
            if (LoadedInstanceNames.Contains(i.FullName))
                continue;
            var instance = (BaseModule)Activator.CreateInstance(i);
            instance.Reflection = this;
            Modules.Add(instance);
            localMods.Add(instance);
            LoadedInstanceNames.Add(i.FullName);
            // await InitializeEvents(instance, i);
        }

        foreach (var i in localMods)
            await InitializeEvents(i, i.GetType());
        Log.Debug($"[ReflectionInclude] Init {typesWithAttr.Count()} modules");
    }

    public async Task SearchFinale()
    {
        await Task.WhenAll(InitAsyncQueue);
        var initCompleteQueue = new List<Task>();
        foreach (var i in Modules)
            initCompleteQueue.Add(i.InitComplete());
        await Task.WhenAll(initCompleteQueue);
    }

    public string[] GetPlugins()
    {
        var items = new List<string>();
        foreach (var item in LoadedAssemblyNames)
            if (item.StartsWith("RevSharp.Skidbot"))
                items.Add(item);
        return items.ToArray();
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

    private List<Task> InitAsyncQueue = new List<Task>();
    public event CommandExecuteDelegate CommandExecuteTrigger;
    public delegate void CommandExecuteDelegate(Server? server, User? author, BaseChannel? channel, INamedChannel? namedChannel, CommandInfo info, BaseModule module);
    public ConfigData Config { get; init; }
    private async Task InitializeEvents<T>(T item, Type type) where T : BaseModule
    {
        item.Client = _client;
        _client.MessageReceived += async (m) =>
        {
            if (m.AuthorId != _client.CurrentUserId && !m.IsSystemMessage)
            {
                try
                {
                    bool hasContent = m.Content?.Length > 0;
                    bool notSelf = m.AuthorId != _client.CurrentUserId;
                    bool startsWithPrefix = (m.Content ?? "").StartsWith(Config.Prefix);
                    if (item.BaseCommandName != null && notSelf && hasContent && startsWithPrefix)
                    {
                        var commandInfo = CommandHelper.FetchInfo(this, m);
                        if (commandInfo != null && commandInfo.Command == item.BaseCommandName)
                        {
                            var author = await _client.GetUser(m.AuthorId, forceUpdate: false);
                            if (author is not { IsBot: true })
                            {
                                await item.CommandReceived(commandInfo, m);

                                var server = await m.FetchServer();
                                var authorName = "<None>";
                                if (author != null)
                                    authorName = $"{author.Username}#{author.Discriminator}";

                                var bch = await _client.GetChannel(m.ChannelId);
                                INamedChannel? channel = null;
                                if (bch is INamedChannel namedChannel)
                                    channel = namedChannel;
                                CommandExecuteTrigger?.Invoke(
                                    server,
                                    author,
                                    bch,
                                    channel,
                                    commandInfo,
                                    item);
                                /*var statControl = FetchModule<StatisticController>();
                                statControl?.CommandCounter.WithLabels(new string[]
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
                                }).Inc();*/
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to run {type.Name}.CommandReceived()\n{e}");
                    try
                    {
                        await m.Reply(
                            string.Join(
                                "\n", new string[]
                                {
                                    $"Uncaught exception while running `{type.Name}.CommandReceived()`", "```",
                                    e.ToString().Substring(0, Math.Min(e.ToString().Length, 2000)), "```"
                                }));
                    } catch{}
                }
                try
                {
                    await item.MessageReceived(m);
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to run {type.Name}.MessageReceived()\n{e}");
                    try
                    {
                        await m.Reply(string.Join("\n", new string[]
                        {
                            $"Uncaught exception while running `{type.Name}.MessageReceived()`",
                            "```",
                            e.ToString().Substring(0, Math.Min(e.ToString().Length, 2000)),
                            "```"
                        }));
                    }
                    catch
                    { }

                }
            }
        };
        try
        {
            if (item.WaitForInit)
            {
                await item.Initialize(this);
            }
            else
            {
                InitAsyncQueue.Add(item.Initialize(this));
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to initialize {type.Name}\n{ex}");
        }
        Log.Debug($"[ReflectionInclude] Init {type.Name}");
    }
}