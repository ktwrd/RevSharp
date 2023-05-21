using System.ComponentModel.Design.Serialization;
using System.Reflection;
using RevSharp.Core;

namespace RevSharp.ReBot.Reflection;

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
            try
            {
                await item.MessageReceived(m);
            }
            catch (Exception e)
            {
                await m.Reply(string.Join("\n", new string[]
                {
                    $"Uncaught exception while running `{type.Name}.MessageReceived()`",
                    "```",
                    e.ToString(),
                    "```"
                }));
                Console.Error.WriteLine(e);
            }
        };
        
        await item.Initialize(this);
        Log.Debug($"[ReflectionInclude] Init {type.Name}");
    }
}