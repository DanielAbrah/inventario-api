namespace InventarioApi.Dtos;

/// <summary>Resultados de una página de elementos.</summary>
public sealed record PagedResultDto<T>(
    IReadOnlyList<T> Items,
    int Total,
    int Pagina,
    int Tamano);
