using ApiScann.DTOs;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ScanLog> ScanLog { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ScanLog>()
            .Property(x => x.Id)
            .HasDefaultValueSql("NEWID()");
    }
}
