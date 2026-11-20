using System.ComponentModel.DataAnnotations;

namespace InventarioApi.Dtos;

/// <summary>Datos para reemplazar la información de un producto.</summary>
public sealed record ProductoUpdateDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    public required string Nombre { get; init; }

    [Range(0.01, 1000000, ErrorMessage = "El precio debe ser mayor que 0.")]
    public decimal Precio { get; init; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
    public int Stock { get; init; }
}
