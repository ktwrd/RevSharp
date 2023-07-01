using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using RevSharp.Xenia.Models.ContentDetection;

namespace RevSharp.Xenia.AdminRest.Controllers;

[ApiController]
[Route("condetect")]
public class ConDetectController : Controller
{
    private async Task<AnalysisServerConfig?> GetServer(string serverId)
    {
        var collection = Program.Database.GetCollection<AnalysisServerConfig>(AnalysisServerConfig.CollectionName);
        var filter = Builders<AnalysisServerConfig>
            .Filter
            .Eq("ServerId", serverId);
        var result = await collection.FindAsync(filter);
        return result.FirstOrDefault();
    }

    private async Task SetServer(AnalysisServerConfig model)
    {
        var collection = Program.Database.GetCollection<AnalysisServerConfig>(AnalysisServerConfig.CollectionName);
        var filter = Builders<AnalysisServerConfig>
            .Filter
            .Eq("ServerId", model.ServerId);
        
        var exists = (await collection.FindAsync(filter))?.Any() ?? false;
        if (exists)
            await collection.ReplaceOneAsync(filter, model);
        else
            await collection.InsertOneAsync(model);
    }
    
    [HttpGet("server/{serverId}")]
    public async Task<ActionResult> FetchServer(string serverId)
    {
        var data = await GetServer(serverId);
        if (data == null)
        {
            Response.StatusCode = 404;
            return Content("NotFound");
        }

        return Json(data, Program.SerializerOptions);
    }
    [HttpGet("server/{serverId}/allow")]
    public async Task<ActionResult> AllowServer(string serverId)
    {
        var data = await GetServer(serverId)
            ?? new AnalysisServerConfig()
            {
                ServerId = serverId
            };
        if (data.LogChannelId.Length < 1)
        {
            return Content("LogChannelNotSet");
        }
        data.Enabled = true;
        data.AllowAnalysis = true;
        data.HasRequested = false;
        await SetServer(data);
        data = await GetServer(data.ServerId);
        return Json(data, Program.SerializerOptions);
    }

    [HttpGet("server/{serverId}/deny")]
    public async Task<ActionResult> DenyServer(string serverId)
    {
        var data = await GetServer(serverId)
           ?? new AnalysisServerConfig()
           {
               ServerId = serverId
           };
        data.Enabled = false;
        data.AllowAnalysis = false;
        data.HasRequested = false;
        await SetServer(data);
        data = await GetServer(data.ServerId);
        return Json(data, Program.SerializerOptions);
    }
    [HttpGet("server/{serverId}/ban")]
    public async Task<ActionResult> BanServer(string serverId, string reason = "<None>")
    {
        var data = await GetServer(serverId)
           ?? new AnalysisServerConfig()
           {
               ServerId = serverId
           };
        data.Enabled = false;
        data.AllowAnalysis = false;
        data.HasRequested = false;
        data.IsBanned = true;
        data.BanReason = reason;
        await SetServer(data);
        data = await GetServer(data.ServerId);
        return Json(data, Program.SerializerOptions);
    }
}