using Microsoft.AspNetCore.Mvc;
using Milestone.Models;
using System.Text.Json;

namespace Milestone.Controllers;

public class GameController : Controller
{
    private const string BoardSessionKey = "GameBoard";

    public IActionResult StartGame()
    {
        return View(new GameSettingsViewModel());
    }

    [HttpPost]
    public IActionResult StartGame(GameSettingsViewModel settings)
    {
        if (!ModelState.IsValid)
            return View(settings);

        var board = BoardViewModel.Create(settings);
        HttpContext.Session.SetString(BoardSessionKey, JsonSerializer.Serialize(board));

        return RedirectToAction("MineSweeperBoard");
    }

    public IActionResult MineSweeperBoard()
    {
        var json = HttpContext.Session.GetString(BoardSessionKey);
        if (json == null)
            return RedirectToAction("StartGame");

        var board = JsonSerializer.Deserialize<BoardViewModel>(json);
        return View(board);
    }

    [HttpPost]
    public IActionResult RevealCell(int row, int col)
    {
        var json = HttpContext.Session.GetString(BoardSessionKey);
        if (json == null)
            return RedirectToAction("StartGame");

        var board = JsonSerializer.Deserialize<BoardViewModel>(json);
        var cell = board.Grid[row][col];

        if (!cell.IsRevealed && !cell.IsFlagged && board.Status == GameStatus.InProgress)
        {
            cell.IsRevealed = true;

            // Failure Scenario
            if (cell.IsMine)
            {
                // Reveal all mines and mark as lost
                foreach (var c in board.Grid.SelectMany(r => r).Where(c => c.IsMine))
                    c.IsRevealed = true;

                board.Status = GameStatus.Lost;
                Console.WriteLine("Player hit a mine - game over.");
            }
            // Success Scenario
            else
            {
                if (cell.AdjacentMines == 0)
                    board.FloodFill(row, col);

                if (board.CheckWin())
                {
                    board.Status = GameStatus.Won;
                    Console.WriteLine("Player cleared the board - game won.");
                }
            }
        }

        HttpContext.Session.SetString(BoardSessionKey, JsonSerializer.Serialize(board));
        return RedirectToAction("MineSweeperBoard");
    }

    public IActionResult NewGame()
    {
        HttpContext.Session.Remove(BoardSessionKey);
        return RedirectToAction("StartGame");
    }

    public IActionResult Exit()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Menu", "Home");
    }
}
