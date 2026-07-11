using Microsoft.AspNetCore.Mvc;
using Milestone.Extensions;
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
    public async Task<IActionResult> RevealCell(int row, int col)
    {
        var board = _gameService.GetBoard();
        if (board == null)
            return NotFound();

        var changed = _gameService.RevealCell(board, row, col);

        return await CellUpdateResult(changed, board.Status);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleFlag(int row, int col)
    {
        var board = _gameService.GetBoard();
        if (board == null)
            return NotFound();

        var changed = _gameService.ToggleFlag(board, row, col);

        return await CellUpdateResult(changed, board.Status);
    }

    private async Task<IActionResult> CellUpdateResult(List<Cell> changed, GameStatus status)
    {
        var cells = new List<object>();
        foreach (var cell in changed)
        {
            cells.Add(new
            {
                row = cell.Row,
                col = cell.Col,
                html = await this.RenderPartialViewToStringAsync("_CellPartial", cell)
            });
        }

        return Json(new
        {
            status = status.ToString(),
            timestamp = DateTime.Now.ToString("HH:mm:ss"),
            cells
        });
    }

    public IActionResult GameWon()
    {
        var board = _gameService.GetBoard();
        if (board == null)
            return RedirectToAction("StartGame");

        return View(_gameService.CalculateScore(board));
    }

    public IActionResult GameLost()
    {
        var board = _gameService.GetBoard();
        if (board == null)
            return RedirectToAction("StartGame");

        var result = _gameService.CalculateScore(board);
        result.Score = 0;
        return View(result);
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
