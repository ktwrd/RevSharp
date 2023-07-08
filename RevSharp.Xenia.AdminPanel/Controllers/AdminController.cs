using Microsoft.AspNetCore.Mvc;
using RevSharp.Core;
using RevSharp.Core.Models;

namespace RevSharp.Xenia.AdminPanel.Controllers;

public class AdminController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> UserDetails(string? id)
    {
        if (id == null)
            return NotFound();

        User? user;
        try
        {
            user = await Program.RevoltClient.GetUser(id);
        }
        catch (RevoltException rex)
        {
            return View("RevoltError", rex);
        }

        if (user == null)
        {
            return NotFound();
        }
        return View("UserDetails", user);
    }
}