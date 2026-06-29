using System.ComponentModel.DataAnnotations;

namespace Milestone.Models;

public enum BoardSize { Small, Medium, Large }
public enum Difficulty { Easy, Medium, Hard }

public class GameSettingsViewModel
{
    [Required]
    public BoardSize Size { get; set; } = BoardSize.Medium;

    [Required]
    public Difficulty Difficulty { get; set; } = Difficulty.Medium;
}
