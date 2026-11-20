using System.ComponentModel.DataAnnotations;

namespace InventarioApi.Dtos;

/// <summary>Credenciales para iniciar sesión.</summary>
public sealed record LoginDto
{
    [Required, EmailAddress, StringLength(256)]
    public required string Email { get; init; }

    [Required, StringLength(100)]
    public required string Password { get; init; }
}
