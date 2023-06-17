using Microsoft.EntityFrameworkCore;

namespace WebApplication6.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Board> Boards { get; set; }
        public DbSet<BoardFile> BoardFiles { get; set; }
    }
}
