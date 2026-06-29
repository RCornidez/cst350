using Milestone.Interfaces;
using Milestone.Models;
using System.Text.Json;

namespace Milestone.Services {
    public class GameService : IGameService
    {
        private const string BoardSessionKey = "GameBoard";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession Session => _httpContextAccessor.HttpContext!.Session;

        public GameService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public BoardViewModel? GetBoard()
        {
            var json = Session.GetString(BoardSessionKey);
            return json == null ? null : JsonSerializer.Deserialize<BoardViewModel>(json);
        }

        public BoardViewModel CreateBoard(GameSettingsViewModel settings)
        {
            var board = BoardViewModel.Create(settings);
            Session.SetString(BoardSessionKey, JsonSerializer.Serialize(board));
            return board;
        }

        public void RevealCell(BoardViewModel board, int row, int col)
        {
            var cell = board.Grid[row][col];

            if (!cell.IsRevealed && !cell.IsFlagged && board.Status == GameStatus.InProgress)
            {
                cell.IsRevealed = true;

                if (cell.IsMine)
                {
                    foreach (var c in board.Grid.SelectMany(r => r).Where(c => c.IsMine))
                        c.IsRevealed = true;

                    board.Status = GameStatus.Lost;
                    Console.WriteLine("Player hit a mine - game over.");
                }
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

            Session.SetString(BoardSessionKey, JsonSerializer.Serialize(board));
        }

        public void ClearBoard()
        {
            Session.Remove(BoardSessionKey);
        }
    }
}
