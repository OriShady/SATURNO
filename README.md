# SATURNoSQL: guía del sistema

## 1. Qué hace

SATURNoSQL es un sistema administrativo de cursos con una interfaz web estática y una API ASP.NET Core. Permite administrar cursos y sus catálogos, buscar cursos, registrar usuarios con domicilio y credenciales, y asignar usuarios a cursos. Aunque el nombre diga NoSQL, la persistencia actual es relacional: SQL Server con Entity Framework Core (EF Core).

## 2. Arquitectura y organización

```text
Navegador (HTML/CSS/JavaScript)
        | fetch / HTTP + JSON
        v
API ASP.NET Core: controladores y reglas de flujo
        | NoSqlUContext (EF Core)
        v
SQL Server: NoSqlU1_migrated
```

- `frontend/`: páginas administrativas y scripts que consumen la API.
- `src/Api/`: aplicación HTTP, controladores, configuración y Swagger.
- `src/Data/`: entidades, `NoSqlUContext`, mapeo relacional y migraciones EF Core.
- `src/Domain/`: proyecto reservado; actualmente no contiene lógica de dominio.
- `src/Tests/`: proyecto xUnit, sin pruebas implementadas actualmente.

El patrón es cliente-servidor: un frontend separado consume una API HTTP estilo REST implementada con controladores ASP.NET Core. La API usa inyección de dependencias para entregar `NoSqlUContext` a cada controlador. La solución está separada en proyectos, pero no implementa una arquitectura limpia completa ni un patrón Repository/Service propio: los controladores realizan consultas, validaciones y operaciones de negocio directamente. EF Core traduce LINQ a SQL y `SaveChangesAsync` persiste los cambios. No hay vistas MVC en el backend. Al agregar lógica nueva, sigue el patrón existente o acuerda primero moverla a servicios de dominio.

### Mapa de carpetas y archivos

```text
SATURNoSQL/
├── .github/workflows/dotnet.yml       # Workflow de CI; actualmente vacío
├── .vscode/settings.json              # Puerto de Live Server (5502)
├── frontend/
│   ├── admin/                          # Pantallas HTML del panel
│   ├── css/                            # Estilos compartidos y alternativos
│   └── js/                             # Lógica del navegador y llamadas fetch
├── scripts/sqlserver/schema.sql        # Esquema SQL manual, antiguo
├── src/
│   ├── Api/                            # Aplicación HTTP ASP.NET Core
│   │   ├── Controllers/                # Rutas y acciones de la API
│   │   ├── Models/                     # DTOs de entrada/salida
│   │   ├── Properties/launchSettings.json # Perfil local, puertos y entorno
│   │   ├── Program.cs                  # Servicios y pipeline HTTP
│   │   ├── appsettings.json            # Configuración y conexión SQL
│   │   └── Api.csproj                  # Framework, paquetes y referencias
│   ├── Data/                           # Persistencia y modelo relacional
│   │   ├── Entities/                   # Clases que representan tablas
│   │   ├── Migrations/                 # Historial de cambios del esquema
│   │   ├── NoSqlUContext.cs             # DbSets, mapeos, relaciones y semillas
│   │   └── Data.csproj                 # Dependencias EF Core
│   ├── Domain/                         # Reservado; no tiene lógica actualmente
│   └── Tests/                          # Proyecto xUnit sin pruebas actuales
├── NoSQLU1_SQLServer/                  # Directorio legado; no es un proyecto de la solución
├── NoSqlU.sln                          # Agrupa Api, Data, Domain y Tests
├── README_MIGRATION.md                 # Notas antiguas de migración/ejecución
└── CONTEXTO_PROYECTO.md                # Este manual
```

**Frontend.** `frontend/admin/` contiene `index.html` (dashboard), `categorias.html`, `cursos.html`, `busqueda.html`, `registro.html`, `usuarios.html` y `asignacion.html`. Cada HTML define estructura y formularios, enlaza `../css/admin.css` y carga su JavaScript correspondiente. `admin.css` concentra el diseño compartido; `cursos.css` no está enlazado por las páginas actuales. En `frontend/js/`, `dashboard.js` carga indicadores y cursos; `categorias.js` gestiona categorías; `cursos.js` crea cursos y carga catálogos; `busqueda.js` forma filtros; `registro.js` registra usuarios; `usuarios.js` lista/activa/desactiva usuarios y abre asignaciones; `asignacion.js` inscribe a un usuario en un curso.

**API.** `Api.csproj` define una Web API `net7.0`, agrega EF Core SQL Server y Swagger, y referencia `Data` y `Domain`. `Program.cs` registra controladores, Swagger, CORS y `NoSqlUContext`; configura el pipeline y opcionalmente sirve `frontend/admin` bajo `/admin`. `appsettings.json` contiene `DefaultConnection`. `Properties/launchSettings.json` define los puertos locales y `Development`. `Controllers/` contiene `HealthController` (conectividad), `CursosController` (cursos), `CategoriasController`, `NivelesController`, `InstructoresController`, `FiltroController` (búsqueda) y `UsuariosController` (registro, catálogo de estados y asignaciones). `Models/CourseCreateDto.cs` define el contrato para crear cursos; los DTOs de registro y asignación de usuarios están declarados al final de `UsuariosController.cs`.

**Datos y otros proyectos.** `Data/Entities/` contiene `Categoria`, `Curso`, `Nivel`, `Instructor`, `Usuario`, `UsuarioContrasena`, `Sexo`, `Estado`, `DireccionUsuario` y `CursoUsuario`. `Data.csproj` es una biblioteca `net7.0` con EF Core SQL Server/Design y referencia `Domain`. En `Migrations/`, los archivos `.cs` aplican o revierten cambios; los `.Designer.cs` y `NoSqlUContextModelSnapshot.cs` son metadatos generados, no se editan manualmente. `Domain.csproj` existe pero no tiene clases de dominio. `Tests.csproj` configura xUnit y referencia la API, pero actualmente no hay archivos de pruebas. El workflow de GitHub Actions está vacío, así que no hay CI configurada.

**Archivos que no son lógica de negocio.** `bin/` y `obj/` son compilación y archivos generados por .NET; `.vs/` y `.vscode/` guardan estado/configuración del editor; `.git/` contiene historial local; `.gitignore` excluye artefactos y preferencias locales; `.gitattributes` configura finales de línea y comportamiento de Git. No modificar `bin/`, `obj/`, `.vs/` ni archivos EF generados a mano. `src/Api/src/Data/` solo contiene `obj/` generado: la capa Data real está en `src/Data/`. `NoSQLU1_SQLServer/` solo muestra metadatos Git/atributos heredados y no aparece en `NoSqlU.sln`.

## 3. Base de datos

El modelo principal está en `src/Data/NoSqlUContext.cs`. Las entidades se mapean a tablas y columnas SQL explícitas; las relaciones se configuran con claves foráneas y las consultas de EF Core se ejecutan contra SQL Server.

```text
categorias  1 ---- N cursos N ---- 1 niveles
instructores 1 -- N cursos

cat_sexo 1 ---- N usuarios 1 ---- 1 usuario_contrasena
                         |
                         +---- 1 ---- N direcciones_usuario N ---- 1 cat_estados
                         |
                         +---- 1 ---- N cursos_usuarios N ---- 1 cursos
```

- **Cursos:** requieren categoría, nivel e instructor. Incluyen nombre, descripción, precio, duración, publicación y estatus. El precio y la duración deben ser mayores que cero.
- **Catálogos de cursos:** categorías, niveles e instructores tienen nombre único. Un curso conserva referencias a estos registros; el borrado de relacionados está restringido.
- **Usuarios:** guardan datos personales, correo único, teléfono, sexo y estatus. La base exige al menos uno de los apellidos.
- **Credenciales:** `usuario_contrasena` es una relación uno a uno con usuario; el login es único y almacena `password_hash`.
- **Domicilios:** un usuario puede tener varios; cada domicilio referencia un estado. La eliminación del usuario elimina sus credenciales y domicilios en cascada.
- **Inscripciones:** `cursos_usuarios` resuelve la relación muchos-a-muchos entre usuarios y cursos, con fechas y estatus. Sus claves foráneas restringen el borrado de los registros relacionados.
- **Datos iniciales:** EF Core siembra categorías, niveles, instructores y cursos; la migración de usuarios agrega el catálogo de sexos y los estados de México.

Los estatus no están normalizados en una tabla catálogo y el código usa combinaciones de mayúsculas/minúsculas (`activo`/`inactivo` y `ACTIVO`/`INACTIVO`). Respeta los valores esperados por cada controlador y consulta.

## 4. Conexión y ejecución local

La conexión se configura en `src/Api/appsettings.json`, bajo `ConnectionStrings:DefaultConnection`:

```text
Server=localhost;Database=NoSqlU1_migrated;Trusted_Connection=True;
TrustServerCertificate=True;MultipleActiveResultSets=True
```

Usa autenticación integrada de Windows. Si falta `DefaultConnection`, `Program.cs` utiliza LocalDB (`(localdb)\\mssqllocaldb`). La API registra el contexto con `UseSqlServer`; no aplica migraciones automáticamente.

Desde la raíz, con SQL Server disponible y `dotnet-ef` instalado (si hace falta: `dotnet tool install --global dotnet-ef`):

```powershell
dotnet restore
dotnet ef database update --project src/Data/Data.csproj --startup-project src/Api/Api.csproj
dotnet run --project src/Api/Api.csproj
```

Cuando cambie el modelo, crea y revisa primero la migración: `dotnet ef migrations add NombreMigracion --project src/Data/Data.csproj --startup-project src/Api/Api.csproj`; después aplica `database update`.

- API HTTP: `http://localhost:62391`; HTTPS: `https://localhost:62390`.
- Comprobación de API y base: `http://localhost:62391/api/health` (esperado: `{"db":"ok"}`).
- Frontend con Live Server: `http://localhost:5502/frontend/admin/index.html`.
- También puede servirse desde la API en `/admin` si existe la ruta física `frontend/admin` relativa al proyecto.

Usa preferentemente migraciones EF Core como fuente oficial del esquema. No ejecutes a ciegas `schema.sql`: es una alternativa antigua que solo crea las tablas de cursos y catálogos, no incluye usuarios y difiere de EF Core en tipos, fechas y datos iniciales. Además, `AgregandoUsuarios` contiene columnas/relaciones que no aparecen en el modelo actual; compara el esquema y la migración antes de actualizar una base existente.

## 5. Flujos y API principales

Todos los endpoints usan el prefijo `/api`.

| Área | Endpoints relevantes | Comportamiento |
|---|---|---|
| Salud | `GET /health` | Comprueba si la base acepta conexiones. |
| Cursos | `GET/POST /cursos`, `GET/PUT/DELETE /cursos/{id}` | Lista y crea cursos; `PUT` cambia estatus; `DELETE` desactiva, no borra. Al crear, se puede proporcionar nivel/instructor por ID o nombre; por nombre se reutilizan o crean. |
| Búsqueda | `GET /filtro/buscar` | Filtra por texto, categoría, nivel, instructor, estatus y rango de precio. |
| Catálogos | `/categorias`, `/niveles`, `/instructores` | CRUD según el controlador. Categorías e instructores se desactivan; niveles se borran físicamente. |
| Usuarios | `GET /usuarios`, `GET /usuarios/estados`, `POST /usuarios/registro`, `PUT /usuarios/{id}/estatus` | Consulta usuarios/estados, registra usuario con credenciales y domicilio, y alterna su estatus. |
| Asignaciones | `GET /usuarios/cursos-disponibles`, `POST /usuarios/asignar-curso` | Lista cursos activos y registra la inscripción; la API rechaza una asignación duplicada. |

Flujo típico: el JavaScript envía JSON con `fetch`; el controlador valida y usa `NoSqlUContext`; EF Core consulta o persiste; la API devuelve JSON y el frontend actualiza la pantalla. Los scripts cliente apuntan a `http://localhost:62391/api`.

### Cómo seguir los flujos principales

- **Registro:** `admin/registro.html` → `js/registro.js` carga `/usuarios/estados` y envía el formulario a `POST /usuarios/registro` → `UsuariosController` arma `Usuario`, `UsuarioContrasena` y `DireccionUsuario` → EF Core persiste las entidades relacionadas.
- **Alta de curso:** `admin/cursos.html` → `js/cursos.js` carga categorías, niveles e instructores → `POST /cursos` con `CourseCreateDto` → `CursosController` valida categoría/precio/duración y puede buscar o crear nivel e instructor por nombre → guarda el curso dentro de una transacción.
- **Asignación:** `admin/usuarios.html` → `js/usuarios.js` pasa el ID a `asignacion.html` → `js/asignacion.js` carga cursos activos y envía `POST /usuarios/asignar-curso` → `UsuariosController` evita duplicar la inscripción y crea `CursoUsuario`.
- **Búsqueda:** `admin/busqueda.html` → `js/busqueda.js` compone parámetros con `URLSearchParams` → `GET /filtro/buscar` → `FiltroController` agrega condiciones LINQ → EF Core las traduce a SQL y devuelve resultados con relaciones.

Para aprender o depurar una función, sigue siempre este camino: **HTML → JavaScript → ruta HTTP → acción del controller → entidad/DbContext → tabla y migración**. Si cambia una columna o relación, revisa el modelo de Data y genera una migración; si cambia una regla o endpoint, revisa también el DTO, el controller y el consumidor JavaScript.

## 6. Mantenimiento y riesgos

- **Contraseñas:** el registro aplica SHA-256 directo. No es adecuado para almacenar contraseñas; antes de usar cuentas reales se requiere un algoritmo dedicado para passwords, con salt y factor de costo.
- **Acceso:** no hay autenticación/autorización aunque se registren credenciales; CORS permite cualquier origen. No exponer la API tal cual en producción.
- **Esquema:** usa preferentemente migraciones EF Core como fuente oficial. `schema.sql` es antiguo, no incluye usuarios y difiere de EF Core. `AgregandoUsuarios` contiene columnas/relaciones que no aparecen en el modelo actual; compara antes de actualizar una base existente.
- **Borrado y estatus:** cursos, instructores, usuarios y categorías se desactivan; niveles se eliminan físicamente. Respeta mayúsculas/minúsculas del código (`activo`/`inactivo` frente a `ACTIVO`/`INACTIVO`).
- **Frontend:** algunos scripts construyen HTML con datos de la API mediante `innerHTML`, lo que requiere cuidado para evitar inyección.
- **Calidad/versión:** los proyectos apuntan a .NET 7; no hay pruebas automatizadas ni CI activa. `README_MIGRATION.md` conserva instrucciones antiguas (por ejemplo, puerto 8080 en vez del 5502 configurado actualmente).