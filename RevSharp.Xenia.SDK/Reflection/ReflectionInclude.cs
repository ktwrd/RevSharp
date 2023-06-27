using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using kate.shared.Extensions;
using Newtonsoft.Json.Linq;
using RevSharp.Core;
using RevSharp.Core.Models;
using RevSharp.Xenia.Helpers;
using RevSharp.Xenia.Models;

namespace RevSharp.Xenia.Reflection;

public class ReflectionInclude
{
    public ReflectionInclude(Client client)
    {
        _client = client;
        Modules = new List<BaseModule>();
        LoadedInstanceNames = new List<string>();
        LoadedAssemblyNames = new List<string>();
        LoadedAssemblies = new List<Assembly>();
    }
    private readonly Client _client;
    public ConfigData Config { get; init; }
    public string ConfigContent { get; init; }

    private List<BaseModule> Modules { get; set; }
    private List<string> LoadedInstanceNames { get; set; }
    private List<string> LoadedAssemblyNames { get; set; }
    private List<Assembly> LoadedAssemblies { get; set; }
    public async Task Search(Assembly assembly)
    {
        var assemblyName = assembly.FullName.Split(',')[0];
        if (FeatureFlags.PluginWhitelist.Length > 0)
        {
            if (!FeatureFlags.PluginWhitelist.Contains(assemblyName))
                return;
        }
        LoadedAssemblyNames.Add(assembly.FullName.Split(',')[0]);
        LoadedAssemblies.Add(assembly);
        Log.WriteLine($"[ReflectionInclude] Searching");
        IEnumerable<Type> typesWithAttr = from type in assembly.GetTypes()
            where type.IsDefined(typeof(RevSharpModuleAttribute), false)
                  && type.IsSubclassOf(typeof(BaseModule)) && type != null
            select type;
        var localMods = new List<BaseModule>();
        foreach (var i in typesWithAttr)
        {
            if (LoadedInstanceNames.Contains(i.FullName))
                continue;
            Log.WriteLine($"Loading {i.FullName}");
            var instance = (BaseModule)Activator.CreateInstance(i);
            instance.Reflection = this;
            Modules.Add(instance);
            localMods.Add(instance);
            LoadedInstanceNames.Add(i.FullName);
            // await InitializeEvents(instance, i);
        }

        foreach (var i in localMods)
            await InitializeEvents(i, i.GetType());
        Log.WriteLine($"[ReflectionInclude] Init {typesWithAttr.Count()} modules");
    }

    public async Task SearchFinale()
    {
        await Task.WhenAll(InitAsyncQueue);
        var initCompleteQueue = new List<Task>();
        foreach (var i in Modules)
            initCompleteQueue.Add(i.InitComplete());
        await Task.WhenAll(initCompleteQueue);
    }

    public T? GetConfig<T>(string name)
    {
        var jobj = JObject.Parse(ConfigContent);
        var items = jobj["Configs"]?.ToArray();
        for (int i = 0; i < Config.Configs.Length; i++)
        {
            if (name == Config.Configs[i].Name)
            {
                var str = items[i].ToString();
                var des = JsonSerializer.Deserialize<T>(str, Client.SerializerOptions);
                return des;
            }
        }

        return default;
    }
    
    public string[] GetPlugins(bool includeVersion = false)
    {
        var items = new List<string>();
        foreach (var item in LoadedAssemblies)
            if (item.FullName.StartsWith("RevSharp.Xenia"))
            {
                var name = item.FullName.Split(',')[0];
                if (includeVersion)
                {
                    var versionInfo = item.GetName().Version;
                    if (versionInfo != null)
                        items.Add($"{name} v{versionInfo}");
                    else
                        items.Add(name);
                }
                else
                    items.Add(name);
            }
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

    public T[] FetchModules<T>() where T : BaseModule
    {
        var items = new List<T>();
        foreach (var item in Modules)
        {
            if (item.GetType().IsSubclassOf(typeof(T)))
            {
                items.Add((T)item);
            }
        }

        return items.ToArray();
    }

    public event CommandExecuteDelegate CommandExecuteTrigger;
    internal void OnCommandExecuteTrigger(
        Server? server,
        User? author,
        BaseChannel? channel,
        INamedChannel? namedChannel,
        CommandInfo info,
        BaseModule module)
    {
        CommandExecuteTrigger?.Invoke(server, author, channel, namedChannel, info, module);
    }
    private List<Task> InitAsyncQueue = new List<Task>();
    private async Task InitializeEvents<T>(T item, Type type) where T : BaseModule
    {
        item.Client = _client;
        item.InitEvents();
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