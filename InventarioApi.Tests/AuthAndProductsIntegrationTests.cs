using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using InventarioApi.Data;
using InventarioApi.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace InventarioApi.Tests;

public sealed class AuthAndProductsIntegrationTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task Registration_login_and_product_permissions_work()
    {
        using var client = CreateClientAndResetDatabase();
        const string email = "person@example.com";
        const string password = "StrongPassword123";

        var registration = await client.PostAsJsonAsync("/api/auth/register", new { email, password });
        Assert.Equal(HttpStatusCode.Created, registration.StatusCode);
        var registerBody = await registration.Content.ReadFromJsonAsync<JsonElement>();
        var token = registerBody.GetProperty("token").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));

        var publicList = await client.GetAsync("/api/productos");
        Assert.Equal(HttpStatusCode.OK, publicList.StatusCode);

        var unauthorized = await client.PostAsJsonAsync("/api/productos", new
        {
            nombre = "Teclado",
            precio = 49.99m,
            stock = 5
        });
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);

        var invalidPassword = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "incorrecta" });
        Assert.Equal(HttpStatusCode.Unauthorized, invalidPassword.StatusCode);

        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var created = await client.PostAsJsonAsync("/api/productos", new
        {
            nombre = "Teclado mecánico",
            precio = 79.99m,
            stock = 15
        });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(created.Headers.Location);
        var createdBody = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = createdBody.GetProperty("id").GetInt32();

        var fetched = await client.GetAsync($"/api/productos/{id}");
        Assert.Equal(HttpStatusCode.OK, fetched.StatusCode);

        var updated = await client.PutAsJsonAsync($"/api/productos/{id}", new
        {
            nombre = "Teclado actualizado",
            precio = 89.99m,
            stock = 12
        });
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        var updatedBody = await updated.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Teclado actualizado", updatedBody.GetProperty("nombre").GetString());

        var missingUpdate = await client.PutAsJsonAsync("/api/productos/999999", new
        {
            nombre = "No existe",
            precio = 10,
            stock = 1
        });
        Assert.Equal(HttpStatusCode.NotFound, missingUpdate.StatusCode);

        var invalidProduct = await client.PostAsJsonAsync("/api/productos", new
        {
            nombre = "X",
            precio = -1,
            stock = -1
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalidProduct.StatusCode);
        Assert.Contains("application/problem+json", invalidProduct.Content.Headers.ContentType?.MediaType);

        var profile = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, profile.StatusCode);

        var forbiddenDelete = await client.DeleteAsync($"/api/productos/{id}");
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenDelete.StatusCode);

        await CrearAdminAsync();
        var adminLogin = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@example.com",
            password = "AdminPassword123!"
        });
        Assert.Equal(HttpStatusCode.OK, adminLogin.StatusCode);
        var adminBody = await adminLogin.Content.ReadFromJsonAsync<JsonElement>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", adminBody.GetProperty("token").GetString());

        var deleted = await client.DeleteAsync($"/api/productos/{id}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
    }

    [Fact]
    public async Task Registration_rejects_duplicate_email_and_short_password()
    {
        using var client = CreateClientAndResetDatabase();

        var invalid = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "short@example.com",
            password = "123"
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);

        var first = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "duplicate@example.com",
            password = "StrongPassword123"
        });
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var duplicate = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "DUPLICATE@example.com",
            password = "StrongPassword123"
        });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
    }

    [Fact]
    public async Task Product_list_applies_filters_and_pagination()
    {
        using var client = CreateClientAndResetDatabase();
        var token = await RegistrarYObtenerTokenAsync(client, "catalog@example.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await client.PostAsJsonAsync("/api/productos", new { nombre = "Teclado", precio = 60m, stock = 4 });
        await client.PostAsJsonAsync("/api/productos", new { nombre = "teclado compacto", precio = 40m, stock = 7 });
        await client.PostAsJsonAsync("/api/productos", new { nombre = "Mouse", precio = 25m, stock = 10 });

        var response = await client.GetAsync(
            "/api/productos?buscar=TECLADO&precioMin=30&precioMax=70&ordenarPor=precio&direccion=desc&pagina=1&tamano=1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var page = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2, page.GetProperty("total").GetInt32());
        Assert.Equal(1, page.GetProperty("tamano").GetInt32());
        Assert.Equal("Teclado", page.GetProperty("items")[0].GetProperty("nombre").GetString());

        var invalidPageSize = await client.GetAsync("/api/productos?tamano=51");
        Assert.Equal(HttpStatusCode.BadRequest, invalidPageSize.StatusCode);

        var distantPage = await client.GetAsync("/api/productos?pagina=2147483647");
        Assert.Equal(HttpStatusCode.OK, distantPage.StatusCode);
        var distantPageBody = await distantPage.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Empty(distantPageBody.GetProperty("items").EnumerateArray());
    }

    private HttpClient CreateClientAndResetDatabase()
    {
        var client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        return client;
    }

    private async Task CrearAdminAsync()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Usuarios.Add(new Usuario
        {
            Email = "admin@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("AdminPassword123!"),
            Rol = "Admin",
            CreadoEn = DateTimeOffset.UtcNow
        });
        await context.SaveChangesAsync();
    }

    private static async Task<string> RegistrarYObtenerTokenAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "StrongPassword123"
        });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("token").GetString()!;
    }
}
