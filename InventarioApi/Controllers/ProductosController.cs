using InventarioApi.Data;
using InventarioApi.Dtos;
using InventarioApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventarioApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductoResponseDto>>> ObtenerTodos()
    {
        var productos = await _context.Productos
            .AsNoTracking()
            .Select(p => new ProductoResponseDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Precio = p.Precio,
                Stock = p.Stock
            })
            .ToListAsync();

        return Ok(productos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductoResponseDto>> ObtenerPorId(int id)
    {
        var producto = await _context.Productos.FindAsync(id);

        if (producto is null)
            return NotFound();

        return Ok(ADto(producto));
    }

    [HttpPost]
    public async Task<ActionResult<ProductoResponseDto>> Crear(ProductoCreateDto dto)
    {
        var producto = new Producto
        {
            Nombre = dto.Nombre,
            Precio = dto.Precio,
            Stock = dto.Stock
        };

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerPorId), new { id = producto.Id }, ADto(producto));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, ProductoUpdateDto dto)
    {
        var producto = await _context.Productos.FindAsync(id);

        if (producto is null)
            return NotFound();

        producto.Nombre = dto.Nombre;
        producto.Precio = dto.Precio;
        producto.Stock = dto.Stock;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var producto = await _context.Productos.FindAsync(id);

        if (producto is null)
            return NotFound();

        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static ProductoResponseDto ADto(Producto p) => new()
    {
        Id = p.Id,
        Nombre = p.Nombre,
        Precio = p.Precio,
        Stock = p.Stock
    };
}