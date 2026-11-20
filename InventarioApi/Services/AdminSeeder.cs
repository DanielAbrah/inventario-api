using InventarioApi.Data;
using InventarioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarioApi.Services;

public sealed class AdminSeeder(AppDbContext context, IConfiguration configuration) : IAdminSeeder
{
    public async Task CrearAdminInicialAsync(CancellationToken cancellationToken)
    {
        var email = configuration["Admin:Email"]?.Trim().ToLowerInvariant();
        var password = configuration["Admin:Password"];
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(password))
            return;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || password.Length < 12)
            throw new InvalidOperationException(
                "Admin:Email y Admin:Password son obligatorios; la contraseña debe tener al menos 12 caracteres.");

        var existe = await context.Usuarios.AnyAsync(usuario => usuario.Email == email, cancellationToken);
        if (existe)
            return;

        context.Usuarios.Add(new Usuario
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Rol = "Admin",
            CreadoEn = DateTimeOffset.UtcNow
        });
        await context.SaveChangesAsync(cancellationToken);
    }
}
