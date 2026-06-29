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
                    board.EndTime = DateTime.UtcNow;
                }
                else
                {
                    if (cell.AdjacentMines == 0)
                        board.FloodFill(row, col);

                    if (board.CheckWin())
                    {
                        board.Status = GameStatus.Won;
                        board.EndTime = DateTime.UtcNow;
                    }
                }
            }

            Session.SetString(BoardSessionKey, JsonSerializer.Serialize(board));
        }

        public void ClearBoard()
        {
            Session.Remove(BoardSessionKey);
        }

        public GameResultViewModel CalculateScore(BoardViewModel board)
        {
            var end = board.EndTime ?? DateTime.UtcNow;
            var elapsed = end - board.StartTime;
            int elapsedSeconds = (int)elapsed.TotalSeconds;

            int deduction = (elapsedSeconds / 20) * 5;
            int baseScore = Math.Max(0, 500 - deduction);

            int sizeMultiplier = board.Rows switch
            {
                5  => 2,
                20 => 4,
                _  => 3
            };

            int diffMultiplier = board.Difficulty switch
            {
                Difficulty.Easy => 2,
                Difficulty.Hard => 4,
                _               => 3
            };

            return new GameResultViewModel
            {
                Elapsed = elapsed,
                Score = baseScore * sizeMultiplier * diffMultiplier
            };
        }
    }
}
