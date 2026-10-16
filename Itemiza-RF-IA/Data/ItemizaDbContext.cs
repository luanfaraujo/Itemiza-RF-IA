
using Itemiza_RF_IA.Models;
using Microsoft.EntityFrameworkCore;
using Itemiza_RF_IA.Data;

namespace Itemiza_RF_IA.Data;

public class ItemizaDbContext : DbContext
{
    public ItemizaDbContext(
        DbContextOptions<ItemizaDbContext> options)
        : base(options)
    {
    }

    public DbSet<Compra> Compras { get; set; }

    public DbSet<Produto> Produtos { get; set; }

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Compra>()
            .HasMany(c => c.Produtos)
            .WithOne(p => p.Compra)
            .HasForeignKey(p => p.CompraId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Produto>()
            .Property(p => p.ValorUnitario)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Produto>()
            .Property(p => p.Quantidade)
            .HasPrecision(18, 3);

        modelBuilder.Entity<Produto>()
            .Property(p => p.ValorTotal)
            .HasPrecision(18, 2);
    }
}
