using System.Net;
using System.Net.Http.Json;
using RevSharp.Core.Models;

namespace RevSharp.Core;

public partial class Client
{
    public async Task<LoginBaseResponse?> SessionLogin(string username, string password, string? friendlyName = null)
    {
        var pushData = new LoginRequestData()
        {
            Email = username,
            Password = password,
            FriendlyName = friendlyName
        };
        var response = await PostAsync("/auth/session/login", JsonContent.Create(pushData, options: SerializerOptions));
        if (response.StatusCode != HttpStatusCode.OK)
            return null;

        var stringContent = response.Content.ReadAsStringAsync().Result;
        return LoginBaseResponse.Parse(stringContent);
    }
}