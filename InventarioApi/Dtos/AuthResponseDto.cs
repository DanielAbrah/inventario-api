namespace InventarioApi.Dtos;

/// <summary>Token y datos básicos devueltos al autenticar una cuenta.</summary>
public sealed record AuthResponseDto(string Token, DateTimeOffset ExpiresAt);
