using InventarioApi.Dtos;

namespace InventarioApi.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> RegistrarAsync(RegisterDto dto, CancellationToken cancellationToken);
    Task<AuthResponseDto?> IniciarSesionAsync(LoginDto dto, CancellationToken cancellationToken);
}
