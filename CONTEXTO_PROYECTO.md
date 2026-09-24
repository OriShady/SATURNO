# Contexto Técnico Completo de SATURNoSQL

> Documento de referencia para desarrolladores y asistentes de IA.
>
> Fecha de revisión: 2026-09-23.

## 1. Propósito del proyecto

SATURNoSQL es un sistema administrativo para gestionar cursos educativos.

Permite:

- Crear, consultar y actualizar categorías.
- Crear, consultar y desactivar cursos.
- Consultar y administrar niveles.
- Consultar y administrar instructores.
- Buscar cursos por texto, categoría, nivel, instructor, estado y precio.
- Mostrar estadísticas generales del sistema.
- Comprobar la conexión con la base de datos.

Aunque el nombre contiene `NoSQL`, la implementación actual no usa MongoDB ni otra base NoSQL. Usa SQL Server mediante Entity Framework Core. El nombre parece provenir de una versión o proyecto anterior.

## 2. Arquitectura general

```text
Frontend HTML/CSS/JavaScript
        |
        | HTTP mediante fetch
        v
ASP.NET Core Web API (.NET 7)
        |
        | Controllers
        v
Entity Framework Core 7
        |
        | SQL Server Provider
        v
SQL Server: NoSqlU1_migrated
```

Capas actuales:

- `frontend`: interfaz administrativa estática.
- `src/Api`: API HTTP y controladores.
- `src/Data`: entidades, `DbContext`, configuración EF Core y migraciones.
- `src/Domain`: proyecto reservado para dominio, actualmente vacío.
- `src/Tests`: proyecto xUnit, actualmente sin pruebas implementadas.

## 3. Estructura del repositorio

```text
SATURNoSQL/
├── .github/
│   └── workflows/
│       └── dotnet.yml
├── .vscode/
│   └── settings.json
├── frontend/
│   ├── admin/
│   │   ├── index.html
│   │   ├── categorias.html
│   │   ├── cursos.html
│   │   └── busqueda.html
│   ├── css/
│   │   ├── admin.css
│   │   └── cursos.css
│   └── js/
│       ├── dashboard.js
│       ├── categorias.js
│       ├── cursos.js
│       └── busqueda.js
├── scripts/
│   └── sqlserver/
│       └── schema.sql
├── src/
│   ├── Api/
│   ├── Data/
│   ├── Domain/
│   └── Tests/
├── NoSqlU.sln
├── README_MIGRATION.md
├── .gitignore
├── .gitattributes
└── CONTEXTO_PROYECTO.md
```

Las carpetas `bin/`, `obj/` y `.vs/` contienen artefactos generados por .NET y Visual Studio. No son código fuente principal.

## 4. Archivos de la raíz

### `NoSqlU.sln`

Solución de Visual Studio. Agrupa estos proyectos:

- `src/Api/Api.csproj`
- `src/Data/Data.csproj`
- `src/Domain/Domain.csproj`
- `src/Tests/Tests.csproj`

### `README_MIGRATION.md`

Documenta la migración a C# y SQL Server, instalación de paquetes, aplicación de migraciones y ejecución local.

Contiene una discrepancia histórica: menciona Live Server en el puerto `8080`, pero la configuración actual de VS Code usa el puerto `5502`.

### `.gitignore`

Ignora resultados de compilación, archivos temporales de Visual Studio, `bin/`, `obj/`, `.vs/`, logs y otros artefactos generados.

### `.gitattributes`

Configura normalización de finales de línea y comportamiento de Git para archivos del proyecto.

## 5. Proyecto `src/Api`

### Propósito

Backend HTTP de ASP.NET Core. Recibe solicitudes del frontend, ejecuta operaciones mediante EF Core y devuelve JSON.

### `Api.csproj`

Proyecto ASP.NET Core sobre `net7.0`.

Paquetes principales:

- `Microsoft.EntityFrameworkCore.SqlServer` 7.0.0
- `Microsoft.EntityFrameworkCore.Tools` 7.0.0
- `Swashbuckle.AspNetCore` 6.5.0

Referencias:

- `Data`
- `Domain`

### `Program.cs`

Configura la aplicación:

1. Registra controladores.
2. Registra Swagger.
3. Configura CORS.
4. Obtiene `DefaultConnection`.
5. Registra `NoSqlUContext` con `UseSqlServer`.
6. Configura el pipeline HTTP.
7. Sirve opcionalmente `frontend/admin` bajo `/admin`.
8. Mapea los controladores.

CORS actualmente permite cualquier origen, método y encabezado:

```text
AllowAnyOrigin
AllowAnyMethod
AllowAnyHeader
```

Swagger y la página de errores detallados se habilitan en `Development`.

La API busca el frontend en una ruta relativa equivalente a:

```text
../../frontend/admin
```

Si la carpeta existe, se puede acceder bajo `/admin`.

El programa no ejecuta automáticamente `Database.Migrate()`. La base debe prepararse manualmente.

### `appsettings.json`

Cadena de conexión actual:

```text
Server=localhost;
Database=NoSqlU1_migrated;
Trusted_Connection=True;
TrustServerCertificate=True;
MultipleActiveResultSets=True;
Application Name=NoSqlU;
```

Significado:

- Servidor SQL Server local: `localhost`.
- Base de datos: `NoSqlU1_migrated`.
- Autenticación integrada de Windows mediante `Trusted_Connection=True`.
- `TrustServerCertificate=True` para el entorno local.
- `MultipleActiveResultSets=True` habilitado.

Existe en `Program.cs` un fallback diferente si falta la configuración:

```text
Server=(localdb)\\mssqllocaldb;Database=NoSqlU1_migrated;Trusted_Connection=True;
```

Por tanto, la conexión configurada normalmente usa `localhost`, pero el fallback usa LocalDB.

### `Properties/launchSettings.json`

Perfil de desarrollo:

```text
HTTP:  http://localhost:62391
HTTPS: https://localhost:62390
```

También establece:

```text
ASPNETCORE_ENVIRONMENT=Development
```

## 6. Controladores y endpoints

Todos usan la ruta base `api/[controller]`.

### Health

Archivo: `src/Api/Controllers/HealthController.cs`

```http
GET /api/health
```

Comprueba `Database.CanConnectAsync()`.

Respuestas esperadas:

```json
{ "db": "ok" }
```

También puede devolver `503` si la base no está disponible o `500` con detalles del error.

### Categorías

Archivo: `src/Api/Controllers/CategoriasController.cs`

```http
GET    /api/categorias
GET    /api/categorias/{id}
POST   /api/categorias
PUT    /api/categorias/{id}
DELETE /api/categorias/{id}
```

`POST` crea una categoría y asigna `FechaCreacion`.

`PUT` modifica:

- `Nombre`
- `Descripcion`
- `Estatus`

`DELETE` elimina físicamente el registro. Si tiene cursos relacionados, la restricción `Restrict` puede impedirlo.

### Niveles

Archivo: `src/Api/Controllers/NivelesController.cs`

```http
GET    /api/niveles
GET    /api/niveles/{id}
POST   /api/niveles
PUT    /api/niveles/{id}
DELETE /api/niveles/{id}
```

El borrado es físico. También puede fallar si existen cursos relacionados.

### Instructores

Archivo: `src/Api/Controllers/InstructoresController.cs`

```http
GET    /api/instructores
GET    /api/instructores/{id}
POST   /api/instructores
PUT    /api/instructores/{id}
DELETE /api/instructores/{id}
```

El `POST` busca por nombre ignorando mayúsculas y minúsculas. Si encuentra uno existente, actualiza sus datos y lo reactiva; de lo contrario, crea uno nuevo.

El `DELETE` es lógico:

```text
estatus = "inactivo"
```

### Cursos

Archivo: `src/Api/Controllers/CursosController.cs`

```http
GET    /api/cursos
GET    /api/cursos/{id}
POST   /api/cursos
PUT    /api/cursos/{id}
DELETE /api/cursos/{id}
```

`GET` carga también:

- Categoría.
- Nivel.
- Instructor.

El `POST` valida:

- Nombre.
- Descripción.
- Categoría.
- Nivel o nombre de nivel.
- Instructor o nombre de instructor.
- Precio mayor que cero.
- Duración mayor que cero.
- Fecha de publicación.

Si recibe un nivel por nombre y no existe, lo crea. Lo mismo hace con el instructor. Estas operaciones se realizan dentro de una transacción junto con la creación del curso.

El `PUT` solamente modifica `estatus`.

El `DELETE` es lógico y cambia el curso a `inactivo`.

### Búsqueda

Archivo: `src/Api/Controllers/FiltroController.cs`

```http
GET /api/filtro/buscar
```

Parámetros:

```text
q
categoria
nivel
instructor
estatus
min
max
```

Filtros:

- `q`: busca en el nombre o descripción.
- `categoria`: nombre exacto.
- `nivel`: nombre exacto.
- `instructor`: nombre exacto.
- `estatus`: estado exacto.
- `min`: precio mínimo.
- `max`: precio máximo.

La consulta utiliza LINQ y EF Core la traduce a SQL Server.

## 7. DTO de cursos

Archivo: `src/Api/Models/CourseCreateDto.cs`

Representa la entrada para crear cursos.

Campos:

```text
Nombre
Descripcion
Precio
DuracionMinutos
FechaPublicacion
CategoriaId
NivelId
Nivel
InstructorId
Instructor
```

El cliente puede enviar `NivelId` o `Nivel`. También puede enviar `InstructorId` o `Instructor`.

## 8. Proyecto `src/Data`

### Propósito

Contiene el acceso a datos, entidades, contexto EF Core, configuración de tablas, relaciones y migraciones.

### `Data.csproj`

Biblioteca .NET 7.

Paquetes principales:

- `Microsoft.EntityFrameworkCore.SqlServer` 7.0.0
- `Microsoft.EntityFrameworkCore.Design` 7.0.0

Referencia:

- `Domain`

### `NoSqlUContext.cs`

Es el `DbContext` principal.

DbSets:

```csharp
DbSet<Categoria> Categorias
DbSet<Nivel> Niveles
DbSet<Instructor> Instructores
DbSet<Curso> Cursos
```

Configura nombres de tablas y columnas, índices únicos, relaciones, restricciones y datos iniciales.

Relaciones:

```text
Categoria  1 ---- N Cursos
Nivel      1 ---- N Cursos
Instructor 1 ---- N Cursos
```

Las claves foráneas usan eliminación restringida (`DeleteBehavior.Restrict`).

Restricciones:

```text
precio > 0
duracion_minutos > 0
```

Índices únicos:

- `categorias.nombre`
- `niveles.nombre`
- `instructores.nombre`

## 9. Entidades

### `Categoria.cs`

Tabla: `categorias`.

Campos:

```text
Id
Nombre
Descripcion
Estatus
FechaCreacion
Cursos
```

### `Nivel.cs`

Tabla: `niveles`.

Campos:

```text
Id
Nombre
Estatus
FechaCreacion
Cursos
```

### `Instructor.cs`

Tabla: `instructores`.

Campos:

```text
Id
Nombre
Apellido
Email
Estatus
FechaCreacion
Cursos
```

### `Curso.cs`

Tabla: `cursos`.

Campos:

```text
Id
CategoriaId
NivelId
InstructorId
Nombre
Descripcion
Precio
DuracionMinutos
FechaPublicacion
Estatus
FechaCreacion
Categoria
Nivel
Instructor
```

Las propiedades `Categoria`, `Nivel` e `Instructor` son propiedades de navegación de EF Core.

## 10. Modelo de base de datos

### Tabla `categorias`

```text
id_categoria    INT IDENTITY PRIMARY KEY
nombre          VARCHAR/NVARCHAR(100) UNIQUE NOT NULL
descripcion     VARCHAR/NVARCHAR(255) NULL
estatus         VARCHAR/NVARCHAR(20) NOT NULL
fecha_creacion  DATETIME2 NOT NULL
```

### Tabla `niveles`

```text
id_nivel        INT IDENTITY PRIMARY KEY
nombre          VARCHAR/NVARCHAR(30) UNIQUE NOT NULL
estatus         VARCHAR/NVARCHAR(20) NOT NULL
fecha_creacion  DATETIME2 NOT NULL
```

### Tabla `instructores`

```text
id_instructor   INT IDENTITY PRIMARY KEY
nombre          VARCHAR/NVARCHAR(150) UNIQUE NOT NULL
apellido        VARCHAR/NVARCHAR(100) NULL
email           VARCHAR/NVARCHAR(150) NULL
estatus         VARCHAR/NVARCHAR(20) NOT NULL
fecha_creacion  DATETIME2 NOT NULL
```

### Tabla `cursos`

```text
id_curso          INT IDENTITY PRIMARY KEY
id_categoria      INT NOT NULL
id_nivel          INT NOT NULL
id_instructor     INT NOT NULL
nombre            VARCHAR/NVARCHAR(200) NOT NULL
descripcion       TEXT/NVARCHAR(MAX) NOT NULL
precio            DECIMAL(10,2) NOT NULL
duracion_minutos  INT o BIGINT NOT NULL
fecha_publicacion DATE o DATETIME2 NOT NULL
estatus           VARCHAR/NVARCHAR(20) NOT NULL
fecha_creacion    DATETIME2 NOT NULL
```

Relaciones:

```text
cursos.id_categoria  -> categorias.id_categoria
cursos.id_nivel      -> niveles.id_nivel
cursos.id_instructor -> instructores.id_instructor
```

## 11. Migraciones EF Core

Carpeta: `src/Data/Migrations`

### `20260921030041_ReconstruccionLimpia.cs`

Migración principal. Su método `Up` crea tablas, índices, claves foráneas, restricciones y datos iniciales. Su método `Down` elimina las tablas.

Datos iniciales de la migración:

- 5 categorías.
- 3 niveles.
- 4 instructores.
- 6 cursos.

### `20260921030041_ReconstruccionLimpia.Designer.cs`

Archivo generado por EF Core con metadatos internos de la migración. No debe editarse manualmente.

### `NoSqlUContextModelSnapshot.cs`

Snapshot del modelo actual de EF Core. EF Core lo utiliza para detectar cambios y generar futuras migraciones.

## 12. Script SQL alternativo

Archivo: `scripts/sqlserver/schema.sql`

Hace lo siguiente:

1. Crea `NoSqlU1_migrated` si no existe.
2. Selecciona esa base.
3. Crea `categorias`, `niveles`, `instructores` y `cursos`.
4. Agrega claves primarias y foráneas.
5. Agrega restricciones de precio y duración.
6. Inserta tres niveles.
7. Inserta dos categorías.

No inserta instructores ni cursos.

### Diferencias entre `schema.sql` y EF Core

| Elemento | `schema.sql` | Migración EF Core |
|---|---|---|
| Texto | `VARCHAR` | `NVARCHAR` |
| Duración | `INT` | `BIGINT` |
| Fecha de publicación | `DATE` | `DATETIME2` |
| Categorías iniciales | 2 | 5 |
| Instructores iniciales | 0 | 4 |
| Cursos iniciales | 0 | 6 |
| Historial de migraciones | No crea `__EFMigrationsHistory` | Sí lo usa EF Core |

La diferencia más delicada es `DuracionMinutos`: en C# es `uint`, EF Core la genera como `bigint`, mientras que el script la crea como `int`.

No se recomienda ejecutar ambas estrategias sin coordinación. El equipo debe definir si la fuente oficial será:

```text
dotnet ef database update
```

o:

```text
scripts/sqlserver/schema.sql
```

## 13. Proyecto `src/Domain`

### `Domain.csproj`

Biblioteca .NET 7 reservada para lógica de dominio, reglas de negocio, interfaces y servicios.

Actualmente no contiene clases ni lógica. Las entidades están en `Data` y las reglas están principalmente en los controladores.

## 14. Proyecto `src/Tests`

### `Tests.csproj`

Proyecto xUnit preparado para pruebas automatizadas.

Paquetes:

- `xunit`
- `xunit.runner.visualstudio`
- `Microsoft.NET.Test.Sdk`

Actualmente no contiene archivos de pruebas `.cs`. No hay cobertura automatizada para API, persistencia, frontend ni migraciones.

## 15. Frontend

El frontend es HTML, CSS y JavaScript sin framework.

### `frontend/admin/index.html`

Dashboard principal. Muestra total de cursos, total de categorías, cursos activos y tarjetas de cursos.

Carga `dashboard.js` y `admin.css`.

### `frontend/admin/categorias.html`

Pantalla para crear, listar, actualizar estado y desactivar categorías.

Carga `categorias.js` y `admin.css`.

### `frontend/admin/cursos.html`

Formulario de alta de cursos. Carga categorías, niveles e instructores y envía los datos a la API.

Carga `cursos.js` y `admin.css`.

### `frontend/admin/busqueda.html`

Pantalla de búsqueda con filtros de texto, categoría, nivel, instructor, estado y precio.

Carga `busqueda.js` y `admin.css`.

## 16. JavaScript del frontend

Todos los scripts apuntan actualmente a:

```text
http://localhost:62391/api
```

### `frontend/js/dashboard.js`

Usa:

```http
GET /api/cursos
GET /api/categorias
PUT /api/cursos/{id}
```

Carga estadísticas, renderiza cursos y activa o desactiva cursos.

Tiene funciones para normalizar formatos antiguos de APIs PHP/MongoDB y formatos actuales de ASP.NET Core.

### `frontend/js/categorias.js`

Usa:

```http
GET    /api/categorias
POST   /api/categorias
PUT    /api/categorias/{id}
DELETE /api/categorias/{id}
```

Renderiza la tabla de categorías y administra su creación y estado.

### `frontend/js/cursos.js`

Usa:

```http
GET  /api/categorias
GET  /api/niveles
GET  /api/instructores
POST /api/cursos
```

Payload típico:

```json
{
  "nombre": "Fundamentos de SQL",
  "descripcion": "Curso básico de SQL",
  "precio": 399,
  "duracionMinutos": 240,
  "fechaPublicacion": "2026-05-12",
  "categoriaId": "2",
  "nivel": "Básico",
  "instructor": "Andrea Torres"
}
```

### `frontend/js/busqueda.js`

Carga catálogos y llama:

```http
GET /api/filtro/buscar
```

Construye los parámetros mediante `URLSearchParams` y muestra resultados como tarjetas.

## 17. CSS

### `frontend/css/admin.css`

Estilo principal del panel administrativo. Define colores, sidebar, menú, tarjetas, formularios, botones, tablas, estados y grids.

### `frontend/css/cursos.css`

Contiene un estilo alternativo para la sección de cursos. Actualmente `cursos.html` no lo referencia, por lo que probablemente es código antiguo o no utilizado.

## 18. Configuración de VS Code

Archivo: `.vscode/settings.json`

```json
{
  "liveServer.settings.port": 5502
}
```

Frontend con Live Server:

```text
http://localhost:5502
```

API:

```text
http://localhost:62391
```

La comunicación entre ambos funciona gracias a CORS.

## 19. Flujo completo de ejecución

### Inicio

1. Se inicia SQL Server.
2. Se crea o actualiza `NoSqlU1_migrated`.
3. Se ejecuta la API.
4. ASP.NET Core carga `appsettings.json`.
5. Se registra `NoSqlUContext`.
6. Se habilitan los controladores.
7. Se abre el frontend con Live Server o desde `/admin`.

### Consulta de cursos

1. El navegador ejecuta `fetch('http://localhost:62391/api/cursos')`.
2. `CursosController.GetAll()` recibe la solicitud.
3. EF Core consulta `Cursos`.
4. EF Core incluye categoría, nivel e instructor.
5. SQL Server devuelve los registros.
6. ASP.NET Core serializa el resultado a JSON.
7. JavaScript renderiza las tarjetas.

### Creación de un curso

1. El usuario completa el formulario.
2. JavaScript valida datos básicos.
3. Envía `POST /api/cursos`.
4. El controlador valida nuevamente.
5. Se verifica la categoría.
6. Se busca o crea el nivel.
7. Se busca o crea el instructor.
8. Se crea el curso.
9. EF Core guarda los cambios.
10. Se confirma la transacción.
11. La API devuelve el identificador creado.

### Desactivación de un curso

1. El usuario pulsa desactivar.
2. JavaScript envía `PUT /api/cursos/{id}`.
3. El controlador cambia `estatus` a `inactivo`.
4. El registro permanece en la base.
5. El dashboard lo muestra como inactivo.

## 20. Cómo ejecutar localmente

### Preparar la base con migraciones EF Core

Desde la raíz:

```powershell
dotnet restore
dotnet tool install --global dotnet-ef
dotnet ef database update --project src/Data/Data.csproj --startup-project src/Api/Api.csproj
dotnet run --project src/Api/Api.csproj
```

Si `dotnet-ef` ya está instalado, se puede omitir la instalación.

### Alternativa con el script SQL

Ejecutar `scripts/sqlserver/schema.sql` desde SQL Server Management Studio o una herramienta compatible. Después iniciar la API.

No aplicar ambas estrategias sin revisar previamente el estado de la base y `__EFMigrationsHistory`.

### Verificar la API

```text
http://localhost:62391/api/health
```

Respuesta esperada:

```json
{ "db": "ok" }
```

### Abrir el frontend

Con Live Server:

```text
http://localhost:5502/frontend/admin/index.html
```

También puede intentarse desde la API:

```text
http://localhost:62391/admin/index.html
```

si la ruta física del frontend es encontrada correctamente.

## 21. Riesgos y deuda técnica

### Seguridad

- No existe autenticación ni autorización.
- Todos los endpoints administrativos están abiertos.
- CORS permite cualquier origen.
- `TrustServerCertificate=True` está habilitado.
- Se devuelven detalles de excepción en algunos errores.

### Validación y contratos

- Categorías y niveles tienen validación limitada.
- Algunos endpoints reciben directamente entidades EF Core.
- Hay riesgo de overposting.
- Los contratos HTTP están acoplados a las entidades de persistencia.

### Persistencia

- `schema.sql` y EF Core no generan el mismo esquema.
- Categorías y niveles se borran físicamente.
- Cursos e instructores usan borrado lógico.
- No existe una estrategia única de inicialización documentada como oficial.

### Serialización

Los cursos cargan relaciones y las relaciones tienen colecciones inversas. Esto puede causar ciclos de serialización o respuestas innecesariamente grandes si no se configura el serializador.

### Frontend

Los scripts generan contenido con `innerHTML` usando datos provenientes de la API. Un valor almacenado malicioso podría provocar inyección de HTML o JavaScript.

### Calidad y mantenimiento

- `Domain` está vacío.
- `Tests` no contiene pruebas.
- El workflow `.github/workflows/dotnet.yml` está vacío.
- `frontend/css/cursos.css` parece no utilizarse.
- Hay comentarios y normalizadores heredados de implementaciones anteriores.
- La aplicación está en .NET 7, una versión fuera de soporte actual.

## 22. Resumen para otra IA

SATURNoSQL es un panel administrativo de cursos. El frontend es estático y llama mediante `fetch` a una API ASP.NET Core .NET 7 en `http://localhost:62391`. La API usa controladores, inyecta `NoSqlUContext` y persiste datos en SQL Server mediante EF Core. La base se llama `NoSqlU1_migrated` y usa autenticación integrada de Windows con `Server=localhost`.

Las entidades principales son `Categoria`, `Nivel`, `Instructor` y `Curso`. Cada curso pertenece obligatoriamente a una categoría, un nivel y un instructor. Los cursos se pueden activar o desactivar sin eliminarlos. Los instructores también usan borrado lógico, pero categorías y niveles se eliminan físicamente.

La creación de cursos recibe el nivel y el instructor por ID o nombre. Si se recibe un nombre inexistente, el backend crea automáticamente la entidad correspondiente dentro de una transacción. La búsqueda se realiza en `/api/filtro/buscar` con filtros de texto, relaciones, estado y precio.

La migración EF Core contiene 5 categorías, 3 niveles, 4 instructores y 6 cursos iniciales. `schema.sql` contiene un esquema parecido, pero no equivalente: usa `VARCHAR`, `INT` para duración, `DATE` para publicación y solo inserta 2 categorías y 3 niveles. Elegir una única estrategia de creación de base es importante.

El sistema actualmente no tiene autenticación, pruebas implementadas ni pipeline CI funcional. Las prioridades técnicas más relevantes son unificar el esquema de base, definir contratos DTO, agregar validación y seguridad, corregir serialización potencialmente circular, evitar `innerHTML` inseguro y agregar pruebas.
