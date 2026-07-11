using Milestone.Models;

namespace Milestone.Interfaces {
    public interface IGameService
    {
        BoardViewModel? GetBoard();
        BoardViewModel CreateBoard(GameSettingsViewModel settings);
        List<Cell> RevealCell(BoardViewModel board, int row, int col);
        List<Cell> ToggleFlag(BoardViewModel board, int row, int col);
        void ClearBoard();
        GameResultViewModel CalculateScore(BoardViewModel board);
    }
}
