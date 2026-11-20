# InventarioApi: Guía del proyecto

API REST de inventario de productos con **.NET 10, ASP.NET Core, Entity Framework Core, PostgreSQL y JWT**. Proyecto de portafolio para conseguir un primer empleo como desarrollador .NET.

---

## Índice

1. [Estado actual](#1-estado-actual)
2. [Entorno y herramientas](#2-entorno-y-herramientas)
3. [Lo que ya está hecho](#3-lo-que-ya-está-hecho)
4. [Flujo de trabajo con Git](#4-flujo-de-trabajo-con-git)
5. [Problemas resueltos y lecciones aprendidas](#5-problemas-resueltos-y-lecciones-aprendidas)
6. [Lo que falta por hacer (con cómo)](#6-lo-que-falta-por-hacer-con-cómo)
7. [Orden sugerido y tiempos](#7-orden-sugerido-y-tiempos)
8. [Referencia rápida de comandos](#8-referencia-rápida-de-comandos)
9. [Checklist final del portafolio](#9-checklist-final-del-portafolio)

---

## 1. Estado actual

| Etapa | Estado |
|---|---|
| Proyecto base y primer endpoint | Hecho |
| CRUD completo (GET, POST, PUT, DELETE) | Hecho |
| Entity Framework Core + PostgreSQL (Docker) | Hecho |
| Migraciones | Hecho |
| DTOs y validaciones | Hecho y probado |
| Autenticación JWT | Hecho y probado |
| Roles y autorización | Hecho y probado |
| OpenAPI/Scalar, errores globales, paginación y filtros | Hecho |
| Capa de servicios | Hecho |
| Pruebas xUnit | Hecho |
| Docker + docker-compose | Configurado |
| GitHub Actions (CI) | Configurado; pendiente verificar en GitHub |
| README final | Hecho |
| Despliegue público | Pendiente de plataforma y credenciales |

---

## 2. Entorno y herramientas

| Herramienta | Versión / nota |
|---|---|
| .NET SDK | **10.0 (LTS)**. Evitar .NET 11 por ahora (es STS y los paquetes "latest" no son compatibles con net10.0) |
| Editor | VS Code + extensión **C# Dev Kit** |
| Base de datos | PostgreSQL 17 en Docker |
| Pruebas de API | Postman |
| Control de versiones | Git + GitHub |
| Herramienta EF | `dotnet-ef` **10.0.15** (global) |

### Regla de oro de versiones

> La versión mayor de los paquetes de Microsoft debe coincidir con tu framework: `net10.0` usa paquetes `10.x`.

Al instalar paquetes de Microsoft usa siempre:

```
dotnet add package NombreDelPaquete --version "10.0.*"
```

### Paquetes instalados

- `Npgsql.EntityFrameworkCore.PostgreSQL` (10.0.*)
- `Microsoft.EntityFrameworkCore.Design` (10.0.*)
- `Microsoft.AspNetCore.Authentication.JwtBearer` (10.0.*)
- `BCrypt.Net-Next`

---

## 3. Lo que ya está hecho

### 3.1 Estructura del proyecto

```
InventarioApi/
├── Controllers/
│   ├── ProductosController.cs
│   └── AuthController.cs
├── Data/
│   └── AppDbContext.cs
├── Dtos/
│   ├── ProductoCreateDto.cs
│   ├── ProductoUpdateDto.cs
│   ├── ProductoResponseDto.cs
│   ├── RegisterDto.cs
│   ├── LoginDto.cs
│   └── AuthResponseDto.cs
├── Migrations/
├── Models/
│   ├── Producto.cs
│   └── Usuario.cs
├── Services/
│   └── TokenService.cs
├── Program.cs
├── appsettings.json
└── InventarioApi.csproj
```

### 3.2 Proyecto base

```
dotnet new webapi --use-controllers -n InventarioApi
dotnet new gitignore
```

Se eligieron **controladores** (en lugar de Minimal APIs) porque es el estilo más común en empresas.

### 3.3 CRUD de productos

| Verbo | Ruta | Respuesta |
|---|---|---|
| GET | `/api/productos` | 200 |
| GET | `/api/productos/{id}` | 200 / 404 |
| POST | `/api/productos` | 201 Created |
| PUT | `/api/productos/{id}` | 200 / 404 |
| DELETE | `/api/productos/{id}` | 204 / 404 |

### 3.4 PostgreSQL con Docker

Se usa el puerto **5433** porque en este equipo el 5432 lo ocupa un PostgreSQL instalado en Windows.

```
docker run --name inventario-db -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres123 -e POSTGRES_DB=inventario -p 5433:5432 -v inventario-data:/var/lib/postgresql/data -d postgres:17
```

Cadena de conexión en **User Secrets** (nunca en `appsettings.json`):

```
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5433;Database=inventario;Username=postgres;Password=postgres123"
```

### 3.5 Entity Framework Core

- `AppDbContext` con `DbSet<Producto>` y `DbSet<Usuario>`.
- Configuración en `OnModelCreating`: nombre obligatorio (máx. 100), precio con precisión `(18,2)`.
- Registro en `Program.cs` con `AddDbContext` y `UseNpgsql`.
- Controlador con `async/await`, inyección de dependencias del contexto.

```
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 3.6 DTOs y validaciones

- `ProductoCreateDto` y `ProductoUpdateDto` para entrada; `ProductoResponseDto` para salida.
- Validaciones con Data Annotations (`[Required]`, `[StringLength]`, `[Range]`).
- `[ApiController]` devuelve **400 Bad Request** automáticamente si fallan.
- Evita el *over-posting* (el cliente ya no puede mandar un `Id`).

Casos probados (todos dieron el resultado esperado): nombre vacío, nombre de 1 carácter, precio negativo, stock negativo, `id` enviado en el body (se ignora).

### 3.7 Autenticación JWT y autorización

- Registro/login con contraseñas **BCrypt** y claims de id, email y rol.
- La clave JWT se configura en User Secrets o variables de entorno (`Jwt:Key`, mínimo 32 bytes).
- Registro público siempre asigna el rol `Usuario`; el Admin inicial se configura fuera del registro, mediante `Admin:Email` y `Admin:Password`.
- POST y PUT de productos requieren JWT; DELETE requiere rol `Admin`; GET permanece público.
- `GET /api/auth/me` devuelve el perfil asociado al token.

Los casos principales están cubiertos por pruebas xUnit de integración:

| # | Prueba | Esperado |
|---|---|---|
| 1 | Registrar usuario | 201 |
| 2 | Registrar el mismo email | 409 |
| 3 | Login correcto | 200 con `token` |
| 4 | Login con contraseña mala | 401 |
| 5 | Password de menos de 8 caracteres | 400 |
| 6 | POST producto sin token | 401 |
| 7 | POST producto con token (Bearer) | 201 |
| 8 | GET productos sin token | 200 |

También se prueban validaciones, permisos Admin/Usuario, filtros y paginación.

---

## 4. Flujo de trabajo con Git

### Ramas

- `main`: versión estable.
- `develop`: integración.
- `feature/...`: una rama por funcionalidad (ejemplos: `feature/ef-core-postgres`, `feature/dtos-validaciones`, `feature/auth-jwt`).

### Ciclo por cada funcionalidad

Para este proyecto, integra el trabajo gradualmente: crea una rama por funcionalidad desde `develop`, agrega solo sus archivos o fragmentos, valida, abre un Pull Request hacia `develop` y espera el merge antes de iniciar la siguiente. **No uses `git add .`**: el código completo ya está local y eso podría incluir varias funcionalidades en un solo commit.

Como el directorio contiene cambios locales de varias features, primero respáldalos y sigue el procedimiento detallado en [GUIA_GITFLOW.md](./GUIA_GITFLOW.md). No cambies de rama mientras queden cambios sin guardar.

### Mensajes de commit (Conventional Commits)

| Prefijo | Uso |
|---|---|
| `feat:` | Nueva funcionalidad |
| `fix:` | Corrección de error |
| `docs:` | Documentación |
| `chore:` | Tareas de mantenimiento |
| `test:` | Pruebas |
| `refactor:` | Reorganizar código sin cambiar comportamiento |

### Antes de cada commit

- Revisar `git status`.
- Confirmar que **no** se suben contraseñas ni claves (deben vivir solo en User Secrets).
- Confirmar que `Migrations/` **sí** se sube y `bin/` y `obj/` **no**.

---

## 5. Problemas resueltos y lecciones aprendidas

| Problema | Causa | Solución |
|---|---|---|
| `obj/` seguía en GitHub | Ya estaba rastreado; `.gitignore` no quita archivos existentes | `git rm -r --cached obj` + commit |
| `git push` fallaba (sin upstream) | Rama creada localmente | `git push -u origin nombre-rama` |
| `NU1202` EF Design 11.0.0 no compatible | NuGet instaló la última versión (11) y el proyecto es net10.0 | `--version "10.0.*"` |
| `dotnet-ef` en 11.0.0 | `update` no permite bajar versión | `uninstall` y luego `install --version "10.0.15"` |
| `Build failed` al migrar | La API seguía corriendo y bloqueaba los `.dll` | Detener con `Ctrl+C` antes de compilar o migrar |
| `28P01` autenticación fallida | Dos procesos escuchaban en 5432 (PostgreSQL local y Docker) | Mover el contenedor al puerto 5433 |
| `relation "Productos" does not exist` | La migración no se había aplicado | `dotnet ef database update` |
| `git pull` rechazado por archivos de `obj/` | Cambios locales sobre archivos que la rama nueva ya no rastrea | `git restore obj` y repetir el pull |

### Reglas prácticas que salieron de aquí

1. Detén la API antes de instalar paquetes, compilar o migrar.
2. Versiones de paquetes Microsoft = versión de tu framework.
3. Secretos solo en User Secrets o variables de entorno.
4. Crea cada rama desde `develop` actualizada.
5. Si un error no se entiende, ejecuta `dotnet build` para ver el mensaje real.

---

## 6. Lo que falta por hacer

Las etapas 1 a 8 ya están implementadas en el código local. La guía de ejecución, los secretos requeridos y los comandos están en el [README.md](./README.md).

### Etapa 9: Despliegue

Esta parte requiere decisiones y credenciales que no deben inventarse ni guardarse en el repositorio:

1. Elegir plataforma: **Azure App Service**, **Railway** o **Render**.
2. Crear una base PostgreSQL gestionada.
3. Configurar `ConnectionStrings__DefaultConnection`, `Jwt__Key`, `Admin__Email` y `Admin__Password` como variables seguras de la plataforma.
4. Desplegar y probar la URL pública.
5. Agregar el enlace real de la demo al README.

### Etapa 10 (opcional): Funciones de negocio

- **Categorías**: entidad con relación uno a muchos con productos.
- **Movimientos de inventario**: entradas y salidas de stock con historial.
- **Alerta de stock bajo**: endpoint que lista productos bajo un mínimo.

---

## 7. Orden de trabajo y estado

| Etapa | Qué | Estado |
|---|---|---|---|
| 1-8 | API, autorización, documentación, tests, Docker y CI | Implementado localmente |
| 9 | Despliegue público | Pendiente de plataforma/credenciales |
| 10 | Funcionalidades opcionales | Pendiente |

No se han creado commits, ramas, Pull Requests ni se ha subido el código.

---

## 8. Referencia rápida de comandos

### .NET

```
dotnet run                      # ejecutar la API
dotnet watch run                # ejecutar y recargar al guardar
dotnet build                    # compilar (muestra errores reales)
dotnet test                     # ejecutar pruebas
dotnet add package X --version "10.0.*"
```

### Entity Framework

```
dotnet ef migrations add NombreMigracion
dotnet ef database update
dotnet ef migrations remove     # deshacer la última migración (si no se aplicó)
```

### User Secrets

```
dotnet user-secrets list
dotnet user-secrets set "Clave" "valor"
```

### Docker

```
docker ps                       # contenedores activos
docker ps -a                    # todos
docker start inventario-db
docker stop inventario-db
docker logs inventario-db
docker rm -f inventario-db      # borrar contenedor
docker volume rm inventario-data  # borrar datos (cuidado)
```

### Git

```
git status
git branch
git checkout -b feature/nombre
git add .
git commit -m "feat: mensaje"
git push -u origin feature/nombre
git pull origin develop
git restore archivo             # descartar cambios locales de un archivo
```

---

## 9. Checklist final del portafolio

**Código**
- [x] CRUD completo con DTOs y validaciones
- [x] JWT con roles
- [x] Paginación y filtros
- [x] Capa de servicios
- [x] Manejo global de errores
- [x] Pruebas unitarias y de integración pasando

**Infraestructura**
- [x] Dockerfile y docker-compose verificados localmente
- [ ] GitHub Actions verificada en GitHub
- [ ] Desplegado con enlace público
- [x] Secretos fuera del repositorio (`.env.example` incluido)

**Repositorio**
- [x] `.gitignore` en la raíz del repo
- [ ] Repositorio renombrado a algo corto (`inventario-api`)
- [ ] Historial de commits ordenado (Conventional Commits)
- [ ] Pull Requests mergeados a `develop` y `main`
- [x] Carpeta `bin/` y `obj/` sin rastrear

**README**
- [x] Descripción y tecnologías
- [ ] Diagrama sencillo de la arquitectura
- [x] Cómo ejecutarlo (`docker compose up`)
- [x] Lista de endpoints
- [ ] Capturas de Swagger
- [x] Badge de CI; enlace a demo pendiente del despliegue
- [ ] Colección de Postman exportada

**Perfil profesional**
- [ ] GitHub con README de perfil y proyecto fijado
- [ ] LinkedIn actualizado con el enlace al proyecto
- [ ] CV de una página
- [ ] Preparar preguntas de entrevista: POO, SQL, REST, Git, inyección de dependencias, async/await

---

## Después de este proyecto

1. Segundo proyecto distinto (por ejemplo, con **Blazor** como frontend que consuma esta API, o una arquitectura en capas más completa).
2. Empezar a postularte a prácticas y puestos junior **mientras** terminas el portafolio; no esperes a que esté perfecto.