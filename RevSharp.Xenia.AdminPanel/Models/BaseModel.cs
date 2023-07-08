using RevSharp.Core.Models;

namespace RevSharp.Xenia.AdminPanel.Models;

public class BaseModel
{
    public string GetServerName(string serverId)
    {
        return GetServer(serverId)?.Name ?? "null";
    }

    public Server? GetServer(string serverId)
    {
        try
        {
            var server = Program.RevoltClient.GetServer(serverId).Result;
            return server;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public User? GetUser(string userId)
    {
        var user = Program.RevoltClient.GetUser(userId).Result;
        return user;
    }

    public string FormatBool(bool data)
    {
        return data ? "✔️" : "❌";
    }
}