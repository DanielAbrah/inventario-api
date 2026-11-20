namespace InventarioApi.Services;

public interface IAdminSeeder
{
    Task CrearAdminInicialAsync(CancellationToken cancellationToken);
}
