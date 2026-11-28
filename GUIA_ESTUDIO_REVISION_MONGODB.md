# Guía de estudio: revisión de SATURNO con MongoDB

Esta guía sirve para preparar la revisión y debe actualizarse cuando cambie el modelo o el código. Describe la rama `MongoUpdates` tal como está ahora; no asume que todos los scripts de referencia describen el mismo esquema.

## 1. Resumen para explicar el proyecto

SATURNO es un panel MVC en ASP.NET Core con endpoints JSON. Administra empleados, domicilios, certificados, cursos y catálogos. En esta versión la aplicación persiste con MongoDB usando `MongoDB.Driver`; no usa EF Core ni aplica migraciones relacionales.

El recorrido de una operación es:

```text
Vista o cliente HTTP
  -> controlador MVC o API
  -> SaturnoStore / MongoCatalog
  -> MongoDB: colección y documentos
```

La configuración de servicios, conexión, autenticación y rutas está en [Program.cs](backend/Api/Program.cs). Las entidades C# definen la representación de documentos y los nombres BSON, por ejemplo [Usuario.cs](backend/Api/Models/Data/Entities/Usuario.cs). El acceso compartido está en [SaturnoStore.cs](backend/Api/Models/Data/SaturnoStore.cs) y [MongoCatalog.cs](backend/Api/Models/Data/MongoCatalog.cs). El inicializador [init.js](backend/scripts/mongodb/init.js) crea o actualiza validadores, índices y datos de catálogo.

### Cómo se acoplan el código y `init.js`

No se genera el esquema de Mongo automáticamente desde las clases C#. Hay dos partes que deben mantenerse compatibles:

1. Las entidades C# y sus atributos BSON indican cómo el driver convierte propiedades y nombres de campos.
2. `init.js` establece las reglas que MongoDB aplica al guardar documentos, además de índices y datos iniciales.

Ejemplo: `Usuario.Id` se serializa como `_id`; `Nombres` se guarda como `nombre`; `Direcciones` se guarda como `domicilios`. El validador de `empleados` en `init.js` debe aceptar esos mismos campos y tipos. Si una parte cambia sin la otra, la aplicación puede compilar pero fallar al insertar o actualizar documentos.

El inicializador no es un importador de SQL Server. No copia los empleados existentes ni traduce tablas relacionales a documentos.

## 2. Aclaración importante: hay más de un diseño de referencia

Los materiales disponibles describen modelos distintos. No conviene presentarlos como si fueran un único esquema:

| Aspecto | Diseño relacional descrito en `Sturnotes.txt` | Script Mongo adjunto anterior `Script-Saturno.txt` | Implementación actual de `MongoUpdates` |
|---|---|---|---|
| Forma de guardar datos | Tablas de empleados, domicilios y cursos del empleado; claves y restricciones SQL. | Colecciones con `_id` tipo `ObjectId`. | Colecciones Mongo con documentos y arrays embebidos donde conviene. |
| Identificadores | PK/FK relacionales. | `ObjectId`, incluso en referencias a catálogos. | IDs numéricos para empleados y catálogos, generados con `contadores`; sexo usa una clave de un carácter. |
| Domicilio | La nota describe un domicilio por empleado (`UNIQUE(id_empleado)`). | Un objeto `domicilio` embebido. | Array `domicilios`; el validador permite varios. |
| Cursos/certificados | `cursos_empleado`; la nota describe unicidad por empleado, nombre e institución. | Array `cursos` con `id_curso` obligatorio. | Array `cursos` con nombre y datos del certificado; `id_curso` es opcional. Hay además una colección separada `cursos` para el catálogo. |
| Catálogos | La nota dice que se quitaron categorías, niveles, instructores y el catálogo general de cursos. | Tiene colecciones de sexos, estados, administradores y cursos; no refleja el catálogo completo actual. | Incluye `sexos`, `estados`, `categorias`, `niveles`, `instructores` y `cursos`. |
| Relaciones | Las FK y restricciones SQL hacen cumplir integridad relacional. | Referencias `ObjectId` entre documentos. | Se guardan IDs numéricos relacionados, pero MongoDB no los hace FK. Parte de las comprobaciones ocurre en controladores. |

**Conclusión para defender:** la aplicación actual y `backend/scripts/mongodb/init.js` sí están diseñados para trabajar juntos: usan IDs numéricos, nombres BSON y colecciones compatibles. El script Mongo adjunto anterior usa otro contrato (por ejemplo, `ObjectId`, `domicilio` singular y cursos con `id_curso` obligatorio); no es intercambiable con el inicializador actual.

**Conclusión para refutar una defensa incorrecta:** no se debe decir que `MongoUpdates` reproduce exactamente el diseño relacional de `Sturnotes.txt`. Hay diferencias observables, como múltiples domicilios permitidos, certificados embebidos y catálogos que la nota relacional elimina. Si la revisión exige obedecer ese diseño, hay una decisión de requisitos pendiente o una desviación que debe corregirse, no una equivalencia automática entre SQL y Mongo.

No ejecutes ambos scripts sobre la misma base esperando que sean compatibles. Comparten nombres de colecciones, pero sus campos y tipos de `_id` difieren.

## 3. Modelo actual de MongoDB

### Colecciones principales

- `empleados`: datos personales, estado, sexo, credencial opcional, domicilios embebidos y cursos/certificados embebidos.
- `sexos` y `estados`: catálogos que se consultan desde los empleados y sus domicilios.
- `categorias`, `niveles`, `instructores` y `cursos`: catálogo independiente para administrar cursos. Un curso guarda IDs de categoría, nivel e instructor.
- `administradores`: cuentas que permiten iniciar sesión al panel.
- `contadores`: un documento por colección con el último número asignado.

MongoDB permite que el documento de empleado guarde juntos datos que suelen consultarse juntos, como sus domicilios y certificados. Eso simplifica leer y guardar ese agregado, pero no equivale a una FK SQL ni crea por sí solo reglas entre colecciones.

### Validación e índices

`init.js` configura validadores JSON Schema con nivel `strict` y acción `error`: Mongo rechaza documentos que incumplan los tipos, campos requeridos, rangos y enumeraciones definidos allí. Por ejemplo, limita la forma de los domicilios y certificados.

Los índices únicos evitan duplicados en campos como correos y nombres de catálogos, según la configuración del script. Los índices también pueden acelerar consultas. Un índice único **no** comprueba que un ID referenciado exista en otra colección.

El script usa `collMod` para actualizar validadores en colecciones existentes y `$setOnInsert` para sembrar datos sin reemplazar los documentos que ya existen. Siembra catálogos y cursos de ejemplo, pero **no crea empleados**: los que aparecen en la pantalla principal son los registrados desde el panel o la API. El registro con clave `X` del catálogo de sexo se muestra como `Otro`, y el inicializador corrige también el nombre de un registro `X` creado anteriormente. Por eso, repetir el script no equivale a sincronizar o sobrescribir todos los datos. Crear un índice único puede fallar si ya hay duplicados.

Los listados principales del panel usan cinco registros por página. Los menús desplegables no se paginan porque necesitan presentar todas las opciones para que el usuario pueda seleccionar una.

## 4. Concurrencia e identificadores

### Cómo se generan

`SaturnoStore.NextIdAsync` busca el documento de `contadores` cuyo `_id` es el nombre de la colección, incrementa `valor` con `$inc` y devuelve el valor actualizado mediante `FindOneAndUpdate` con `ReturnDocument.After`. `MongoCatalog.InsertAsync` asigna ese número a la entidad antes de insertar.

Se mantiene un contador independiente por colección; dos empleados y dos categorías no comparten una secuencia. El `_id` de Mongo sigue siendo único dentro de cada colección.

El inicializador actual inserta IDs conocidos para los datos de catálogo y después usa `$max` para dejar los contadores al menos en el mayor ID encontrado. Así, el siguiente ID no debería chocar con los documentos sembrados.

### Respuesta corta para la revisión

> “El incremento del documento contador es atómico en MongoDB. Las solicitudes concurrentes que actualizan el mismo contador reciben valores distintos. El ID no es global, y no prometemos que la secuencia sea continua.”

### Límites que conviene reconocer

- El incremento y la inserción del documento son dos operaciones separadas, no una transacción. Si la inserción falla después de reservar el ID, queda un hueco; eso no rompe la unicidad.
- Si se borra un documento, su ID no se recicla.
- Los IDs insertados manualmente deben coordinarse con `contadores`; de otro modo se puede intentar reutilizar un número. El inicializador reconcilia el máximo al ejecutarse, pero no supervisa cambios manuales posteriores.
- La primera creación concurrente del documento de contador merece una prueba específica; una colisión al crearlo puede producir un error de clave duplicada. El código no implementa un reintento dedicado para ese caso.
- La asignación de un curso a un empleado usa un `UpdateOne` condicionado a que el curso todavía no esté en su array, y agrega el elemento en la misma operación. Esto evita que dos solicitudes concurrentes agreguen el mismo curso al mismo empleado.
- Las ediciones generales reemplazan o actualizan datos sin una versión de concurrencia. Si dos personas editan el mismo empleado al mismo tiempo, un cambio puede sobrescribir al otro. No hay control optimista de versión en el modelo actual.

Para validar la secuencia en una base local de prueba:

```javascript
db.contadores.find().sort({ _id: 1 })
db.empleados.find({}, { _id: 1 }).sort({ _id: 1 })
```

No uses los documentos reales de revisión para hacer pruebas destructivas.

## 5. Qué ocurre si cambia un catálogo

En MongoDB, el comportamiento depende de la acción de la aplicación. La base actual no tiene claves foráneas que propaguen automáticamente cambios o bloqueen todas las eliminaciones.

| Catálogo/dato | Comportamiento que implementa el código actual | Consecuencia |
|---|---|---|
| Sexo o estado en el panel | El flujo de catálogos impide desactivar un valor activo si detecta empleados/domicilios que lo usan; cambia el estatus en vez de borrar el documento. | Se conserva el historial y la referencia. La comprobación es de la aplicación, no una FK Mongo. |
| Categoría | El endpoint de borrado la marca `inactivo`. | Los cursos existentes conservan `CategoriaId`; no hay borrado en cascada. Al crear un curso, se requiere una categoría activa. |
| Nivel | El endpoint de borrado revisa si algún curso lo referencia; si está en uso, responde `409 Conflict`. Si no, lo elimina físicamente. | Es una comprobación previa en el controlador, no una restricción referencial de MongoDB. |
| Instructor | El endpoint de borrado marca el instructor `inactivo`. | Los cursos conservan su `InstructorId`; no se borra el instructor ni se actualizan automáticamente los cursos. |
| Curso del catálogo | Se desactiva; no se elimina físicamente desde esos endpoints. | Los certificados ya guardados en empleados no desaparecen. La asignación de un curso requiere que siga activo. |
| Nombre/datos de catálogo | Se actualiza el documento del catálogo. | Las referencias por ID siguen apuntando al mismo documento. Los certificados del empleado guardan además nombre/duración como datos propios; esos datos no se actualizan automáticamente al renombrar el curso del catálogo. |

### Matiz importante sobre “está en uso”

Las comprobaciones de uso consultan primero los documentos relacionados y después cambian el estatus o eliminan. Como la consulta y la escritura son pasos separados, una operación concurrente podría crear una referencia entre ambos pasos. No se debe afirmar que estas verificaciones ofrecen la misma garantía que una FK con restricción de borrado.

También hay diferencias entre rutas: el flujo MVC de empleados comprueba que sexo/estado existan y estén activos para nuevas selecciones, mientras que el endpoint JSON de registro comprueba que existan, pero no verifica su estatus activo. Esta diferencia es un punto válido para revisar y posiblemente corregir.

## 6. Preguntas probables: cómo responder con precisión

### “¿Por qué usar MongoDB en vez de SQL Server?”

Respuesta defendible: el modelo aprovecha documentos y arrays embebidos para datos que pertenecen al empleado, como domicilios y certificados, y usa colecciones para catálogos compartidos. El driver oficial permite consultarlos desde C#.

No afirmar que MongoDB elimina la necesidad de modelar relaciones, validaciones o índices. Esas decisiones siguen siendo necesarias y algunas relaciones se validan en la aplicación.

### “¿MongoDB garantiza que el ID de estado exista?”

No. El validador puede exigir que `id_estado` sea entero, pero no consulta la colección `estados`. El código busca el estado en algunos flujos; eso no es una FK y debe cubrirse de forma consistente. La condición puede cambiar entre la comprobación y el guardado.

### “¿Se pueden eliminar empleados?”

Los endpoints requieren que el empleado esté `INACTIVO` antes de eliminarlo definitivamente. Domicilios y cursos están embebidos en el documento, así que al borrar el empleado también desaparece ese historial embebido. Esto difiere de una base SQL con tablas dependientes y `NO ACTION`, que rechazaría borrar al padre mientras existan filas hijas.

### “¿Qué pasa si edito un curso después de asignarlo?”

El curso del catálogo cambia en su colección. La inscripción/certificado del empleado conserva datos como nombre y duración que se copiaron al asignarlo. No hay propagación automática; eso puede ser intencional para preservar el historial, pero debe explicarse y acordarse como regla de negocio.

### “¿El script migra los datos viejos?”

No. `init.js` crea/ajusta colecciones, índices, validadores y catálogos de ejemplo. No lee SQL Server ni transforma filas a documentos. Una migración de datos requeriría un proceso aparte con mapeo, validación, control de duplicados y comprobación de resultados.

### “¿Es correcto que `cursos` esté duplicado?”

Hay dos conceptos distintos: la colección `cursos` es el catálogo de cursos disponibles; el array `cursos` dentro de cada empleado guarda inscripciones/certificados asociados a ese empleado. En cada certificado se conserva una copia de información para el historial. Si el requisito fuera registrar cursos libres sin catálogo, el nombre y la estructura actuales deben compararse con ese requisito, no presentarse como idénticos.

## 7. Puntos que sí son críticas válidas

Para defender bien el trabajo, distingue entre una decisión de diseño y un defecto real:

1. **Desalineación entre versiones del script.** El script adjunto con `ObjectId` no coincide con los IDs numéricos y campos plurales del código actual. Ejecutar uno con el otro puede causar validaciones o lecturas incompatibles.
2. **El diseño relacional de las notas no es el modelo actual.** Por ejemplo, las notas limitan a un domicilio y describen cursos del empleado como registros con restricción única; el documento Mongo permite un array de domicilios y un array de certificados. Confirmar si el requisito cambió.
3. **No hay integridad referencial nativa.** La existencia de IDs relacionados depende de validaciones en controladores y no es uniforme en todos los flujos.
4. **Carrera entre verificar y modificar un catálogo.** Comprobar “no está en uso” y borrar/desactivar en operaciones distintas no es garantía contra solicitudes concurrentes.
5. **No hay control de concurrencia en edición general.** Una edición simultánea puede sobrescribir cambios previos.
6. **Contraseña de empleado en el endpoint JSON.** `UsuariosController` calcula SHA-256 directo para la contraseña del empleado. SHA-256 sin un algoritmo dedicado para contraseñas, salt y factor de costo no es adecuado para almacenar passwords. El administrador inicial, en cambio, usa `PasswordHasher`.
7. **Inicializador no es una migración.** Repetirlo no sincroniza todos los datos; `$setOnInsert` conserva el documento que ya existe y los contadores no reparan manualmente cada inconsistencia.

Al responder a una observación, es mejor decir “esto sí lo garantiza el índice”, “esto lo valida solo el controlador” o “esto todavía no está cubierto” que atribuir capacidades inexistentes a MongoDB.

## 8. Dinámicas seguras para demostrar el comportamiento

Realiza estas pruebas solo en `SaturnoMongo` local o en otra base desechable:

1. **Conexión y salud:** ejecutar la aplicación y consultar `/api/health`. Debe confirmar que la base responde y que existen catálogos requeridos.
2. **Revisar el modelo creado:**

   ```javascript
   show collections
   db.contadores.find().sort({ _id: 1 })
   db.categorias.getIndexes()
   db.empleados.getIndexes()
   ```

3. **Mostrar IDs:** crear dos registros del mismo tipo desde el panel y comprobar que reciben IDs distintos. No esperes que no haya huecos si una operación falla.
4. **Mostrar índice único:** intentar guardar un correo duplicado y observar el rechazo. La API puede expresarlo como conflicto; el índice es la última defensa ante carreras en las que dos solicitudes pasan a la vez la comprobación previa.
5. **Mostrar validador:** en una base de prueba, intentar guardar un documento deliberadamente inválido y comprobar que Mongo lo rechaza. No hagas esta prueba en una colección compartida.
6. **Mostrar catálogo en uso:** usar una categoría/estado asociado y comprobar si la acción lo desactiva, lo rechaza o conserva la referencia, según la fila de la sección 5.
7. **Mostrar certificados como historial:** asignar un curso, cambiar el nombre del curso de catálogo y verificar por separado el nombre del catálogo y el nombre guardado en el empleado.

Para inspeccionar datos no imprimas contraseñas ni `password_hash`; basta con proyectar `_id`, estatus e IDs relacionados.

## 9. Archivos que conviene tener ubicados

- [Program.cs](backend/Api/Program.cs): configuración de MongoDB, autenticación, manejo de errores y rutas.
- [init.js](backend/scripts/mongodb/init.js): validadores, índices, catálogos iniciales y sincronización inicial de contadores.
- [SaturnoStore.cs](backend/Api/Models/Data/SaturnoStore.cs): consultas de dominio, contadores y operaciones de empleado.
- [MongoCatalog.cs](backend/Api/Models/Data/MongoCatalog.cs): CRUD genérico y asignación del ID en inserciones.
- [Usuario.cs](backend/Api/Models/Data/Entities/Usuario.cs), [DireccionUsuario.cs](backend/Api/Models/Data/Entities/DireccionUsuario.cs) y [CursoUsuario.cs](backend/Api/Models/Data/Entities/CursoUsuario.cs): documento y datos embebidos de empleado.
- [AdminController.cs](backend/Api/Controllers/Mvc/AdminController.cs): flujos del panel y validación de catálogos.
- [NivelesController.cs](backend/Api/Controllers/NivelesController.cs), [InstructoresController.cs](backend/Api/Controllers/InstructoresController.cs) y [CategoriasController.cs](backend/Api/Controllers/CategoriasController.cs): ejemplos concretos de actualización, borrado lógico y borrado físico.
- [UsuariosController.cs](backend/Api/Controllers/UsuariosController.cs): registro API, baja, credenciales y asignación de cursos.
