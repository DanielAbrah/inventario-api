# Git Flow: comandos por funcionalidad

Ejecuta cada feature completa antes de pasar a la siguiente: seleccionar archivos, revisar, validar, hacer commit, subir la rama y fusionarla en `develop`. No uses `git add .`: el código tiene varias funcionalidades mezcladas.

> **Tu situación actual:** ya estás en `feature/auth-jwt` y ejecutaste `git stash apply "stash@{0}"`. El mensaje `could not restore untracked files from stash` se refiere a `GUIA_GITFLOW.md`, que ya existía; los demás cambios sí aparecen en el estado que compartiste. **No vuelvas a ejecutar el bloque de recuperación ni a aplicar el stash original.** El stash se conserva como respaldo.

## 0. Respaldo inicial

Si ya hiciste este stash, **no lo repitas**. Comprueba que está:

```powershell
git status --short
git stash list
```

Solo si todavía no aparece `respaldo-antes-de-dividir-funcionalidades`:

```powershell
git stash push --include-untracked -m "respaldo-antes-de-dividir-funcionalidades"
git stash list
git status --short
```

## 1. `feature/auth-jwt`

Ya estás en esta rama y los cambios están recuperados. No repitas `git switch -c` ni `git stash apply` en este paso.

Agrega solo JWT/autenticación. Para los archivos compartidos selecciona los fragmentos de esta feature en el modo interactivo:

```powershell
git add InventarioApi\Models\Usuario.cs
git add InventarioApi\Controllers\AuthController.cs
git add InventarioApi\Dtos\AuthResponseDto.cs InventarioApi\Dtos\LoginDto.cs InventarioApi\Dtos\RegisterDto.cs InventarioApi\Dtos\UsuarioResponseDto.cs
git add InventarioApi\Services\AuthService.cs InventarioApi\Services\IAuthService.cs InventarioApi\Services\TokenService.cs InventarioApi\Services\ITokenService.cs
git add InventarioApi\Migrations\20261120015656_AddUsuarios.cs InventarioApi\Migrations\20261120015656_AddUsuarios.Designer.cs
git add -p InventarioApi\Data\AppDbContext.cs
git add -p InventarioApi\Migrations\AppDbContextModelSnapshot.cs
git add -p InventarioApi\Program.cs
git add -p InventarioApi\InventarioApi.csproj
```

Revisa y valida lo que seleccionaste:

```powershell
git diff --cached --check
git diff --cached --stat
git diff --cached
dotnet build InventarioApi\InventarioApi.csproj --configuration Release
```

Si el diff staged contiene solo JWT/autenticación y el build pasa, termina **esta feature**:

```powershell
git commit -m "feat(auth): add JWT registration and login"
git stash push --include-untracked -m "pendientes-despues-auth-jwt"
git stash list
git push -u origin feature/auth-jwt
git switch develop
git pull --ff-only origin develop
git merge --no-ff feature/auth-jwt -m "merge: auth JWT"
git push origin develop
```

El stash de pendientes guarda las otras funcionalidades para poder cambiar de rama; el respaldo original también se conserva. Confirma en `git stash list` que `pendientes-despues-auth-jwt` quedó arriba.

## 2. `feature/roles-admin`

Después de completar y fusionar la feature anterior:

```powershell
git status --short
git switch develop
git pull --ff-only origin develop
git switch -c feature/roles-admin
git stash apply "stash@{0}"

git add InventarioApi\Services\AdminSeeder.cs InventarioApi\Services\IAdminSeeder.cs
git add -p InventarioApi\Controllers\ProductosController.cs
git add -p InventarioApi\Program.cs
git diff --cached --check
git diff --cached --stat
git diff --cached
dotnet build InventarioApi\InventarioApi.csproj --configuration Release
git commit -m "feat(auth): restrict product deletion to admins"
git stash push --include-untracked -m "pendientes-despues-roles-admin"
git stash list
git push -u origin feature/roles-admin
git switch develop
git pull --ff-only origin develop
git merge --no-ff feature/roles-admin -m "merge: admin roles"
git push origin develop
```

## 3. `feature/product-service-pagination`

```powershell
git switch develop
git pull --ff-only origin develop
git switch -c feature/product-service-pagination
git stash apply "stash@{0}"

git add InventarioApi\Services\IProductoService.cs InventarioApi\Services\ProductoService.cs
git add InventarioApi\Dtos\ProductoCreateDto.cs InventarioApi\Dtos\ProductoUpdateDto.cs InventarioApi\Dtos\ProductoResponseDto.cs
git add InventarioApi\Dtos\ProductoQueryDto.cs InventarioApi\Dtos\PagedResultDto.cs
git add -p InventarioApi\Controllers\ProductosController.cs
git add -p InventarioApi\Program.cs
git diff --cached --check
git diff --cached --stat
git diff --cached
dotnet build InventarioApi\InventarioApi.csproj --configuration Release
git commit -m "feat(products): add service layer and pagination"
git stash push --include-untracked -m "pendientes-despues-product-service-pagination"
git stash list
git push -u origin feature/product-service-pagination
git switch develop
git pull --ff-only origin develop
git merge --no-ff feature/product-service-pagination -m "merge: product service and pagination"
git push origin develop
```

## 4. `feature/openapi-errors`

```powershell
git switch develop
git pull --ff-only origin develop
git switch -c feature/openapi-errors
git stash apply "stash@{0}"

git add InventarioApi\Middleware
git add -p InventarioApi\Program.cs
git add -p InventarioApi\InventarioApi.csproj
git add -p InventarioApi\Controllers\AuthController.cs
git add -p InventarioApi\Controllers\ProductosController.cs
git diff --cached --check
git diff --cached --stat
git diff --cached
dotnet build InventarioApi\InventarioApi.csproj --configuration Release
git commit -m "feat(api): add Scalar OpenAPI and global errors"
git stash push --include-untracked -m "pendientes-despues-openapi-errors"
git stash list
git push -u origin feature/openapi-errors
git switch develop
git pull --ff-only origin develop
git merge --no-ff feature/openapi-errors -m "merge: OpenAPI and error handling"
git push origin develop
```

## 5. `feature/xunit-tests`

Agrega `.gitignore` antes de compilar las pruebas.

```powershell
git switch develop
git pull --ff-only origin develop
git switch -c feature/xunit-tests
git stash apply "stash@{0}"

git add .gitignore InventarioApi.Tests InventarioApi.slnx
git add -p InventarioApi\Program.cs
git diff --cached --check
git diff --cached --stat
git diff --cached
dotnet build InventarioApi.slnx --configuration Release
dotnet test InventarioApi.slnx --configuration Release
git commit -m "test: add xUnit unit and integration tests"
git stash push --include-untracked -m "pendientes-despues-xunit-tests"
git stash list
git push -u origin feature/xunit-tests
git switch develop
git pull --ff-only origin develop
git merge --no-ff feature/xunit-tests -m "merge: xUnit tests"
git push origin develop
```

## 6. `feature/docker-ci`

```powershell
git switch develop
git pull --ff-only origin develop
git switch -c feature/docker-ci
git stash apply "stash@{0}"

git add Dockerfile docker-compose.yml .dockerignore .env.example .github\workflows\ci.yml
git diff --cached --check
git diff --cached --stat
git diff --cached
dotnet build InventarioApi.slnx --configuration Release
dotnet test InventarioApi.slnx --configuration Release
git commit -m "ci: add Docker Compose and GitHub Actions"
git stash push --include-untracked -m "pendientes-despues-docker-ci"
git stash list
git push -u origin feature/docker-ci
git switch develop
git pull --ff-only origin develop
git merge --no-ff feature/docker-ci -m "merge: Docker and CI"
git push origin develop
```

## 7. `docs/project-guide`

```powershell
git switch develop
git pull --ff-only origin develop
git switch -c docs/project-guide
git stash apply "stash@{0}"

git add README.md GUIA.md GUIA_GITFLOW.md InventarioApi\InventarioApi.http
git diff --cached --check
git diff --cached --stat
git diff --cached
dotnet build InventarioApi.slnx --configuration Release
dotnet test InventarioApi.slnx --configuration Release
git commit -m "docs: document API setup and Git Flow"
git push -u origin docs/project-guide
git switch develop
git pull --ff-only origin develop
git merge --no-ff docs/project-guide -m "merge: project documentation"
git push origin develop
```

## 8. Si `develop` está protegido

En cada feature, después de `git push -u origin RAMA`, abre un PR de esa rama hacia `develop` y fusiónalo en GitHub. **No ejecutes** el `git merge` ni `git push origin develop` de los bloques anteriores. El resto de los cambios ya quedó guardado en un stash antes del push; después de fusionar, sincroniza `develop`:

```powershell
git switch develop
git pull --ff-only origin develop
git stash list
```

Aplica el stash más reciente (`stash@{0}`) en la siguiente feature, tal como aparece en su bloque. Verifica siempre el mensaje con `git stash list` antes de aplicarlo.

## 9. Verificación final y `main`

```powershell
git switch develop
git pull --ff-only origin develop
dotnet build InventarioApi.slnx --configuration Release
dotnet test InventarioApi.slnx --configuration Release
git status --short
git stash list
```

Cuando todo esté integrado y verificado:

```powershell
git switch main
git pull --ff-only origin main
git merge --no-ff develop -m "release: complete inventory API"
git push origin main
git switch develop
```

Si `main` está protegida, abre un PR de `develop` hacia `main` en vez del merge/push local. Conserva el stash `respaldo-antes-de-dividir-funcionalidades` hasta confirmar que todo quedó integrado. No subas `.env`, claves ni contraseñas.
