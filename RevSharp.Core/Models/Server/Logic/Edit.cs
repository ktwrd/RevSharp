using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace RevSharp.Core.Models;

public partial class Server
{
    /// <summary>
    /// Edit Server
    /// </summary>
    /// <returns>Was it successful</returns>
    public async Task<bool> Edit(Client client, ServerData data)
    {
        var response = await client.PatchAsync($"/servers/{Id}", JsonContent.Create(data, options: Client.SerializerOptions));
        if (response.StatusCode != HttpStatusCode.OK)
            return false;
        var stringContent = response.Content.ReadAsStringAsync().Result;
        var deser = JsonSerializer.Deserialize<Server>(stringContent, Client.SerializerOptions);
        if (deser == null)
            return false;
        Inject(deser, this);
        return true;
    }
    /// <summary>
    /// Edit Server
    /// </summary>
    /// <returns>Was it successful</returns>
    public Task<bool> Edit(ServerData data)
        => Edit(Client, data);

    /// <summary>
    /// Edit Server
    /// </summary>
    /// <returns>Was it successful</returns>
    public Task<bool> Edit(Client client,
        string? name = null,
        string? description = null,
        string? icon = null,
        string? banner = null,
        ServerCategory[]? categories = null,
        int? flags = null,
        bool? discoverable = null,
        bool? analytics = null,
        string[]? remove = null)
    {
        return Edit(client,
            new ServerData()
            {
                Name = name,
                Description = description,
                Icon = icon,
                Banner = banner,
                Categories = categories,
                Flags = flags,
                Discoverable = discoverable,
                Analytics = analytics,
                Remove = remove
            });
    }

    /// <summary>
    /// Edit Server
    /// </summary>
    /// <returns>Was it successful</returns>
    public Task<bool> Edit(string? name = null,
        string? description = null,
        string? icon = null,
        string? banner = null,
        ServerCategory[]? categories = null,
        int? flags = null,
        bool? discoverable = null,
        bool? analytics = null,
        string[]? remove = null)
    {
        return Edit(Client,
            new ServerData()
            {
                Name = name,
                Description = description,
                Icon = icon,
                Banner = banner,
                Categories = categories,
                Flags = flags,
                Discoverable = discoverable,
                Analytics = analytics,
                Remove = remove
            });
    }
}