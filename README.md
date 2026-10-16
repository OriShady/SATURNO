# SATURNoSQL: guía del sistema

## 1. Qué hace

SATURNoSQL es un sistema administrativo de empleados implementado con ASP.NET Core MVC y SQL Server. La pantalla principal permite buscar, filtrar, paginar, registrar, editar y dar de baja empleados; el alta guiada captura sus datos, domicilio y los cursos de sus certificados. Los catálogos de estados, sexos y demás opciones también se administran desde el panel. La API JSON existente se conserva para clientes externos.

## 2. Arquitectura y organización

```text
Navegador (HTML/CSS, formularios HTTP)
        v
Controlador MVC (C#) <-> Vista Razor (HTML)
        |
        v
Modelo de datos (entidades + NoSqlUContext / EF Core)
        v
SQL Server: NoSqlU1_migrated
```

- `src/Api/Controllers/Mvc/`: acciones MVC y flujo de pantallas.
- `src/Api/Models/ViewModels/`: modelos de pantalla, validaciones y datos de formularios.
- `src/Api/Views/`: vistas Razor, layout y flujos progresivos con JavaScript ligero para búsqueda y diálogos.
- `src/Api/Controllers/`: endpoints JSON de la API, conservados por compatibilidad.
- `src/Data/Entities/`: modelos persistentes (entidades SQL), `NoSqlUContext`, mapeos y migraciones.
- `frontend/css/`: hoja de estilos compartida.
- `src/Domain/`: proyecto reservado; actualmente no contiene lógica de dominio.
- `src/Tests/`: proyecto xUnit, sin pruebas implementadas actualmente.

La aplicación web sigue MVC: los controladores consultan/actualizan los modelos de `Data` mediante `NoSqlUContext`, y renderizan vistas Razor desde `Views/Admin`. Los formularios POST validan los datos en el servidor. EF Core traduce LINQ a SQL y `SaveChangesAsync` persiste los cambios.

### Mapa de carpetas y archivos

```text
SATURNoSQL/
├── .github/workflows/dotnet.yml       # Workflow de CI; actualmente vacío
├── .vscode/settings.json              # Puerto de Live Server (5502)
├── frontend/
│   └── css/                            # Estilos compartidos del panel
├── scripts/sqlserver/schema.sql        # Esquema SQL manual, antiguo
├── src/
│   ├── Api/                            # Aplicación HTTP ASP.NET Core
│   │   ├── Controllers/                # API JSON y controladores MVC
│   │   │   └── Mvc/AdminController.cs  # Acciones de la interfaz
│   │   ├── Models/                     # DTOs y ViewModels MVC
│   │   ├── Views/                      # Vistas Razor y layout
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

**Interfaz MVC.** `Views/Shared/_Layout.cshtml` comparte el menú y los estilos; `Views/Admin/Empleados.cshtml` concentra el directorio y el alta progresiva, `EditarEmpleado.cshtml` edita su ficha y `Catalogos.cshtml` administra los catálogos. Las pantallas heredadas de cursos siguen disponibles para compatibilidad, pero ya no forman parte de la navegación principal.

**Aplicación/API.** `Api.csproj` define la aplicación web `net7.0`, agrega EF Core SQL Server y Swagger, y referencia `Data` y `Domain`. `Program.cs` registra MVC, Swagger y `NoSqlUContext`, sirve los estilos CSS y configura la ruta por defecto `/` hacia `Admin/Empleados`. `DefaultConnection` se carga desde User Secrets en desarrollo o desde la variable de entorno correspondiente en despliegues; `Properties/launchSettings.json` define los puertos locales y `Development`. Los controladores existentes bajo `Controllers/` conservan endpoints JSON; `Controllers/Mvc/AdminController.cs` atiende la interfaz MVC.

**Datos y otros proyectos.** `Data/Entities/` contiene el modelo relacional: `Categoria`, `Curso`, `Nivel`, `Instructor`, `Usuario`, `UsuarioContrasena`, `Sexo`, `Estado`, `DireccionUsuario` y `CursoUsuario`. `Data.csproj` es una biblioteca `net7.0` con EF Core SQL Server/Design y referencia `Domain`. En `Migrations/`, los archivos `.cs` aplican o revierten cambios; los `.Designer.cs` y `NoSqlUContextModelSnapshot.cs` son metadatos generados, no se editan manualmente. `Domain.csproj` existe pero no tiene clases de dominio. `Tests.csproj` configura xUnit y referencia la API, pero actualmente no hay archivos de pruebas. El workflow de GitHub Actions está vacío, así que no hay CI configurada.

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
                         +---- 1 ---- N cursos_usuarios ---- 0..1 cursos (referencia heredada)
```

- **Cursos:** requieren categoría, nivel e instructor. Incluyen nombre, descripción, precio, duración, publicación y estatus. El precio y la duración deben ser mayores que cero.
- **Catálogos de cursos:** categorías, niveles e instructores tienen nombre único. Un curso conserva referencias a estos registros; el borrado de relacionados está restringido.
- **Empleados:** guardan datos personales, correo único, teléfono, fecha de ingreso, sexo y estatus. La baja desactiva al empleado para preservar sus datos e historial.
- **Credenciales:** `usuario_contrasena` es una relación uno a uno con usuario; el login es único y almacena `password_hash`.
- **Domicilios:** un usuario puede tener varios; cada domicilio referencia un estado. La eliminación del usuario elimina sus credenciales y domicilios en cascada.
- **Cursos de certificados:** `cursos_usuarios` guarda nombre del curso, institución, fecha de finalización y estatus. `id_curso` es opcional para conservar compatibilidad con registros heredados; `nombre_curso` copia el nombre de curso existente en la migración.
- **Datos iniciales:** EF Core siembra categorías, niveles, instructores y cursos; la migración de usuarios agrega el catálogo de sexos y los estados de México.

Los estatus no están normalizados en una tabla catálogo y el código usa combinaciones de mayúsculas/minúsculas (`activo`/`inactivo` y `ACTIVO`/`INACTIVO`). Respeta los valores esperados por cada controlador y consulta.

## 4. Conexión y ejecución local

La cadena de conexión no está guardada en el repositorio. Configúrala localmente con User Secrets:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=NoSqlU1_migrated;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;Application Name=NoSqlU;" --project src/Api/Api.csproj
```

Usa autenticación integrada de Windows en ese ejemplo. Para otra instancia, cambia el servidor/base por los valores de tu entorno. También se puede proporcionar mediante la variable de entorno `ConnectionStrings__DefaultConnection`. Si no se configura, la aplicación falla al arrancar con un mensaje claro en vez de intentar una conexión predeterminada. La API no aplica migraciones automáticamente.

Desde la raíz, con SQL Server disponible y `dotnet-ef` instalado (si hace falta: `dotnet tool install --global dotnet-ef`):

```powershell
dotnet restore
dotnet ef database update --project src/Data/Data.csproj --startup-project src/Api/Api.csproj
dotnet run --project src/Api/Api.csproj
```

Cuando cambie el modelo, crea y revisa primero la migración: `dotnet ef migrations add NombreMigracion --project src/Data/Data.csproj --startup-project src/Api/Api.csproj`; después aplica `database update`.

Las migraciones nuevas alinean `duracion_minutos` con SQL Server `int` y eliminan de forma condicional dos columnas de relación sombra que no se usan (`usuarios.EstadoId` y `direcciones_usuario.SexoId`), sin fallar si esas columnas no existen. Antes de desplegar en otra base, revisa y aplica las migraciones pendientes con `dotnet ef database update`.

- API HTTP: `http://localhost:62391`; HTTPS: `https://localhost:62390`.
- Comprobación de API y base: `http://localhost:62391/api/health` (esperado: `{"db":"ok"}`).
- Interfaz MVC: `http://localhost:62391/` (también se abre al iniciar el perfil local).

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

Flujo MVC: el navegador envía el formulario; el controlador valida el ViewModel, consulta o persiste mediante `NoSqlUContext`, y devuelve una vista Razor o redirige tras guardar. Los endpoints JSON `/api/...` se mantienen disponibles para clientes externos.

### Cómo seguir los flujos principales

- **Registro:** `Admin/Registro` muestra el formulario; `Admin/Registrar` valida y crea `Usuario`, `UsuarioContrasena` y `DireccionUsuario`.
- **Alta de curso:** `Admin/Cursos` entrega catálogos; `Admin/CrearCurso` valida el ViewModel y crea o reutiliza nivel e instructor.
- **Asignación:** `Admin/Usuarios` enlaza a `Admin/AsignarCurso`; el POST valida que el curso esté activo y no exista ya la inscripción antes de crear `CursoUsuario`.
- **Búsqueda:** `Admin/Buscar` aplica los filtros enviados por GET a una consulta LINQ y presenta resultados en Razor.

Para aprender o depurar una función, sigue este camino: **Vista Razor → acción del controller → ViewModel/entidad → DbContext → tabla y migración**. Si cambia una columna o relación, revisa el modelo de Data y genera una migración; para la API JSON, conserva también actualizado su DTO.

## 6. Mantenimiento y riesgos

- **Contraseñas:** el registro aplica SHA-256 directo. No es adecuado para almacenar contraseñas; antes de usar cuentas reales se requiere un algoritmo dedicado para passwords, con salt y factor de costo.
- **Acceso:** no hay autenticación/autorización aunque se registren credenciales; CORS permite cualquier origen. No exponer la API tal cual en producción.
- **Esquema:** usa preferentemente migraciones EF Core como fuente oficial. `schema.sql` es antiguo, no incluye usuarios y difiere de EF Core. `AgregandoUsuarios` contiene columnas/relaciones que no aparecen en el modelo actual; compara antes de actualizar una base existente.
- **Borrado y estatus:** cursos, instructores, usuarios y categorías se desactivan; niveles se eliminan físicamente. Respeta mayúsculas/minúsculas del código (`activo`/`inactivo` frente a `ACTIVO`/`INACTIVO`).
- **Calidad/versión:** los proyectos apuntan a .NET 7; no hay pruebas automatizadas ni CI activa. `README_MIGRATION.md` contiene instrucciones generales de ejecución local.