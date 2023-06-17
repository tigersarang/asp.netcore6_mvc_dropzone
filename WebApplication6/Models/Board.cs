namespace WebApplication6.Models
{
    public class Board
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public List<BoardFile> BaordFiles { get; set; }
    }
}
