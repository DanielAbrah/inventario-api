using System.ComponentModel.DataAnnotations;

namespace InventarioApi.Dtos;

/// <summary>Datos para registrar una cuenta de usuario.</summary>
public sealed record RegisterDto
{
    [Required, EmailAddress, StringLength(256)]
    public required string Email { get; init; }

    [Required, MinLength(8), StringLength(100)]
    public required string Password { get; init; }
}
