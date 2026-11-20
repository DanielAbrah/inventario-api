using System.Security.Claims;
using InventarioApi.Dtos;
using InventarioApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventarioApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [EndpointSummary("Registra una cuenta con rol de usuario y devuelve un token JWT.")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponseDto>> Registrar(
        RegisterDto dto,
        CancellationToken cancellationToken)
    {
        var respuesta = await authService.RegistrarAsync(dto, cancellationToken);
        if (respuesta is null)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "El correo ya está registrado."
            });
        }

        return CreatedAtAction(nameof(ObtenerPerfil), null, respuesta);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EndpointSummary("Inicia sesión y devuelve un token JWT.")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> IniciarSesion(
        LoginDto dto,
        CancellationToken cancellationToken)
    {
        var respuesta = await authService.IniciarSesionAsync(dto, cancellationToken);
        if (respuesta is null)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Credenciales inválidas."
            });
        }

        return Ok(respuesta);
    }

    [HttpGet("me")]
    [Authorize]
    [EndpointSummary("Devuelve el perfil de la cuenta autenticada.")]
    [ProducesResponseType(typeof(UsuarioResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<UsuarioResponseDto> ObtenerPerfil(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Ok(new UsuarioResponseDto(
            User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            User.FindFirstValue(ClaimTypes.Role) ?? string.Empty));
    }
}
