using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InventarioApi.Dtos;
using InventarioApi.Models;
using Microsoft.IdentityModel.Tokens;

namespace InventarioApi.Services;

public sealed class TokenService(IConfiguration configuration) : ITokenService
{
    public AuthResponseDto CrearToken(Usuario usuario)
    {
        var key = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Falta configurar Jwt:Key.");
        var issuer = configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Falta configurar Jwt:Issuer.");
        var audience = configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("Falta configurar Jwt:Audience.");
        var keyBytes = Encoding.UTF8.GetBytes(key);

        if (keyBytes.Length < 32)
            throw new InvalidOperationException("Jwt:Key debe contener al menos 32 bytes.");

        var expiration = DateTimeOffset.UtcNow.AddMinutes(60);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Rol)
        };
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(keyBytes),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expiration.UtcDateTime,
            signingCredentials: credentials);

        return new AuthResponseDto(new JwtSecurityTokenHandler().WriteToken(token), expiration);
    }
}
