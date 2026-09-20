Resumen de migración a C# y SQL Server (LocalDB)

Estructura creada:
- src/Api: proyecto ASP.NET Core Web API (net7.0)
- src/Data: proyecto de acceso a datos con EF Core (SqlServer)
- src/Domain: proyecto de dominio (clases de negocio)
- src/Tests: proyecto de pruebas (xUnit)

Pasos para abrir y ejecutar en su máquina:
1. Abra una terminal PowerShell en la raíz del repo.
2. Restaurar paquetes: dotnet restore src/Api
3. (Opcional) Crear solución y agregar proyectos:
   dotnet new sln -n NoSqlU
   dotnet sln add src/Api/Api.csproj
   dotnet sln add src/Data/Data.csproj
   dotnet sln add src/Domain/Domain.csproj
   dotnet sln add src/Tests/Tests.csproj
4. Aplicar migraciones y crear la base de datos (requiere dotnet-ef):
   cd src/Data
   dotnet add package Microsoft.EntityFrameworkCore.Design
   cd ..\Api
   dotnet tool install --global dotnet-ef
   dotnet ef migrations add InitialCreate -p ..\Data\Data.csproj -s .\Api.csproj
   dotnet ef database update -p ..\Data\Data.csproj -s .\Api.csproj

   Si prefiere LocalDB asegúrese de tener LocalDB instalado (incluido con Visual Studio). La cadena de conexión por defecto está en src/Api/appsettings.json.

5. Ejecutar la API:
   dotnet run --project src/Api

Notas:
- El esquema original de MySQL se ha mapeado a entidades EF Core en src/Data/Entities.
- Las tablas y restricciones (foreign keys y check constraints) se configuran en NoSqlUContext.
- La semilla de datos incluye niveles; puede extenderse para insertar categorías, instructores y cursos.

Si deseas, puedo ejecutar la generación de migraciones y ajustar la semilla para insertar los datos completos del archivo api-cursos/sql/schema.sql.


## Ejecución local corregida

1. Verifica que SQL Server esté iniciado y que la base `NoSqlU1_migrated` exista.
2. Si aún no existe, ejecuta `scripts/sqlserver/schema.sql` en SQL Server Management Studio.
3. La API usa:
   - HTTP: `http://localhost:62391`
   - HTTPS: `https://localhost:62390`
4. Si el frontend corre con Live Server en `http://localhost:8080`, no cambies ese puerto por el de la API.
5. Prueba primero `http://localhost:62391/api/health`. Debe responder:
   `{ "db": "ok" }`
6. Después abre el frontend en `http://localhost:8080/frontend/admin/index.html`.

La conexión de `appsettings.json` apunta explícitamente a la base `NoSqlU1_migrated`. Si tu instancia de SQL Server no es `localhost`, cambia únicamente `Server=...` por el nombre de servidor que aparece en SSMS.
