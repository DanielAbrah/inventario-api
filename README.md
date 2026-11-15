# Inventory-or-tasks-API-CRUD-authentication-validations-testing-with-xUnit

docker run --name inventario-db -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres123 -e POSTGRES_DB=inventario -p 5433:5432 -v inventario-data:/var/lib/postgresql/data -d postgres:17
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5433;Database=inventario;Username=postgres;Password=postgres123"
dotnet ef database update