using InventarioApi.Data;
using InventarioApi.Dtos;
using InventarioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarioApi.Services;

public sealed class AuthService(AppDbContext context, ITokenService tokenService) : IAuthService
{
    public async Task<AuthResponseDto?> RegistrarAsync(RegisterDto dto, CancellationToken cancellationToken)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        if (await context.Usuarios.AnyAsync(usuario => usuario.Email == email, cancellationToken))
            return null;

        var usuario = new Usuario
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Rol = "Usuario",
            CreadoEn = DateTimeOffset.UtcNow
        };

        context.Usuarios.Add(usuario);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            if (await context.Usuarios.AnyAsync(cuenta => cuenta.Email == email, cancellationToken))
                return null;

            throw;
        }

        return tokenService.CrearToken(usuario);
    }

    public async Task<AuthResponseDto?> IniciarSesionAsync(LoginDto dto, CancellationToken cancellationToken)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var usuario = await context.Usuarios
            .AsNoTracking()
            .SingleOrDefaultAsync(cuenta => cuenta.Email == email, cancellationToken);

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
            return null;

        return tokenService.CrearToken(usuario);
    }
}
