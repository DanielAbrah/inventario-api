using InventarioApi.Data;
using InventarioApi.Dtos;
using InventarioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarioApi.Services;

public sealed class ProductoService(AppDbContext context) : IProductoService
{
    public async Task<PagedResultDto<ProductoResponseDto>> ObtenerAsync(
        ProductoQueryDto consulta,
        CancellationToken cancellationToken)
    {
        if (consulta.PrecioMin.HasValue && consulta.PrecioMax.HasValue &&
            consulta.PrecioMin.Value > consulta.PrecioMax.Value)
        {
            throw new ArgumentException("PrecioMin no puede ser mayor que PrecioMax.");
        }

        IQueryable<Producto> productos = context.Productos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(consulta.Buscar))
        {
            var buscar = consulta.Buscar.Trim();
            var patron = $"%{buscar.ToLowerInvariant()}%";
            productos = productos.Where(producto => EF.Functions.Like(producto.Nombre.ToLower(), patron));
        }

        if (consulta.PrecioMin.HasValue)
            productos = productos.Where(producto => producto.Precio >= consulta.PrecioMin.Value);

        if (consulta.PrecioMax.HasValue)
            productos = productos.Where(producto => producto.Precio <= consulta.PrecioMax.Value);

        var total = await productos.CountAsync(cancellationToken);
        var offset = (long)(consulta.Pagina - 1) * consulta.Tamano;
        if (offset >= total)
        {
            return new PagedResultDto<ProductoResponseDto>(
                Array.Empty<ProductoResponseDto>(), total, consulta.Pagina, consulta.Tamano);
        }

        var descendente = consulta.Direccion.Equals("desc", StringComparison.OrdinalIgnoreCase);
        productos = (consulta.OrdenarPor.ToLowerInvariant(), descendente) switch
        {
            ("precio", false) => productos.OrderBy(producto => producto.Precio).ThenBy(producto => producto.Id),
            ("precio", true) => productos.OrderByDescending(producto => producto.Precio).ThenBy(producto => producto.Id),
            ("stock", false) => productos.OrderBy(producto => producto.Stock).ThenBy(producto => producto.Id),
            ("stock", true) => productos.OrderByDescending(producto => producto.Stock).ThenBy(producto => producto.Id),
            (_, false) => productos.OrderBy(producto => producto.Nombre).ThenBy(producto => producto.Id),
            _ => productos.OrderByDescending(producto => producto.Nombre).ThenBy(producto => producto.Id)
        };

        var items = await productos
            .Skip((int)offset)
            .Take(consulta.Tamano)
            .Select(producto => new ProductoResponseDto(
                producto.Id, producto.Nombre, producto.Precio, producto.Stock))
            .ToListAsync(cancellationToken);

        return new PagedResultDto<ProductoResponseDto>(items, total, consulta.Pagina, consulta.Tamano);
    }

    public async Task<ProductoResponseDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken)
    {
        return await context.Productos
            .AsNoTracking()
            .Where(producto => producto.Id == id)
            .Select(producto => new ProductoResponseDto(
                producto.Id, producto.Nombre, producto.Precio, producto.Stock))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ProductoResponseDto> CrearAsync(ProductoCreateDto dto, CancellationToken cancellationToken)
    {
        var producto = new Producto
        {
            Nombre = dto.Nombre.Trim(),
            Precio = dto.Precio,
            Stock = dto.Stock
        };

        context.Productos.Add(producto);
        await context.SaveChangesAsync(cancellationToken);
        return ADto(producto);
    }

    public async Task<ProductoResponseDto?> ActualizarAsync(
        int id,
        ProductoUpdateDto dto,
        CancellationToken cancellationToken)
    {
        var producto = await context.Productos.FindAsync([id], cancellationToken);
        if (producto is null)
            return null;

        producto.Nombre = dto.Nombre.Trim();
        producto.Precio = dto.Precio;
        producto.Stock = dto.Stock;
        await context.SaveChangesAsync(cancellationToken);
        return ADto(producto);
    }

    public async Task<bool> EliminarAsync(int id, CancellationToken cancellationToken)
    {
        var producto = await context.Productos.FindAsync([id], cancellationToken);
        if (producto is null)
            return false;

        context.Productos.Remove(producto);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static ProductoResponseDto ADto(Producto producto) =>
        new(producto.Id, producto.Nombre, producto.Precio, producto.Stock);
}
