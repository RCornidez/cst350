using Microsoft.AspNetCore.Mvc;
using Milestone.Interfaces;
using Milestone.Models;

namespace Milestone.Controllers;

public class GameController : Controller
{
    private readonly IGameService _gameService;

    public GameController(IGameService gameService)
    {
        _gameService = gameService;
    }

    public IActionResult StartGame()
    {
        return View(new GameSettingsViewModel());
    }

    [HttpPost]
    public IActionResult StartGame(GameSettingsViewModel settings)
    {
        if (!ModelState.IsValid)
            return View(settings);

        _gameService.CreateBoard(settings);
        return RedirectToAction("MineSweeperBoard");
    }

    public IActionResult MineSweeperBoard()
    {
        var board = _gameService.GetBoard();
        if (board == null)
            return RedirectToAction("StartGame");

        return View(board);
    }

    [HttpPost]
    public IActionResult RevealCell(int row, int col)
    {
        var board = _gameService.GetBoard();
        if (board == null)
            return RedirectToAction("StartGame");

        _gameService.RevealCell(board, row, col);
        return RedirectToAction("MineSweeperBoard");
    }

    public IActionResult NewGame()
    {
        _gameService.ClearBoard();
        return RedirectToAction("StartGame");
    }

    public IActionResult Exit()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Menu", "Home");
    }
}
