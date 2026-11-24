# SATURNO

Panel ASP.NET Core MVC para empleados, certificados y catálogos. La API JSON y el panel usan MongoDB mediante el controlador oficial.

## Ejecutar en Windows

Abre CMD en la carpeta que contiene `NoSqlU.sln`. Necesitas el SDK de .NET 7 o uno compatible, el runtime de ASP.NET Core 7, MongoDB iniciado y `mongosh` disponible.

```cmd
mongosh mongodb://localhost:27017/SaturnoMongo --file scripts/mongodb/init.js
dotnet run --project src\Api\Api.csproj
```

El script prepara una base nueva con colecciones, validadores, índices y catálogos iniciales. Puedes repetirlo en la misma base para actualizar los validadores y completar los datos iniciales.

- Panel de empleados en [localhost:62391](http://localhost:62391/).
- Comprobación de conexión y catálogos en [/api/health](http://localhost:62391/api/health).
- Documentación de la API en [/swagger](http://localhost:62391/swagger).

La configuración local está en `src/Api/appsettings.json`. Para otra instancia usa `MongoDB__ConnectionString` y `MongoDB__Database` y ejecuta el script sobre la base correspondiente. Para una URI con credenciales, usa User Secrets o una variable de entorno.

## Modelo de datos

- `empleados` contiene datos personales, domicilios y certificados. Los registros de la API que tienen credencial guardan el hash dentro del documento.
- `sexos`, `estados`, `categorias`, `niveles`, `instructores` y `cursos` son catálogos independientes.
- Los identificadores numéricos mantienen los contratos actuales de la API y el número de empleado. MongoDB asigna esos números mediante incrementos atómicos en `contadores`. El catálogo de sexos usa códigos de un car?cter.
- Los certificados pueden registrar nombre e institución sin pertenecer al catálogo de cursos.

La base arranca con catálogos y cursos de ejemplo. Los empleados se crean desde el panel o la API.
