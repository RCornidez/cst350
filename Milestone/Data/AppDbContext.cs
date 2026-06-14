using Microsoft.EntityFrameworkCore;
using Milestone.Models;

namespace Milestone.Data {
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UserModel> Users { get; set; }
    }
}
