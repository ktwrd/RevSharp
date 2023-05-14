using System.Net;
using System.Net.Http.Json;

namespace RevSharp.Core.Models;

public partial class User
{
    internal async Task<bool> SetBlockState(Client client, bool block)
    {
        if (block)
        {
            var response = await client.PutAsync($"/users/{Id}/block");
            await Fetch(client);
            return response.StatusCode == HttpStatusCode.OK;
        }
        else
        {
            var response = await client.DeleteAsync($"/users/{Id}/block");
            await Fetch(client);
            return response.StatusCode == HttpStatusCode.OK;
        }
    }
}