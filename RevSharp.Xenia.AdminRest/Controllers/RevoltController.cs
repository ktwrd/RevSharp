using Microsoft.AspNetCore.Mvc;

namespace RevSharp.Xenia.AdminRest.Controllers;

[ApiController]
[Route("revolt")]
public class RevoltController : Controller
{
    [HttpGet("server/{serverId}")]
    public async Task<ActionResult> GetServer(string serverId)
    {
        var data = await Program.RevoltClient.GetServer(serverId, true);
        return Json(data, Program.SerializerOptions);
    }
}