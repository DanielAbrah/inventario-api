namespace InventarioApi.Models;

public sealed class Usuario
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Rol { get; set; } = "Usuario";
    public DateTimeOffset CreadoEn { get; set; }
}
