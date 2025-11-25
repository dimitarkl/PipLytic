using Microsoft.EntityFrameworkCore;
using PipLytic.Api.Entities;

namespace PipLytic.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Trade> Trades { get; set; }
    public DbSet<Company> Companies { get; set; }

}