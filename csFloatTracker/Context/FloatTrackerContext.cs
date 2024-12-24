using csFloatTracker.Model;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace csFloatTracker.Context;

public class FloatTrackerContext : DbContext
{
    public DbSet<CsAccount> CsAccounts { get; set; }
    public DbSet<InventoryItem> Inventory { get; set; }
    public DbSet<TransactionItem> TransactionHistory { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = Path.Combine(AppContext.BaseDirectory, "data.db");
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CsAccount>()
            .HasMany(c => c.Inventory)
            .WithOne(i => i.CsAccount)
            .HasForeignKey(i => i.CsAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CsAccount>()
            .HasMany(c => c.TransactionHistory)
            .WithOne(t => t.CsAccount)
            .HasForeignKey(t => t.CsAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CsAccount>().HasData(new CsAccount
        {
            Id = 1,
            SoldCount = 0,
            PurchasedCount = 0,
            Balance = 0,
            Profit = 0
        });

        base.OnModelCreating(modelBuilder);
    }
}
