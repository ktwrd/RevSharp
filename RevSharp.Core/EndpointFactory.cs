namespace RevSharp.Core;

internal partial class EndpointFactory
{
    internal Client Client { get; set; }
    private string? ProvidedEndpoint = null;
    internal string BaseUrl => ProvidedEndpoint ?? Client.Endpoint;

    internal EndpointFactory(Client client)
    {
        Client = client;
    }

    internal EndpointFactory(string end)
    {
        ProvidedEndpoint = end;
    }
}