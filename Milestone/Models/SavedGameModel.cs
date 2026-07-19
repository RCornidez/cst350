namespace Milestone.Models {
    public class SavedGameModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime DateSaved { get; set; }
        public string GameData { get; set; } = "";
    }
}
