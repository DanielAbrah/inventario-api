using InventarioApi.Models;
using InventarioApi.Dtos;

namespace InventarioApi.Services;

public interface ITokenService
{
    AuthResponseDto CrearToken(Usuario usuario);
}
