using Microsoft.EntityFrameworkCore;
using UserExe.Entities;

namespace UserExe.Data;

public class MyAppDbContext(DbContextOptions<MyAppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
}