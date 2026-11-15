using InventarioApi.Data;
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
    public async Task<ActionResult<IEnumerable<Producto>>> ObtenerTodos()
    {
        return Ok(await _context.Productos.ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Producto>> ObtenerPorId(int id)
    {
        var producto = await _context.Productos.FindAsync(id);

        if (producto is null)
            return NotFound();

        return Ok(producto);
    }

    [HttpPost]
    public async Task<ActionResult<Producto>> Crear(Producto nuevo)
    {
        _context.Productos.Add(nuevo);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevo.Id }, nuevo);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, Producto actualizado)
    {
        var producto = await _context.Productos.FindAsync(id);

        if (producto is null)
            return NotFound();

        producto.Nombre = actualizado.Nombre;
        producto.Precio = actualizado.Precio;
        producto.Stock = actualizado.Stock;

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
}