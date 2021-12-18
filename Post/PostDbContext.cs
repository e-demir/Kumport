using Microsoft.EntityFrameworkCore;

namespace KumportAPI.Post
{
    public class PostDbContext : DbContext
    {
        public PostDbContext(DbContextOptions<PostDbContext> options) : base(options)
        {
        }
        public DbSet<PostModel> Posts { get; set; }
    }
}
