# SATURNO

Panel ASP.NET Core MVC para empleados, certificados y catálogos de sexo y estado. Usa MongoDB mediante el controlador oficial.

## Estructura

- `backend/` contiene los proyectos .NET, los controladores, los modelos, el acceso a MongoDB, el inicializador y las pruebas.
- `frontend/` contiene las vistas Razor, el JavaScript de las pantallas y los estilos.

La separación permite localizar los cambios visuales y las reglas del servidor en carpetas distintas. El proyecto web compila las vistas y empaqueta los estilos para servir todo desde la misma aplicación MVC.

| Módulo | Controlador | Modelo | Vistas |
|---|---|---|---|
| Empleados | `Controllers/Empleados` | `Models/Empleados` | `frontend/Views/Empleados` |
| Catálogos | `Controllers/Catalogos` | `Models/Catalogos` | `frontend/Views/Catalogos` |
| Sesión | `Controllers/Sesion` | `Models/Sesion` | `frontend/Views/Sesion` |

Los controladores y modelos están bajo `backend/Api`. `Models` reúne los modelos de formulario, las reglas de cada módulo y, en `Models/Data`, las entidades y el acceso a MongoDB.

## Ejecutar en Windows

Abre CMD en la carpeta que contiene `NoSqlU.sln`. Necesitas el SDK de .NET 7 o uno compatible, el runtime de ASP.NET Core 7, MongoDB iniciado y `mongosh` disponible.

```cmd
mongosh mongodb://localhost:27017/SaturnoMongo --file backend/scripts/mongodb/init.js
dotnet user-secrets set "AdministradorInicial:Usuario" "admin" --project backend/Api/Api.csproj
dotnet user-secrets set "AdministradorInicial:Password" "TU_CONTRASENA" --project backend/Api/Api.csproj
dotnet run --project backend\Api\Api.csproj
```

El script prepara las colecciones, los validadores, los índices y los catálogos iniciales. Puedes repetirlo en la misma base para actualizar los validadores.

Sustituye `TU_CONTRASENA` por tu contraseña. Al arrancar, la aplicación crea la cuenta si ese usuario todavía no existe y guarda su hash en `administradores`. Cuando ya puedas iniciar sesión, puedes quitar el secreto `AdministradorInicial:Password` con `dotnet user-secrets remove "AdministradorInicial:Password" --project backend/Api/Api.csproj`. El alta inicial conserva la contraseña del usuario si ya existía.

- Panel de empleados en [localhost:62391](http://localhost:62391/).

La configuración local está en `backend/Api/appsettings.json`. Para otra instancia usa `MongoDB__ConnectionString` y `MongoDB__Database` y ejecuta el script sobre la base correspondiente. Para una URI con credenciales, usa User Secrets o una variable de entorno.

Para preparar la revisión, consulta la [guía de estudio](GUIA_ESTUDIO_REVISION_MONGODB.md) y la [guía paso a paso para principiantes](GUIA_PASO_A_PASO_SATURNO_MONGODB.md). Ambas describen una versión anterior que incluía la API REST y pantallas adicionales de cursos.

## Modelo de datos

- `empleados` contiene datos personales, domicilios y certificados. Los documentos creados en versiones anteriores pueden incluir el campo `credencial`.
- `sexos`, `estados`, `categorias`, `niveles`, `instructores` y `cursos` son catálogos independientes.
- MongoDB asigna los números de empleado y los identificadores de estado mediante incrementos atómicos en `contadores`. El catálogo de sexos usa códigos de un carácter.
- Los certificados pueden registrar nombre e institución sin pertenecer al catálogo de cursos.
- La pantalla principal lista empleados y ofrece filtros y paginación. Los cursos del formulario guardan nombre, duración y fecha de inicio o entrega de constancia. Un empleado debe estar inactivo para poder eliminarlo definitivamente.

La base arranca con catálogos y cursos de ejemplo. Los empleados se crean desde el panel. La pantalla principal muestra los registros existentes, cinco por página.
