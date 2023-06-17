using System.Text.Json.Serialization;

namespace WebApplication6.Models
{
    public class BoardFile
    {
        public int Id { get; set; }
        public string FileName { get; set; }

        public int BoardId { get; set; }
        [JsonIgnore]
        public Board Board { get; set; }
    }
}
