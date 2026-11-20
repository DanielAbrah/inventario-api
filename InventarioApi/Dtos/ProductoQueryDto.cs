using System.ComponentModel.DataAnnotations;

namespace InventarioApi.Dtos;

/// <summary>Filtros, orden y paginación para consultar productos.</summary>
public sealed record ProductoQueryDto
{
    [Range(1, int.MaxValue)]
    public int Pagina { get; init; } = 1;

    [Range(1, 50)]
    public int Tamano { get; init; } = 10;

    [StringLength(100)]
    public string? Buscar { get; init; }

    [Range(0, 1000000)]
    public decimal? PrecioMin { get; init; }

    [Range(0, 1000000)]
    public decimal? PrecioMax { get; init; }

    [RegularExpression("^(nombre|precio|stock)$", ErrorMessage = "OrdenarPor debe ser nombre, precio o stock.")]
    public string OrdenarPor { get; init; } = "nombre";

    [RegularExpression("^(asc|desc)$", ErrorMessage = "Direccion debe ser asc o desc.")]
    public string Direccion { get; init; } = "asc";
}
