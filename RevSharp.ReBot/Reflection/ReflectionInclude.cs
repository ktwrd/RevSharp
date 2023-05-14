using System.Reflection;
using RevSharp.Core;

namespace RevSharp.ReBot.Reflection;

public class ReflectionInclude
{
    public ReflectionInclude(Client client)
    {
        _client = client;
    }
    private readonly Client _client;

    public async Task Search(Assembly assembly)
    {
        Console.WriteLine($"[ReflectionInclude] Searching");
        IEnumerable<Type> typesWithAttr = from type in assembly.GetTypes()
            where type.IsDefined(typeof(RevSharpModuleAttribute), false)
                  && type.IsSubclassOf(typeof(BaseModule)) && type != null
            select type;
        foreach (var i in typesWithAttr)
        {
            var instance = (BaseModule)Activator.CreateInstance(i);
            await InitializeEvents(instance, i);
        }
        Console.WriteLine($"[ReflectionInclude] Init {typesWithAttr.Count()} modules");
    }
    private async Task InitializeEvents<T>(T item, Type type) where T : BaseModule
    {
        item.Client = _client;
        _client.MessageReceived += async (m) =>
        {
            await item.MessageReceived(m);
        };
        
        await item.Initialize();
        Console.WriteLine($"[ReflectionInclude] Init {type.Name}");
    }
}