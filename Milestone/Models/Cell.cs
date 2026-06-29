namespace Milestone.Models;

public class Cell
{
    public int Row { get; set; }
    public int Col { get; set; }
    public bool IsMine { get; set; }
    public bool IsRevealed { get; set; }
    public bool IsFlagged { get; set; }
    public int AdjacentMines { get; set; }
}
