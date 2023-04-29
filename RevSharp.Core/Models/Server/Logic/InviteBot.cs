namespace RevSharp.Core.Models;

public partial class Server
{
    public Task<bool> InviteBot(Client client, string botId)
        => client.Bot.Invite(botId, Id);

    public Task<bool> InviteBot(string botId)
        => InviteBot(Client, botId);

    public Task<bool> InviteBot(Client client, Bot bot)
        => client.Bot.Invite(bot, this);

    public Task<bool> InviteBot(Bot bot)
        => InviteBot(Client, bot);
}