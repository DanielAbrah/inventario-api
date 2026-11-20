# Inventario API

[![CI](https://github.com/DanielAbrah/-Inventory-or-tasks-API-CRUD-authentication-validations-testing-with-xUnit-/actions/workflows/ci.yml/badge.svg)](https://github.com/DanielAbrah/-Inventory-or-tasks-API-CRUD-authentication-validations-testing-with-xUnit-/actions/workflows/ci.yml)

API REST de inventario construida con .NET 10, ASP.NET Core, EF Core y PostgreSQL. Incluye CRUD de productos, autenticación JWT, roles, filtros/paginación, documentación OpenAPI con Scalar y pruebas automatizadas.

## Requisitos

- .NET SDK 10
- Docker Desktop (para PostgreSQL o Docker Compose)

## Ejecutar localmente

Desde la raíz del repositorio:

```powershell
dotnet user-secrets init --project InventarioApi
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5433;Database=inventario;Username=postgres;Password=CAMBIA_ESTA_CLAVE" --project InventarioApi
dotnet user-secrets set "Jwt:Key" ([Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))) --project InventarioApi
dotnet user-secrets set "Admin:Email" "admin@example.com" --project InventarioApi
dotnet user-secrets set "Admin:Password" "CAMBIA_POR_UNA_CLAVE_ADMIN_SEGURA" --project InventarioApi
```

Para iniciar PostgreSQL localmente en el puerto 5433:

```powershell
docker run --name inventario-db -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=CAMBIA_ESTA_CLAVE -e POSTGRES_DB=inventario -p 5433:5432 -v inventario-data:/var/lib/postgresql/data -d postgres:17
```

La clave debe coincidir con la cadena de conexión configurada arriba. Luego aplica las migraciones y ejecuta la API:

```powershell
dotnet ef database update --project InventarioApi
dotnet run --project InventarioApi
```

La aplicación crea el usuario Admin configurado si aún no existe. El registro público siempre asigna el rol `Usuario`. No reutilices las claves de ejemplo.

En entorno Development, la especificación OpenAPI está en `/openapi/v1.json` y Scalar en `/scalar/v1`. Para crear un usuario normal, usa `POST /api/auth/register`; para el usuario Admin usa las credenciales configuradas arriba.

## Docker Compose

1. Copia `.env.example` a `.env` y reemplaza **todos** los valores de ejemplo por secretos aleatorios propios.
2. Desde la raíz:

```powershell
docker compose up --build
```

Compose levanta PostgreSQL, espera que esté disponible, aplica migraciones y crea el Admin inicial. La API queda en `http://localhost:8080`. El archivo `.env` está excluido de Git.

## Endpoints

| Método | Ruta | Acceso |
|---|---|---|
| POST | `/api/auth/register` | Público |
| POST | `/api/auth/login` | Público |
| GET | `/api/auth/me` | JWT |
| GET | `/api/productos` | Público; filtros y paginación |
| GET | `/api/productos/{id}` | Público |
| POST | `/api/productos` | JWT |
| PUT | `/api/productos/{id}` | JWT |
| DELETE | `/api/productos/{id}` | JWT con rol `Admin` |

`GET /api/productos` admite `pagina`, `tamano` (máximo 50), `buscar`, `precioMin`, `precioMax`, `ordenarPor` (`nombre`, `precio` o `stock`) y `direccion` (`asc` o `desc`). Las respuestas de error usan Problem Details.

Usa `InventarioApi/InventarioApi.http` como colección de solicitudes de ejemplo. Para borrar productos, autentícate como Admin; los usuarios registrados no tienen ese permiso.

## Pruebas y compilación

```powershell
dotnet test InventarioApi.slnx
dotnet build InventarioApi.slnx --configuration Release
```

Las pruebas de integración usan SQLite en memoria; no requieren una instancia de PostgreSQL.

## Estructura

- `InventarioApi/`: API, persistencia, migraciones y servicios.
- `InventarioApi.Tests/`: pruebas unitarias y de integración.
- `.github/workflows/ci.yml`: compilación y pruebas en GitHub Actions.
- `GUIA.md`: guía y checklist del proyecto.
- `GUIA_GITFLOW.md`: pasos para integrar las funcionalidades en ramas y PRs separados.

El despliegue público queda pendiente de elegir una plataforma y proporcionar una base de datos gestionada y secretos para ese entorno.
