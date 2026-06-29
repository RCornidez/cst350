namespace Milestone.Models;

public enum GameStatus { InProgress, Won, Lost }

public class BoardViewModel
{
    public Cell[][] Grid { get; set; } = [];
    public int Rows { get; set; }
    public int Cols { get; set; }
    public int TotalMines { get; set; }
    public GameStatus Status { get; set; } = GameStatus.InProgress;

    public static BoardViewModel Create(GameSettingsViewModel settings)
    {
        (int rows, int cols) = settings.Size switch
        {
            BoardSize.Small => (5, 5),
            BoardSize.Large => (20, 20),
            _               => (10, 10)
        };

        double mineRate = settings.Difficulty switch
        {
            Difficulty.Easy => 0.10,
            Difficulty.Hard => 0.30,
            _               => 0.20
        };

        int totalMines = (int)Math.Round(rows * cols * mineRate);

        // Build empty grid
        Cell[][] grid = Enumerable.Range(0, rows)
            .Select(row => Enumerable.Range(0, cols)
                .Select(col => new Cell { Row = row, Col = col })
                .ToArray())
            .ToArray();

        // Shuffle all cell positions and mark the first totalMines as mines
        var rng = new Random();
        var minePositions = Enumerable.Range(0, rows * cols)
            .OrderBy(_ => rng.Next())
            .Take(totalMines);

        foreach (int pos in minePositions)
            grid[pos / cols][pos % cols].IsMine = true;

        // For each non-mine cell, count how many of its neighbors are mines
        var nonMineCells = grid.SelectMany(row => row).Where(cell => !cell.IsMine);

        foreach (var cell in nonMineCells)
            cell.AdjacentMines = GetNeighbors(grid, rows, cols, cell.Row, cell.Col).Count(c => c.IsMine);

        return new BoardViewModel { Grid = grid, Rows = rows, Cols = cols, TotalMines = totalMines };
    }

    // flood fill - reveals all connected empty cells from the starting position
    public void FloodFill(int startRow, int startCol)
    {
        var queue = new Queue<Cell>();
        queue.Enqueue(Grid[startRow][startCol]);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var neighbor in GetNeighbors(Grid, Rows, Cols, current.Row, current.Col))
            {
                if (neighbor.IsRevealed || neighbor.IsFlagged || neighbor.IsMine)
                    continue;

                neighbor.IsRevealed = true;

                if (neighbor.AdjacentMines == 0)
                    queue.Enqueue(neighbor);
            }
        }
    }

    public bool CheckWin()
    {
        return Grid.SelectMany(row => row).All(cell => cell.IsMine || cell.IsRevealed);
    }

    private static IEnumerable<Cell> GetNeighbors(Cell[][] grid, int rows, int cols, int row, int col)
    {
        int[] offsets = [-1, 0, 1];

        return offsets
            .SelectMany(dr => offsets.Select(dc => (r: row + dr, c: col + dc)))
            .Where(n => n.r >= 0 && n.r < rows && n.c >= 0 && n.c < cols)
            .Select(n => grid[n.r][n.c]);
    }
}
