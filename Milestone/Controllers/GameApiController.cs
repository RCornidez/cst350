using Microsoft.AspNetCore.Mvc;
using Milestone.Interfaces;
using Milestone.Models;
using System.Text.Json;

namespace Milestone.Controllers;

[ApiController]
[Route("api")]
public class GameApiController : ControllerBase
{
    private readonly IGameService _gameService;

    public GameApiController(IGameService gameService)
    {
        _gameService = gameService;
    }

    private int UserId => HttpContext.Session.GetInt32("UserId")!.Value;

    [HttpGet("showSavedGames")]
    public IActionResult ShowSavedGames()
    {
        var games = _gameService.GetGamesForUser(UserId)
            .Select(game => new { game.Id, game.UserId, game.DateSaved });

        return Ok(games);
    }

    [HttpGet("showSavedGames/{id:int}")]
    public IActionResult ShowSavedGame(int id)
    {
        var game = _gameService.GetGameForUser(id, UserId);
        if (game == null)
            return NotFound();

        return Ok(new
        {
            game.Id,
            game.UserId,
            game.DateSaved,
            GameData = JsonSerializer.Deserialize<BoardViewModel>(game.GameData)
        });
    }

    [HttpDelete("deleteOneGame/{id:int}")]
    public IActionResult DeleteOneGame(int id)
    {
        if (!_gameService.DeleteGameForUser(id, UserId))
            return NotFound();

        return Ok(new { deleted = id });
    }
}
