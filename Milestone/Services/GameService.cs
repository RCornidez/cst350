using Milestone.Data;
using Milestone.Interfaces;
using Milestone.Models;
using System.Text.Json;

namespace Milestone.Services {
    public class GameService : IGameService
    {
        private const string BoardSessionKey = "GameBoard";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _db;
        private ISession Session => _httpContextAccessor.HttpContext!.Session;

        public GameService(IHttpContextAccessor httpContextAccessor, AppDbContext db)
        {
            _httpContextAccessor = httpContextAccessor;
            _db = db;
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

        public void SetBoard(BoardViewModel board)
        {
            Session.SetString(BoardSessionKey, JsonSerializer.Serialize(board));
        }

        public List<Cell> RevealCell(BoardViewModel board, int row, int col)
        {
            var changed = new List<Cell>();
            var cell = board.Grid[row][col];

            if (!cell.IsRevealed && !cell.IsFlagged && board.Status == GameStatus.InProgress)
            {
                cell.IsRevealed = true;
                changed.Add(cell);

                if (cell.IsMine)
                {
                    foreach (var c in board.Grid.SelectMany(r => r).Where(c => c.IsMine && !c.IsRevealed))
                    {
                        c.IsRevealed = true;
                        changed.Add(c);
                    }

                    board.Status = GameStatus.Lost;
                    board.EndTime = DateTime.UtcNow;
                }
                else
                {
                    if (cell.AdjacentMines == 0)
                        changed.AddRange(board.FloodFill(row, col));

                    if (board.CheckWin())
                    {
                        board.Status = GameStatus.Won;
                        board.EndTime = DateTime.UtcNow;
                    }
                }
            }

            Session.SetString(BoardSessionKey, JsonSerializer.Serialize(board));
            return changed;
        }

        public List<Cell> ToggleFlag(BoardViewModel board, int row, int col)
        {
            var changed = new List<Cell>();
            var cell = board.Grid[row][col];

            if (!cell.IsRevealed && board.Status == GameStatus.InProgress)
            {
                cell.IsFlagged = !cell.IsFlagged;
                changed.Add(cell);
                Session.SetString(BoardSessionKey, JsonSerializer.Serialize(board));
            }

            return changed;
        }

        public void ClearBoard()
        {
            Session.Remove(BoardSessionKey);
        }

        public SavedGameModel SaveGame(int userId, BoardViewModel board)
        {
            var game = new SavedGameModel
            {
                UserId = userId,
                DateSaved = DateTime.UtcNow,
                GameData = JsonSerializer.Serialize(board)
            };

            _db.Games.Add(game);
            _db.SaveChanges();
            return game;
        }

        public List<SavedGameModel> GetGamesForUser(int userId) =>
            _db.Games
                .Where(g => g.UserId == userId)
                .OrderByDescending(g => g.DateSaved)
                .ToList();

        public List<SavedGameRowViewModel> GetSavedGameRows(int userId) =>
            GetGamesForUser(userId)
                .Select(game =>
                {
                    var board = JsonSerializer.Deserialize<BoardViewModel>(game.GameData)!;
                    return new SavedGameRowViewModel
                    {
                        Id = game.Id,
                        DateSaved = game.DateSaved,
                        Rows = board.Rows,
                        Cols = board.Cols,
                        Difficulty = board.Difficulty,
                        Status = board.Status
                    };
                })
                .ToList();

        public SavedGameModel? GetGameForUser(int id, int userId)
        {
            var game = _db.Games.Find(id);
            return game?.UserId == userId ? game : null;
        }

        public bool LoadGame(int id, int userId)
        {
            var game = GetGameForUser(id, userId);
            if (game == null)
                return false;

            var board = JsonSerializer.Deserialize<BoardViewModel>(game.GameData);
            if (board == null)
                return false;

            SetBoard(board);
            return true;
        }

        public bool DeleteGameForUser(int id, int userId)
        {
            var game = GetGameForUser(id, userId);
            if (game == null)
                return false;

            _db.Games.Remove(game);
            _db.SaveChanges();
            return true;
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
