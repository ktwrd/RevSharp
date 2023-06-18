using System.Net;
using System.Text.Json;

namespace RevSharp.Core.Models;

public partial class User
{
    public async Task<bool> Edit(Client client, UserUpdateData data)
    {
        var ser = JsonSerializer.Serialize(data, Client.PutSerializerOptions);
        var con = new StringContent(ser, null, "application/json");
        var url = Client.SEndpoint.User(Id);
        var response = await client.PatchAsync(url, con);
        if (response.StatusCode != HttpStatusCode.OK)
            return false;

        return await Fetch(client);
    }

    public Task<bool> Edit(UserUpdateData data) => Edit(Client, data);

    public Task<bool> UpdatePresence(Client client, string text, UserPresence status)
    {
        return Edit(
            Client, new UserUpdateData()
            {
                Status = new UserUpdateStatusData()
                {
                    Text = text,
                    PresenceString = status.ToString()
                }
            });
    }

    public Task<bool> UpdatePresence(string text, UserPresence status) => UpdatePresence(Client, text, status);
}