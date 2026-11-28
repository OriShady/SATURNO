# SATURNO explicado paso a paso: guía para entender y defender el proyecto

Esta guía está escrita como mapa para estudiar el código sin tener que aprender todos los términos de golpe. Describe la rama `MongoUpdates` revisada el 26 de noviembre de 2026. Si el código cambia, vuelve a comprobar las rutas y el comportamiento antes de usarla en una revisión.

## 1. La idea en una frase

SATURNO es una aplicación web para administrar empleados, sus domicilios y cursos/certificados, además de algunos catálogos. La pantalla está hecha con ASP.NET Core MVC; los datos se guardan en MongoDB.

Piensa en una escuela:

- **La vista** es el formulario que ve la persona.
- **El controlador** es quien recibe la hoja, revisa los datos y decide qué hacer.
- **El ViewModel** es la hoja ordenada que viaja entre la pantalla y el controlador.
- **La entidad** es la forma en que el programa representa un dato guardado.
- **MongoDB** es el archivero donde quedan guardados los documentos.
- **`init.js`** prepara los cajones del archivero, sus reglas, índices y algunos catálogos de ejemplo. No es quien registra los empleados que tú capturas.

El camino normal es:

```text
Persona usa la pantalla
  -> Vista Razor (.cshtml) manda una solicitud HTTP
  -> Controlador C# recibe la solicitud
  -> ViewModel recibe los campos del formulario
  -> Controlador valida y usa SaturnoStore
  -> MongoCatalog / MongoDB Driver habla con MongoDB
  -> colección de MongoDB guarda o devuelve documentos
  -> controlador devuelve otra vista o una respuesta JSON
```

La aplicación tiene dos puertas de entrada:

1. **MVC:** devuelve páginas HTML. Es lo que usa una persona en el navegador.
2. **API:** devuelve datos JSON. Puede ser usada por otro programa, Swagger o un cliente HTTP.

## 2. Primero: no confundas los tres diseños

En los materiales disponibles aparecen tres cosas relacionadas, pero no idénticas:

| Material | Qué representa |
|---|---|
| `Sturnotes.txt` | Notas sobre un diseño **relacional SQL**: tablas, PK/FK, un domicilio por empleado y `NO ACTION`. |
| `Script-Saturno.txt` adjunto | Un **script Mongo anterior**: usa `ObjectId`, un campo `domicilio` singular y un modelo de cursos diferente. |
| `backend/scripts/mongodb/init.js` | El inicializador que usa la aplicación actual en `MongoUpdates`: IDs enteros para la mayoría de catálogos/empleados, documentos con nombres BSON concretos, domicilios embebidos en un array y certificados en otro array. |

No digas que los tres son el mismo esquema. El código C# de esta rama tiene que coincidir con `backend/scripts/mongodb/init.js`, no automáticamente con un archivo Mongo anterior o con el diseño SQL.

### Diferencias concretas que sí pueden preguntarte

| Tema | Diseño SQL de las notas | Script Mongo adjunto anterior | Código actual + `init.js` |
|---|---|---|---|
| ID empleado | Clave primaria SQL | `ObjectId` | Número entero guardado como `_id`, asignado con contador |
| ID sexo | Clave/relación SQL | `ObjectId` en referencia | Una letra como `"M"`, `"F"` o `"X"` |
| Domicilio | Tabla aparte; nota limita a uno por empleado | Objeto `domicilio` | Array `domicilios` dentro del empleado; el formulario actual opera sobre el primero |
| Cursos del empleado | Tabla `cursos_empleado` y restricción única descrita en las notas | Array con `id_curso` obligatorio | Array `cursos`; `id_curso` puede ser nulo y también se guarda el nombre del curso |
| Catálogo de cursos | Las notas dicen que no se usa el catálogo general | Colección `cursos` sencilla | Colecciones `categorias`, `niveles`, `instructores` y `cursos` |
| Integridad | Restricciones/foreign keys de SQL | Referencias `ObjectId` declaradas en documentos | IDs entre colecciones; Mongo no los trata como foreign keys |

**Respuesta honesta en revisión:** “La implementación vigente está alineada con `init.js`. El script Mongo adjunto anterior y las notas SQL representan versiones/modelos distintos. Si la consigna vigente es exactamente la del diseño SQL, todavía hay que acordar y corregir esas diferencias; no voy a afirmar que Mongo hace cumplir una FK que no existe.”

No ejecutes el script Mongo adjunto anterior y `init.js` sobre una misma base esperando que sean compatibles. Aunque usen nombres de colección parecidos, los tipos de ID y la forma de los campos no coinciden.

## 3. Qué es MongoDB en palabras sencillas

MongoDB organiza información en:

- **Base de datos:** el archivador completo. La configuración actual usa `SaturnoMongo`.
- **Colección:** un cajón para documentos parecidos, por ejemplo `empleados` o `cursos`.
- **Documento:** una ficha parecida a un JSON/BSON.
- **Campo:** un dato dentro de una ficha, por ejemplo `nombre` o `estatus`.
- **Índice:** una guía para encontrar datos más rápido. Si es `unique`, además evita duplicados en ese campo o combinación.
- **Validador:** una regla que Mongo comprueba cuando se intenta guardar un documento.

Ejemplo simplificado de un empleado actual:

```json
{
  "_id": 27,
  "nombre": "Lucía",
  "apellido_paterno": "Pérez",
  "apellido_materno": null,
  "id_sexo": "F",
  "correo_institucional": "lucia@example.com",
  "estatus": "ACTIVO",
  "domicilios": [
    {
      "calle": "Av. Central",
      "numero_exterior": "15",
      "codigo_postal": "12345",
      "id_estado": 9
    }
  ],
  "cursos": []
}
```

Es un dibujo educativo, no una instrucción para copiar un empleado a la base. El código C# usa atributos como `[BsonElement("nombre")]` para traducir nombres de propiedades (`Nombres`) a nombres BSON (`nombre`).

## 4. Cómo está dividido el proyecto

### Frontend: lo que ve la persona

No es una aplicación React separada. El navegador recibe páginas Razor `.cshtml`, que mezclan HTML con expresiones C# para mostrar los datos que entregó el controlador. JavaScript pequeño controla algunos pasos del formulario y diálogos; CSS da formato.

Archivos importantes:

- `frontend/Views/Shared/_Layout.cshtml`: marco común, menú lateral y botón Salir.
- `frontend/Views/Admin/Empleados.cshtml`: directorio, filtros, tabla, paginación y diálogo de baja definitiva.
- `frontend/Views/Admin/FormularioEmpleado.cshtml`: alta/edición por pasos; aquí están los combos de Sexo y Estado.
- `frontend/Views/Admin/Catalogos.cshtml`: lista y formulario modal de sexo/estado.
- `frontend/Views/Admin/Cursos.cshtml`: formulario para crear cursos.
- `frontend/Views/Admin/Buscar.cshtml`: filtros y resultados de cursos.
- `frontend/Views/Admin/AsignarCurso.cshtml`: combo de cursos para asignar a empleado.
- `frontend/css/admin.css`: estilos que se sirven como `/css/admin.css`.
- `frontend/Views/Sesion/Entrar.cshtml`: pantalla de inicio de sesión.

### Backend: lo que ocurre detrás

- `backend/Api/Program.cs`: configura ASP.NET, MongoDB, autenticación, permisos, Swagger y rutas.
- `backend/Api/Controllers/Mvc/AdminController.cs`: acciones de las pantallas; registra/edita empleados, carga listas y procesa catálogos.
- `backend/Api/Controllers/Mvc/SesionController.cs`: inicia/cierra sesión del administrador.
- `backend/Api/Controllers/UsuariosController.cs`: endpoints JSON para empleados, registro y asignación de cursos.
- `backend/Api/Controllers/CategoriasController.cs`, `NivelesController.cs`, `InstructoresController.cs`, `CursosController.cs`: API JSON de catálogos y cursos.
- `backend/Api/Controllers/HealthController.cs`: comprueba que Mongo responde y que hay catálogos básicos.
- `backend/Api/Models/ViewModels/AdminViewModels.cs`: hojas/formas de pantalla y varias reglas de validación.
- `backend/Api/Models/Validation/DateValidationAttributes.cs`: validaciones especiales de edad y orden de fechas.

### Datos

- `backend/Api/Models/Data/Entities/`: clases C# que representan documentos o subdocumentos.
- `backend/Api/Models/Data/SaturnoStore.cs`: acceso Mongo de operaciones específicas del negocio.
- `backend/Api/Models/Data/MongoCatalog.cs`: operaciones genéricas para consultar, buscar, insertar, reemplazar o borrar una colección.
- `backend/scripts/mongodb/init.js`: reglas, índices y datos iniciales compatibles con esta versión.

## 5. ¿Dónde está cada combo?

Un `<select>` es un menú desplegable real. Un `<datalist>` se parece a un combo/autocompletado, pero permite escribir texto libre.

| Campo | Archivo y propiedad/código | Quién le carga opciones |
|---|---|---|
| Sexo en alta/edición del empleado | `FormularioEmpleado.cshtml`, `<select asp-for="SexoId">` | `BuildEmpleadoFormViewModelAsync` en `AdminController` carga `model.Sexos` desde colección `sexos`. |
| Estado en alta/edición | `FormularioEmpleado.cshtml`, `<select asp-for="EstadoId">` | El mismo método carga estados activos y conserva el seleccionado al editar. |
| Sexo en formulario legado de registro | `Registro.cshtml`, `<select asp-for="SexoId">` | `BuildRegistroViewModelAsync`; la ruta GET `Registro` actualmente redirige al directorio, así que el flujo principal usa el formulario progresivo de empleado. |
| Estado en formulario legado | `Registro.cshtml`, `<select asp-for="EstadoId">` | `BuildRegistroViewModelAsync`. |
| Categoría al crear curso | `Cursos.cshtml`, `<select asp-for="CategoriaId">` | `BuildCursoViewModelAsync` carga categorías activas. |
| Nivel al crear curso | `Cursos.cshtml`, `<input list="niveles">` | `BuildCursoViewModelAsync`; es un `datalist`, no un `<select>` cerrado. |
| Instructor al crear curso | `Cursos.cshtml`, `<input list="instructores">` | `BuildCursoViewModelAsync`; también es `datalist`. |
| Filtros de cursos | `Buscar.cshtml`, selects de categoría/nivel/instructor/estatus | La acción `Buscar` llena catálogos activos y opciones de estatus escritas en la propia vista. |
| Asignar curso a empleado | `AsignarCurso.cshtml`, `<select asp-for="CursoId">` | `BuildAsignarCursoViewModelAsync` carga cursos activos. |
| Estatus en directorio | `Empleados.cshtml`, `<select name="estatus">` | Las dos opciones (`ACTIVO`, `INACTIVO`) están en la vista. |
| Cambiar sexo/estado del catálogo | `Catalogos.cshtml`, pestañas y modal | El modal guarda un sexo o estado; no es el combo del empleado. |

### El viaje de un combo: ejemplo Sexo

1. El documento está en `db.sexos`, por ejemplo `{ _id: "F", nombre: "Femenino", estatus: "ACTIVO" }`.
2. `Sexo.cs` representa ese documento: `Id`, `Descripcion`, `Estatus`.
3. `AdminController.BuildEmpleadoFormViewModelAsync` consulta los sexos activos.
4. Los pone en `EmpleadoFormViewModel.Sexos`.
5. `FormularioEmpleado.cshtml` recorre `Model.Sexos` y genera una opción por cada sexo.
6. El navegador envía el valor seleccionado como `SexoId`.
7. El POST `RegistrarEmpleado` o `EditarEmpleado` valida y copia ese valor al empleado.
8. `[BsonRepresentation(BsonType.String)]` permite guardar el carácter como texto en `id_sexo`.

Por eso el combo no “guarda” nada por sí mismo: solo permite escoger; el backend decide si el valor es válido y Mongo guarda el documento final.

## 6. Flujo completo de alta de empleado

Este es el recorrido importante que puedes explicar en voz alta:

1. El usuario abre `/Admin/RegistrarEmpleado`.
2. `AdminController.RegistrarEmpleado()` GET llama a `BuildEmpleadoFormViewModelAsync()`.
3. El ViewModel recibe estados y sexos activos para dibujar los combos.
4. `FormularioEmpleado.cshtml` muestra tres secciones: datos personales, domicilio e historial de cursos.
5. El usuario rellena la pantalla y pulsa Guardar empleado. El formulario envía un POST a `RegistrarEmpleado`.
6. ASP.NET vincula los campos enviados a un `EmpleadoFormViewModel`.
7. Data Annotations comprueba campos básicos; `ValidarEmpleadoAsync` comprueba correo duplicado, sexo/estado válido y reglas de cursos.
8. Si hay errores, se devuelve el formulario con mensajes; no se inserta el empleado.
9. Si está válido, `AplicarFormulario` copia los datos al objeto `Usuario`, su domicilio y los cursos.
10. `_db.Usuarios.InsertAsync(usuario)` llama a `MongoCatalog.InsertAsync`.
11. `MongoCatalog` pide un ID a `SaturnoStore.NextIdAsync`, lo asigna y el driver inserta el documento en `empleados`.
12. El controlador redirige al directorio buscando por el número recién asignado.

### Alta y edición no son idénticas

- En alta, el formulario comienza sin empleado; al guardar crea uno.
- En edición, GET busca el documento por ID y rellena el mismo formulario.
- En la edición actual, el formulario toma `Direcciones.FirstOrDefault()` y actualiza esa primera dirección. Aunque Mongo admite un array de domicilios, el formulario no administra varias direcciones.
- `EditarEmpleadoAsync` de `SaturnoStore` actualiza campos del empleado y reemplaza las listas de domicilio/cursos enviadas por el flujo de edición.
- El número de empleado es la clave; no se debe cambiar al editar.

## 7. El formulario progresivo: por qué hay JavaScript

En `FormularioEmpleado.cshtml`:

- Los botones `data-step="0"`, `"1"` y `"2"` abren Personal, Domicilio y Cursos.
- Cada bloque usa `data-panel`.
- `show(index)` muestra solo una sección y actualiza cuál pestaña está activa.
- `go(index)` impide avanzar si la sección actual tiene campos inválidos.
- `add-course` copia la plantilla HTML `course-row` para agregar una fila.
- El botón `data-quitar` borra una fila y vuelve a numerar los campos para que ASP.NET pueda recibir una lista.
- `syncDateLimits()` hace que la fecha de ingreso no sea anterior al nacimiento, y que la entrega de constancia no sea anterior al inicio del curso.
- `form.noValidate = true` permite que el propio script controle cuándo y cómo mostrar la validación del navegador. La validación del backend sigue siendo necesaria; JavaScript se puede omitir o manipular.

## 8. Qué hace cada parte de `init.js`

### A. Reglas que ayudan a construir validadores

- `txt(...)`: regla para texto y longitudes.
- `optional(...)`: permite `null` además del tipo normal.
- `intId`: entero positivo para IDs.
- `date`: fecha BSON.
- `email`, `phone`: patrones sencillos para correo y celular.
- `estatus` / `estatusCurso`: valores permitidos para los campos de estatus.
- `object(...)`: construye el validador para un objeto con campos requeridos.

Son funciones JavaScript que ayudan a no repetir las mismas reglas en todos los documentos.

### B. Formas de documentos internos

- `domicilio`: estructura de cada elemento dentro del array `domicilios`.
- `certificado`: estructura de cada curso/constancia dentro del array `cursos`.
- `empleado`: campos personales, arrays, credencial opcional y regla de apellidos.

`anyOf` en empleado expresa que debe existir al menos uno de los dos apellidos. El `anyOf` de certificado permite una situación en curso o un documento que ya tiene fecha de finalización.

### C. Validadores de colecciones

`validators` relaciona nombre de colección con reglas:

- `empleados`
- `administradores`
- `sexos`
- `estados`
- `categorias`
- `niveles`
- `instructores`
- `cursos`

El ciclo que sigue consulta las colecciones existentes. Si existe una, `collMod` cambia su validador; si no existe, `createCollection` la crea con validador. `validationLevel: "strict"` aplica la regla a escrituras; `validationAction: "error"` rechaza datos no válidos.

**Importante:** el validador impone forma y tipos, pero no comprueba que `id_estado: 9` corresponda a un documento existente en `estados`. Esa sería una relación entre colecciones, no una validación de tipo.

### D. Índices

- Índices únicos sobre nombres de sexo/estado/categorías/niveles/instructores con collation española y `strength: 2` (no distingue mayúsculas/minúsculas, pero sí suele distinguir acentos).
- Índice único para correo del empleado.
- Índice único parcial para nombre de usuario de credencial: solo considera documentos donde ese campo sea texto.
- Índices normales para estatus/ID, cursos relacionados, sexo y estado de domicilio.

Los índices normales ayudan a buscar. Los únicos, además, rechazan duplicados. Ningún índice de esos crea una foreign key.

### E. Función `seed`

`seed(coleccion, documentos)` recorre documentos y hace `updateOne` con `upsert: true` y `$setOnInsert`.

En castellano:

- si encuentra el `_id`, no reemplaza sus datos;
- si no lo encuentra, crea ese registro.

El inicializador usa `seed` para catálogos y cursos de ejemplo. **No siembra empleados.** Los empleados que aparecen deben haber sido registrados desde el sistema, o haber sido cargados por otro proceso.

La línea adicional `db.sexos.updateOne({ _id: "X" }, { $set: { nombre: "Otro" } })` sí cambia el nombre del registro `X` ya existente. Se dejó para que el catálogo antiguo “No binario / Otro” se vea como “Otro” sin cambiar su clave.

### F. Lista de estados y datos iniciales

Los arrays de estados, niveles, categorías, instructores y cursos sirven para preparar una base inicial de práctica. No contienen tu empleado personal.

### G. Contadores

El ciclo final busca el `_id` más alto de las colecciones que usan números y hace `$max` en `contadores`. Así el contador queda como mínimo en el máximo documento ya existente.

**Una consecuencia útil:** si el ID más grande de empleados es 40, la próxima inserción debe reservar 41 o más, no volver a 1.

### H. Lo que `init.js` no hace

- No inicia el servicio MongoDB.
- No ejecuta la aplicación ASP.NET.
- No importa filas desde SQL Server.
- No crea empleados falsos.
- No reemplaza cada registro del catálogo al volver a ejecutarse.
- No convierte automáticamente el script Mongo anterior al esquema actual.

## 9. Relaciones: cómo explicarlas sin decir “foreign key” de más

MongoDB deja guardar datos juntos o guardarlos aparte y referenciarlos por ID. En SATURNO se usan las dos ideas:

### Datos embebidos (guardados dentro del documento de empleado)

`Usuario` tiene:

- `Direcciones`: array de `DireccionUsuario`, guardado como `domicilios`.
- `CursosUsuarios`: array de `CursoUsuario`, guardado como `cursos`.
- `Contrasena`: credencial opcional embebida como `credencial` en el registro creado por el endpoint de API heredado.

Ventaja: leer el empleado también trae su domicilio/historial embebido. No hay que buscar otra colección para esas piezas.

Límite: no son filas hijas protegidas por una FK. Si se elimina el documento empleado, esos arrays desaparecen con él. Además, el formulario MVC actual solo edita el primer domicilio.

### Referencias entre colecciones

- `Usuario.SexoId` guarda código como `"F"` y se relaciona por convención con `_id` en `sexos`.
- Cada domicilio guarda `EstadoId`, que debería corresponder a `_id` de `estados`.
- Un curso de catálogo guarda `CategoriaId`, `NivelId`, `InstructorId`.
- Un certificado puede conservar `CursoId`, opcionalmente, y también copia el nombre/duración.

El código puede buscar el documento relacionado para mostrar su nombre. Por ejemplo, `SaturnoStore.BuscarDetalleEmpleadoAsync` carga sexo y estados por separado; `CargarCursosAsync` carga curso/categoría/nivel/instructor y los asigna a propiedades marcadas `[BsonIgnore]` para mostrar el detalle.

### ¿MongoDB garantiza que exista una referencia?

No automáticamente. Mongo almacena el número o la letra; no impone que ese valor exista en otro cajón.

El código comprueba relaciones en algunos flujos, pero no de forma perfecta o igual en todos. Por ejemplo, `ValidarEmpleadoAsync` del flujo MVC exige sexo y estado activos para una selección nueva; `UsuariosController.Registrar` comprueba existencia, pero no revisa si están activos. Es una diferencia real que se debe reconocer y se puede corregir.

## 10. Cómo se organizan los IDs y la concurrencia

1. Hay un documento de contador por colección, por ejemplo `{ "_id": "empleados", "valor": 27 }`.
2. Cuando una inserción usa `MongoCatalog.InsertAsync`, llama a `SaturnoStore.NextIdAsync`.
3. `NextIdAsync` ejecuta `$inc: { valor: 1 }` con `FindOneAndUpdate` y pide el valor actualizado.
4. MongoDB hace atómico ese incremento sobre el documento. Dos solicitudes concurrentes al mismo contador reciben números distintos.
5. Ese valor se asigna a `Usuario.Id` y el driver lo serializa en Mongo como `_id`.

Frase para la profesora:

> “El contador por colección se incrementa con una operación atómica `$inc`. Eso evita que dos inserciones normales lean el mismo número. Los IDs no son globales ni se garantiza que no haya huecos: si el contador avanzó y luego falla el insert, ese número queda reservado.”

Límites que no hay que esconder:

- El incremento y el insert del empleado son dos operaciones, no una transacción conjunta. Puede haber huecos.
- No se reciclan IDs al borrar.
- Si alguien mete IDs manualmente, también debe coordinar `contadores`.
- `init.js` reconcilia el máximo al ejecutarse, pero una inserción manual posterior puede dejar el contador atrasado.
- La primera creación concurrente del documento de contador merece pruebas: el código no incluye un mecanismo propio de reintento para cada posible colisión de creación.
- `AsignarCursoAsync` sí combina en un `UpdateOne` la condición “aún no existe ese curso en ese empleado” con el `$push`, evitando duplicados en llamadas concurrentes normales a esa operación.
- Las ediciones generales no usan versión de documento. Dos ediciones simultáneas pueden pisarse.

## 11. Validaciones: cuatro guardias diferentes

No existe una sola validación. Hay varias capas, cada una ayuda con una cosa:

### 1. Navegador (HTML y JavaScript)

Ejemplos: `required`, `type="email"`, `min`, `maxlength`, `pattern`; el JavaScript del formulario revisa fechas y pasos.

Sirve para dar feedback rápido. **No es una barrera de seguridad ni garantiza datos válidos** porque puede saltarse.

### 2. ViewModel / Data Annotations

En `AdminViewModels.cs`:

- `[Required]`: campo obligatorio.
- `[StringLength]`: longitud máxima/mínima.
- `[EmailAddress]`: formato de correo.
- `[Range]`: límites numéricos.
- `[RegularExpression]`: patrón, por ejemplo celular de diez dígitos.
- `MinimumAge`: edad mínima del empleado.
- `DateOnOrAfter`: una fecha debe ser igual o posterior a otra.

ASP.NET ejecuta estas reglas al enlazar el POST. Si no es válido, `ModelState.IsValid` es falso y el controlador devuelve el formulario con errores.

### 3. Controlador / reglas de negocio

Ejemplos en `AdminController`:

- correo no duplicado;
- sexo y estado elegidos deben existir/estar activos en el flujo MVC;
- no registrar ambos apellidos vacíos;
- un curso debe tener duración y fecha de inicio o constancia;
- un curso de catálogo debe estar activo para asignarlo;
- un empleado debe estar inactivo antes de eliminarlo definitivamente.

Es donde se aplican reglas que dependen de consultas y de la operación.

### 4. MongoDB / `init.js`

Comprueba estructura, tipos, valores admitidos, longitudes y algunos patrones. Los índices únicos son la última defensa contra duplicados incluso si dos peticiones pasan una comprobación previa a la vez.

**Lo que no hace:** verificar automáticamente que cada ID relacionado exista en otra colección.

## 12. Métodos principales: explicación rápida

### En `MongoCatalog.cs`

- `Query`: permite construir una consulta LINQ sobre la colección.
- `FindAsync(id)`: encuentra un documento por `_id`.
- `InsertAsync(entity)`: pide ID al contador si corresponde y luego inserta.
- `ReplaceAsync(entity)`: reemplaza el documento con ese ID.
- `DeleteAsync(id)`: elimina físicamente un documento.

Es un ayudante genérico: sirve para categorías, cursos, usuarios, estados, etc.

### En `SaturnoStore.cs`

- Constructor: obtiene las colecciones Mongo y crea sus `MongoCatalog`.
- `NextIdAsync`: incrementa y devuelve el contador para una colección.
- `FiltrarEmpleados`: combina filtro por número, estatus y palabras del nombre.
- `BuscarDetalleEmpleadoAsync`: busca empleado y completa en memoria los nombres de sexo/estado.
- `EditarEmpleadoAsync`: actualiza campos del empleado.
- `EliminarEmpleadoInactivoAsync`: solo borra empleado si su estatus está inactivo.
- `SexoEnUsoAsync` / `EstadoEnUsoAsync`: detecta documentos que usan esa opción.
- `AsignarCursoAsync`: agrega una inscripción si todavía no existe para ese empleado.
- `CargarCursosAsync`: lee cursos y completa datos de categoría, nivel e instructor para presentación.
- `CanConnectAsync`: hace ping y comprueba que hay estados y sexos.

### En `AdminController.cs`

- `Empleados`: recibe filtros/página y prepara el ViewModel.
- `BuildEmpleadosViewModelAsync`: cuenta resultados, limita a cinco registros para la página y pagina con `Skip`/`Take`.
- `RegistrarEmpleado` GET: prepara combos y muestra el formulario.
- `RegistrarEmpleado` POST: valida, convierte formulario en entidad, guarda y redirige.
- `EditarEmpleado` GET/POST: carga, valida y actualiza.
- `ValidarEmpleadoAsync`: reglas que necesitan consultar sexo/estado/cursos/correo.
- `AplicarFormulario`: copia los campos del formulario a `Usuario`, domicilio y cursos.
- `Catalogos`, `GuardarCatalogo`, `EliminarCatalogo`: muestra, crea/edita y activa/desactiva sexo o estado.
- `Buscar`: aplica filtros y pagina resultados de cursos.

### En controladores API

- `UsuariosController`: consulta/registro de empleados, estatus y asignación de curso como JSON.
- `CategoriasController`: CRUD API; su DELETE cambia categoría a inactiva.
- `NivelesController`: DELETE físico, pero primero rechaza nivel en uso.
- `InstructoresController`: DELETE lógico, lo cambia a inactivo.
- `CursosController`: crea, busca y cambia estatus de cursos.
- `HealthController`: chequea base/catálogos para `/api/health`.

## 13. ¿Qué pasa cuando cambia un catálogo?

Un catálogo es como una lista de opciones válidas. Hay que distinguir:

- **Editar nombre:** se modifica el documento de catálogo. Los documentos que guardan el ID siguen apuntando a ese mismo ID.
- **Desactivar:** se conserva el documento para historial, pero debe dejar de ofrecerse para nuevas selecciones.
- **Borrar físico:** el documento desaparece; las referencias existentes podrían quedarse colgando si el código no lo impide.

Comportamiento observado:

| Dato | Qué hace el flujo |
|---|---|
| Sexo/estado desde el panel de catálogos | Si está activo y en uso, el panel bloquea desactivarlo; si no está en uso, cambia el estatus. No borra el documento. |
| Categoría desde API | Cambia estatus a `inactivo`; los cursos conservan su `CategoriaId`. La creación de curso requiere categoría activa. |
| Nivel desde API | Comprueba si hay cursos que lo usan; si está en uso devuelve conflicto; si no, lo elimina físicamente. |
| Instructor desde API | Cambia estatus a `inactivo`; conserva el ID que tienen los cursos. |
| Curso del catálogo | Se desactiva; certificados ya registrados dentro del empleado no desaparecen. |
| Renombrar curso de catálogo | El certificado del empleado conserva el texto que ya guardó; no se actualiza automáticamente. |

La comprobación “no está en uso” suele ser leer y después escribir/borrar. No es una FK transaccional: puede haber una carrera si otra solicitud crea la referencia entre esos pasos. Si se pregunta, dilo con claridad.

## 14. Paginación: qué sí y qué no

En la interfaz MVC principal, `PageSize = 5` está en `AdminController`. El patrón es:

1. contar total;
2. calcular páginas;
3. limitar el número de página a un valor válido;
4. saltar `(pagina - 1) * 5`;
5. tomar cinco;
6. dibujar botones Anterior/Siguiente.

En el flujo actual:

- Directorio MVC de empleados: cinco por página.
- Tabla MVC de sexo/estado: cinco por página.
- Lista MVC de categorías: cinco por página.
- Dashboard de cursos y resultados de búsqueda de cursos: cinco por página.
- Endpoint JSON `GET /api/usuarios`: hasta 20 por página, pero pide al menos un filtro y no devuelve metadatos de total/páginas.
- Algunos endpoints API (`GET /api/cursos`, `GET /api/categorias`, `GET /api/niveles`, `GET /api/instructores`) devuelven todos sus resultados.
- Los combos cargan todas las opciones activas; no se paginan porque hay que poder seleccionar cualquiera.

Por eso, no digas “todo el sistema, incluyendo toda la API, está paginado”. Lo correcto es: “Las listas principales del panel tienen páginas de cinco; la API tiene un caso de paginación de usuarios a 20, pero otros `GET` todavía devuelven todo.”

## 15. Login y configuración

En `Program.cs`:

- Se crea el cliente Mongo con `MongoDB:ConnectionString` y se selecciona `MongoDB:Database`.
- Se registra `SaturnoStore` para que controladores puedan usarlo.
- Se configura autenticación con cookies.
- La política fallback requiere sesión para las rutas, salvo las marcadas anónimas.
- Swagger está disponible en Development.
- La ruta `/` cae en `Admin/Empleados`.

El usuario inicial del panel se crea desde `AdministradorInicial:Usuario` y `AdministradorInicial:Password` guardados localmente con User Secrets. La contraseña del administrador usa `PasswordHasher<Administrador>`. `SesionController` verifica el hash, crea la cookie de sesión y permite cerrar sesión.

**Punto crítico que sí hay que reconocer:** el registro JSON heredado de empleado en `UsuariosController` calcula SHA-256 directamente para la contraseña del empleado. No es una estrategia adecuada de almacenamiento de contraseñas; distinto al administrador, que usa `PasswordHasher`.

## 16. Cambios del catálogo Sexo: masculino, femenino y otro

En la base vigente el catálogo inicial tiene tres claves:

```text
M -> Masculino
F -> Femenino
X -> Otro
```

`X` es una clave técnica estable. El texto que ve la persona se puede cambiar sin cambiar esa clave. `init.js` actualiza el nombre de `_id: "X"` a `"Otro"`, así también corrige el registro de base que antes pudo haberse llamado `"No binario / Otro"`.

Ese cambio no agrega empleados ni cambia la clave de sexo dentro de empleados existentes.

## 17. Preguntas típicas y respuesta simple

### “¿Dónde se dibuja el combo de sexo?”

En `frontend/Views/Admin/FormularioEmpleado.cshtml`, en el `<select asp-for="SexoId">`. La lista llega desde `AdminController.BuildEmpleadoFormViewModelAsync`, que consulta `sexos`. La entidad se guarda en `Usuario.SexoId` como `id_sexo`.

### “¿Por qué no se ve el empleado que acabo de ingresar?”

Revisaría en orden: ¿la base conectada es `SaturnoMongo`?, ¿el POST terminó correctamente?, ¿la inserción llegó a `empleados`?, ¿el filtro de la pantalla sigue activo?, ¿se muestra la página correcta?, ¿hay error de validador o índice? `/api/health` prueba conexión y catálogos, no confirma que haya empleados.

### “¿Qué es `seed`?”

Una rutina para insertar catálogos/cursos iniciales solo cuando el ID todavía no existe. No es la pantalla de alta de empleado y no importa datos de otra base.

### “¿Qué impide que se repita el correo?”

El controlador hace una revisión antes de insertar, y el índice único de Mongo es la defensa a nivel de base ante duplicados. Si la base ya tenía correos duplicados, crear el índice único puede fallar.

### “¿Qué hace una relación en Mongo si no hay foreign key?”

Se guarda el ID relacionado. La aplicación busca el catálogo cuando necesita mostrar su nombre y valida algunos usos antes de guardar. Mongo valida tipo/forma, pero no comprueba automáticamente la existencia del otro documento.

### “¿Por qué domicilio/cursos están dentro del empleado?”

Son datos que se consultan como parte del historial del empleado; se eligió guardarlos como subdocumentos para leerlos junto con el empleado. Es una decisión de modelado, no una garantía de que sea igual que las tablas del script SQL.

### “¿Qué significa un `409 Conflict`?”

La solicitud choca con el estado actual del sistema, por ejemplo un nivel que todavía tiene cursos asociados o una clave/nombre duplicado.

## 18. Cómo defender o aceptar una crítica

Usa esta fórmula:

1. **Escucha cuál requisito está evaluando.**
2. **Señala el archivo y método que hacen el trabajo.**
3. **Explica lo que sí garantiza.**
4. **Aclara lo que no garantiza.**
5. **Si el comportamiento no coincide con la consigna, reconoce la brecha y propone el cambio.**

Ejemplo:

> “La validación de que el estado existe ocurre en el controlador MVC y el validador Mongo asegura que `id_estado` sea entero. Pero el validador no comprueba la existencia de ese ID en `estados`, porque Mongo no tiene una FK entre esas colecciones. Además, el endpoint JSON debe aplicar la misma regla de estado activo. Esa es una inconsistencia que podemos cerrar.”

Ser preciso no es “perder la defensa”; es demostrar que sabes separar interfaz, lógica de aplicación y garantía de base de datos.

## 19. Mini-recorrido para estudiar antes de exponer

1. Abre `FormularioEmpleado.cshtml` y localiza `SexoId`, `EstadoId` y el botón Guardar.
2. Sigue a `AdminController.RegistrarEmpleado` GET para ver cómo se cargan opciones.
3. Sigue a `AdminController.RegistrarEmpleado` POST para ver validación y guardado.
4. Revisa `ValidarEmpleadoAsync` y `AplicarFormulario`.
5. Sigue `_db.Usuarios.InsertAsync` a `MongoCatalog.InsertAsync`.
6. Sigue el contador a `SaturnoStore.NextIdAsync`.
7. Compara los nombres BSON de `Usuario.cs`, `DireccionUsuario.cs` y `CursoUsuario.cs` con el bloque `empleado` de `init.js`.
8. Compara las reglas del validador con los valores que realmente genera el ViewModel.
9. Revisa `GuardarCatalogo` y `EliminarCatalogo` para distinguir editar y desactivar.
10. Compara `Sturnotes.txt`, el script Mongo adjunto anterior y `init.js`; anota diferencias de requisito en vez de mezclarlos.

## 20. Comandos de inspección que no borran datos

Ejecuta estos comandos desde PowerShell en la raíz del proyecto:

```powershell
mongosh "mongodb://localhost:27017/SaturnoMongo"
```

Cuando veas el prompt de `mongosh`, puedes usar:

```javascript
show collections
db.empleados.countDocuments()
db.sexos.find({}, { _id: 1, nombre: 1, estatus: 1 })
db.empleados.find({}, { _id: 1, nombre: 1, correo_institucional: 1, estatus: 1 }).sort({ _id: 1 }).limit(5)
db.contadores.find().sort({ _id: 1 })
```

Estos ejemplos solo consultan información; no muestran hashes de contraseñas y no eliminan ni modifican datos. Para salir escribe `exit`.

Para arrancar el proyecto, `init.js` se ejecuta una vez desde la raíz y prepara Mongo; después `dotnet run --project backend\Api\Api.csproj` inicia la aplicación web. El inicializador no es necesario cada vez que solo reinicias ASP.NET, salvo que hayas cambiado las reglas/catálogos del inicializador.

## 21. Archivos que conviene abrir durante una pregunta

- [Program.cs](backend/Api/Program.cs): arranque, Mongo, cookies y permisos.
- [init.js](backend/scripts/mongodb/init.js): esquema Mongo práctico, índices y catálogos iniciales.
- [AdminController.cs](backend/Api/Controllers/Mvc/AdminController.cs): pantallas y flujo MVC.
- [FormularioEmpleado.cshtml](frontend/Views/Admin/FormularioEmpleado.cshtml): combos, campos y pasos.
- [Empleados.cshtml](frontend/Views/Admin/Empleados.cshtml): lista, filtros y paginación.
- [AdminViewModels.cs](backend/Api/Models/ViewModels/AdminViewModels.cs): propiedades y validaciones.
- [Usuario.cs](backend/Api/Models/Data/Entities/Usuario.cs): documento empleado y campos BSON.
- [DireccionUsuario.cs](backend/Api/Models/Data/Entities/DireccionUsuario.cs): dirección embebida.
- [CursoUsuario.cs](backend/Api/Models/Data/Entities/CursoUsuario.cs): certificado/curso embebido.
- [SaturnoStore.cs](backend/Api/Models/Data/SaturnoStore.cs): operaciones Mongo del negocio y contador.
- [MongoCatalog.cs](backend/Api/Models/Data/MongoCatalog.cs): CRUD genérico y asignación del ID.
- [UsuariosController.cs](backend/Api/Controllers/UsuariosController.cs): versión JSON de operaciones de usuario.
- [CategoriasController.cs](backend/Api/Controllers/CategoriasController.cs), [NivelesController.cs](backend/Api/Controllers/NivelesController.cs), [InstructoresController.cs](backend/Api/Controllers/InstructoresController.cs): ejemplos de cómo los catálogos responden a eliminar/cambiar.

## 22. Una respuesta de 30 segundos para presentar

> “SATURNO es una aplicación ASP.NET Core MVC con API JSON. La vista captura datos, el controlador valida y aplica reglas, las entidades C# describen documentos, `SaturnoStore` concentra consultas Mongo y `MongoCatalog` ofrece operaciones comunes. Mongo guarda empleados como documentos y domicilio/cursos como subdocumentos; los catálogos compartidos están en colecciones separadas y se conectan por IDs que la aplicación interpreta. `init.js` establece validadores, índices y catálogos iniciales, pero no crea empleados ni migra la base SQL. Los contadores asignan IDs numéricos mediante `$inc` atómico; hay que distinguir esa garantía de integridad referencial, que Mongo no aplica automáticamente entre colecciones.”
