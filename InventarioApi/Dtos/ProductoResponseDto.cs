namespace InventarioApi.Dtos;

/// <summary>Producto devuelto por la API.</summary>
public sealed record ProductoResponseDto(int Id, string Nombre, decimal Precio, int Stock);
