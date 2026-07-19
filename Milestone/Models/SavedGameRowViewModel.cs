namespace Milestone.Models {
    public class SavedGameRowViewModel
    {
        public int Id { get; set; }
        public DateTime DateSaved { get; set; }
        public int Rows { get; set; }
        public int Cols { get; set; }
        public Difficulty Difficulty { get; set; }
        public GameStatus Status { get; set; }
    }
}
