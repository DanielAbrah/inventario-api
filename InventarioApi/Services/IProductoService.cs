using InventarioApi.Dtos;

namespace InventarioApi.Services;

public interface IProductoService
{
    Task<PagedResultDto<ProductoResponseDto>> ObtenerAsync(ProductoQueryDto consulta, CancellationToken cancellationToken);
    Task<ProductoResponseDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken);
    Task<ProductoResponseDto> CrearAsync(ProductoCreateDto dto, CancellationToken cancellationToken);
    Task<ProductoResponseDto?> ActualizarAsync(int id, ProductoUpdateDto dto, CancellationToken cancellationToken);
    Task<bool> EliminarAsync(int id, CancellationToken cancellationToken);
}
