using System.Text.Json;
using RevSharp.ReBot.Reflection;

namespace RevSharp.ReBot.Modules;

[RevSharpModule]
public class EconController : BaseModule
{
    public string Location => Path.Combine(Directory.GetCurrentDirectory(), "econ.json");

    private List<EconProfile> _profiles = new List<EconProfile>();
    public void Save()
    {
        var content = JsonSerializer.Serialize(_profiles, Program.SerializerOptions);
        File.WriteAllText(Location, content);
    }

    public override Task Initialize(ReflectionInclude reflection)
    {
        if (File.Exists(Location))
            _profiles = JsonSerializer.Deserialize<List<EconProfile>>(File.ReadAllText(Location),
                Program.SerializerOptions);
        return base.Initialize(reflection);
    }

    public EconProfile? GetUser(string userId, string serverId)
    {
        foreach (var item in _profiles)
            if (item.UserId == userId && item.ServerId == serverId)
                return item;
        return null;
    }

    public void SetUser(EconProfile model)
    {
        for (int i = 0; i < _profiles.Count; i++)
        {
            if (_profiles[i].UserId == model.UserId)
            {
                _profiles[i] = model;
                return;
            }
        }
        _profiles.Add(model);
        Save();
    }
}

public class EconProfile
{
    public string UserId { get; set; }
    public string ServerId { get; set; }
    public long Coins { get; set; }
    public long LastDailyTimestamp { get; set; }
}