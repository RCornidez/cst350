using Microsoft.AspNetCore.Mvc;

namespace Milestone.Controllers;

public class GameController : Controller
{
    public IActionResult StartGame()
    {
        return View();
    }
}
