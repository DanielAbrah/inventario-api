namespace InventarioApi.Dtos;

/// <summary>Datos públicos de la cuenta autenticada.</summary>
public sealed record UsuarioResponseDto(string Email, string Rol);
