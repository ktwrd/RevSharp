using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using RevSharp.Core;
using RevSharp.Core.Models;
using RevSharp.Xenia.AdminPanel.Models;
using RevSharp.Xenia.Models.ContentDetection;

namespace RevSharp.Xenia.AdminPanel.Controllers;

public class CDAdminController : Controller
{
    private readonly ILogger<CDAdminController> _logger;

    public CDAdminController(ILogger<CDAdminController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        var cfg = new List<AnalysisServerConfig>(GetAllConfigs());
        var model = new CDAdminViewModel(this)
        {
            ServerConfigs = cfg
        };
        return View("Index", model);
    }

    public async Task<IActionResult> Details(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        Server? server;
        try
        {
            server = await Program.RevoltClient.GetServer(id);
        }
        catch (RevoltException rex)
        {
            return View("RevoltError", rex);
        }

        if (server == null)
        {
            return NotFound();
        }
        var config = GetConfig(id)
            ?? new AnalysisServerConfig()
            {
                ServerId = id  
            };
        var model = new CDAdminDetailModel()
        {
            Server = server,
            Config = config
        };
        return View("Details", model);
    }

    public IEnumerable<AnalysisServerConfig> GetAllConfigs()
    {
        var collection = Program.Database.GetCollection<AnalysisServerConfig>(AnalysisServerConfig.CollectionName);
        var filter = Builders<AnalysisServerConfig>
            .Filter.Empty;
        var result = collection.FindAsync(filter).Result;
        return result.ToList();
    }

    public AnalysisServerConfig? GetConfig(string serverId)
    {
        var collection = Program.Database.GetCollection<AnalysisServerConfig>(AnalysisServerConfig.CollectionName);
        var filter = Builders<AnalysisServerConfig>
            .Filter.Eq("ServerId", serverId);
        var result = collection.FindAsync(filter).Result;
        return result.FirstOrDefault();
    }
}