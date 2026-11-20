using InventarioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarioApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Producto>(entity =>
        {
            entity.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Precio).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Rol).IsRequired().HasMaxLength(20);
        });
    }
}