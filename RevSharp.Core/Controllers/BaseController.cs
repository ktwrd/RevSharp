namespace RevSharp.Core.Controllers;

public class BaseController
{
    protected readonly Client client;

    internal BaseController(Client client)
    {
        this.client = client;
    }
}