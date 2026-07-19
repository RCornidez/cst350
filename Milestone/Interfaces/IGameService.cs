using Milestone.Models;

namespace Milestone.Interfaces {
    public interface IGameService
    {
        BoardViewModel? GetBoard();
        BoardViewModel CreateBoard(GameSettingsViewModel settings);
        void SetBoard(BoardViewModel board);
        List<Cell> RevealCell(BoardViewModel board, int row, int col);
        List<Cell> ToggleFlag(BoardViewModel board, int row, int col);
        void ClearBoard();
        GameResultViewModel CalculateScore(BoardViewModel board);
        SavedGameModel SaveGame(int userId, BoardViewModel board);
        List<SavedGameModel> GetGamesForUser(int userId);
        SavedGameModel? GetGameForUser(int id, int userId);
        List<SavedGameRowViewModel> GetSavedGameRows(int userId);
        bool LoadGame(int id, int userId);
        bool DeleteGameForUser(int id, int userId);
    }
}
