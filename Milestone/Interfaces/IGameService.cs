using Milestone.Models;

namespace Milestone.Interfaces {
    public interface IGameService
    {
        BoardViewModel? GetBoard();
        BoardViewModel CreateBoard(GameSettingsViewModel settings);
        void RevealCell(BoardViewModel board, int row, int col);
        void ClearBoard();
        GameResultViewModel CalculateScore(BoardViewModel board);
    }
}
