using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using InventarioApi.Models;
using InventarioApi.Services;
using Microsoft.Extensions.Configuration;

namespace InventarioApi.Tests;

public sealed class TokenServiceTests
{
    [Fact]
    public void Created_token_contains_user_identity_role_and_expiration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "unit-test-key-that-is-at-least-32-bytes-long",
                ["Jwt:Issuer"] = "InventarioApi.Tests",
                ["Jwt:Audience"] = "InventarioApi.Tests"
            })
            .Build();
        var service = new TokenService(configuration);

        var response = service.CrearToken(new Usuario
        {
            Id = 42,
            Email = "admin@example.com",
            Rol = "Admin"
        });

        var token = new JwtSecurityTokenHandler().ReadJwtToken(response.Token);
        Assert.Equal("42", token.Claims.Single(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal("admin@example.com", token.Claims.Single(claim => claim.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal("Admin", token.Claims.Single(claim => claim.Type == ClaimTypes.Role).Value);
        Assert.True(response.ExpiresAt > DateTimeOffset.UtcNow);
    }
}
