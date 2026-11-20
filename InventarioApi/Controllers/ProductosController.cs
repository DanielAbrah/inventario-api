using InventarioApi.Dtos;
using InventarioApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventarioApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductosController(IProductoService productoService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [EndpointSummary("Lista productos con filtros y paginación.")]
    [EndpointDescription("Permite buscar por nombre, filtrar por precio, ordenar y solicitar páginas de hasta 50 productos.")]
    [ProducesResponseType(typeof(PagedResultDto<ProductoResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<ProductoResponseDto>>> ObtenerTodos(
        [FromQuery] ProductoQueryDto consulta,
        CancellationToken cancellationToken)
    {
        return Ok(await productoService.ObtenerAsync(consulta, cancellationToken));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [EndpointSummary("Obtiene un producto por su identificador.")]
    [ProducesResponseType(typeof(ProductoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductoResponseDto>> ObtenerPorId(int id, CancellationToken cancellationToken)
    {
        var producto = await productoService.ObtenerPorIdAsync(id, cancellationToken);
        return producto is null ? NotFound() : Ok(producto);
    }

    [HttpPost]
    [Authorize]
    [EndpointSummary("Crea un producto.")]
    [ProducesResponseType(typeof(ProductoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProductoResponseDto>> Crear(
        ProductoCreateDto dto,
        CancellationToken cancellationToken)
    {
        var producto = await productoService.CrearAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = producto.Id }, producto);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    [EndpointSummary("Actualiza todos los datos de un producto.")]
    [ProducesResponseType(typeof(ProductoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductoResponseDto>> Actualizar(
        int id,
        ProductoUpdateDto dto,
        CancellationToken cancellationToken)
    {
        var producto = await productoService.ActualizarAsync(id, dto, cancellationToken);
        return producto is null ? NotFound() : Ok(producto);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Elimina un producto (solo Admin).")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        var eliminado = await productoService.EliminarAsync(id, cancellationToken);
        return eliminado ? NoContent() : NotFound();
    }
}
