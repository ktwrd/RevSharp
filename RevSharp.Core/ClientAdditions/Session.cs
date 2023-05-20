using System.Net;
using System.Net.Http.Json;
using RevSharp.Core.Models;

namespace RevSharp.Core;

public partial class Client
{
    /// <summary>
    /// Fetch login response for a user. Can be used for C# GUI/CLI clients.
    /// </summary>
    /// <param name="username">Username/Email to login as</param>
    /// <param name="password">Password to use</param>
    /// <param name="friendlyName">What should this client identify itself as?</param>
    /// <returns><see cref="LoginSuccessResponse"/> or <see cref="LoginMFAResponse"/> or <see cref="LoginDisabledResponse"/></returns>
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