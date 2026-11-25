
// Ejecutar con mongosh sobre la base configurada en SATURNO.
const txt = (maxLength, minLength = 1) => ({ bsonType: "string", minLength, maxLength });
const optional = rule => ({ ...rule, bsonType: [rule.bsonType, "null"], minLength: 0 });
const intId = { bsonType: "int", minimum: 1 };
const date = { bsonType: "date" };
const estatus = { enum: ["ACTIVO", "INACTIVO"] };
const estatusCurso = { enum: ["activo", "inactivo"] };
const object = (required, properties) => ({ bsonType: "object", required, properties });

const domicilio = object(["calle", "numero_exterior", "codigo_postal", "id_estado"], {
  calle: txt(150), numero_exterior: txt(20), numero_interior: optional(txt(20)),
  codigo_postal: txt(10), id_estado: intId, colonia: optional(txt(100)), municipio: optional(txt(100))
});

const certificado = object(["nombre_curso", "estatus"], {
  id_curso: { bsonType: ["int", "null"], minimum: 1 },
  duracion_minutos: { bsonType: ["int", "null"], minimum: 1 },
  fecha_entrega_constancia: { bsonType: ["date", "null"] },
  nombre_curso: txt(200), institucion: optional(txt(150)),
  fecha_inicio: { bsonType: ["date", "null"] },
  fecha_finalizacion: { bsonType: ["date", "null"] },
  estatus: { enum: ["EN CURSO", "COMPLETADO"] }
});
certificado.anyOf = [
  { properties: { estatus: { enum: ["EN CURSO"] } } },
  { required: ["fecha_finalizacion"], properties: { fecha_finalizacion: date } }
];

const empleado = object(
  ["_id", "nombre", "fecha_nacimiento", "fecha_ingreso", "id_sexo", "correo_institucional", "estatus", "domicilios", "cursos"],
  {
    _id: intId, nombre: txt(100), apellido_paterno: optional(txt(50)), apellido_materno: optional(txt(50)),
    fecha_nacimiento: date, fecha_ingreso: date, correo_institucional: txt(150),
    id_sexo: txt(1), celular: optional(txt(20)), estatus,
    domicilios: { bsonType: "array", items: domicilio },
    cursos: { bsonType: "array", items: certificado },
    credencial: object(["usuario", "password_hash"], { usuario: txt(50), password_hash: txt(255) })
  }
);
empleado.anyOf = ["apellido_paterno", "apellido_materno"].map(campo => ({
  required: [campo], properties: { [campo]: txt(50) }
}));

const validators = {
  empleados: empleado,
  administradores: object(["_id", "password_hash", "estatus"], { _id: txt(100), password_hash: txt(255), estatus }),
  sexos: object(["_id", "nombre", "estatus"], { _id: txt(1), nombre: txt(30), estatus }),
  estados: object(["_id", "nombre", "estatus"], { _id: intId, nombre: txt(100), estatus }),
  categorias: object(["_id", "nombre", "estatus"], {
    _id: intId, nombre: txt(100), descripcion: optional(txt(255)), estatus: estatusCurso, fecha_creacion: date
  }),
  niveles: object(["_id", "nombre", "estatus"], {
    _id: intId, nombre: txt(30), estatus: estatusCurso, fecha_creacion: date
  }),
  instructores: object(["_id", "nombre", "estatus"], {
    _id: intId, nombre: txt(150), apellido: optional(txt(100)), email: optional(txt(150)),
    estatus: estatusCurso, fecha_creacion: date
  }),
  cursos: object(["_id", "nombre", "descripcion", "duracion_minutos", "estatus", "precio", "id_categoria", "id_nivel", "id_instructor"], {
    _id: intId, nombre: txt(200), descripcion: { bsonType: "string", minLength: 1 },
    precio: { bsonType: "decimal", minimum: 0, exclusiveMinimum: true },
    duracion_minutos: intId, estatus: estatusCurso, id_categoria: intId, id_nivel: intId,
    id_instructor: intId, fecha_publicacion: date, fecha_creacion: date
  })
};

const existentes = db.getCollectionNames();
for (const [nombre, schema] of Object.entries(validators)) {
  const options = { validator: { $jsonSchema: schema }, validationLevel: "strict", validationAction: "error" };
  if (existentes.includes(nombre)) {
    const resultado = db.runCommand({ collMod: nombre, ...options });
    if (resultado.ok !== 1) throw new Error(resultado.errmsg);
  } else {
    db.createCollection(nombre, options);
  }
}

const insensitive = { locale: "es", strength: 2 };
for (const nombre of ["sexos", "estados", "categorias", "niveles", "instructores"]) {
  db.getCollection(nombre).createIndex({ nombre: 1 }, { unique: true, collation: insensitive });
}
db.empleados.createIndex({ correo_institucional: 1 }, { unique: true, collation: insensitive });
db.empleados.createIndex({ "credencial.usuario": 1 }, {
  unique: true, partialFilterExpression: { "credencial.usuario": { $type: "string" } }, collation: insensitive
});
db.empleados.createIndex({ estatus: 1, _id: 1 });
db.empleados.createIndex({ "cursos.id_curso": 1 });
db.empleados.createIndex({ id_sexo: 1 });
db.empleados.createIndex({ "domicilios.id_estado": 1 });

function seed(coleccion, documentos) {
  const items = db.getCollection(coleccion);
  for (const { _id, ...datos } of documentos) {
    items.updateOne({ _id }, { $setOnInsert: datos }, { upsert: true });
  }
}

seed("sexos", [
  { _id: "M", nombre: "Masculino", estatus: "ACTIVO" },
  { _id: "F", nombre: "Femenino", estatus: "ACTIVO" },
  { _id: "X", nombre: "No binario / Otro", estatus: "ACTIVO" }
]);

const estadosMexico = [
  "Aguascalientes",
  "Baja California",
  "Baja California Sur",
  "Campeche",
  "Coahuila de Zaragoza",
  "Colima",
  "Chiapas",
  "Chihuahua",
  "Ciudad de México",
  "Durango",
  "Guanajuato",
  "Guerrero",
  "Hidalgo",
  "Jalisco",
  "Estado de México",
  "Michoacán de Ocampo",
  "Morelos",
  "Nayarit",
  "Nuevo León",
  "Oaxaca",
  "Puebla",
  "Querétaro",
  "Quintana Roo",
  "San Luis Potosí",
  "Sinaloa",
  "Sonora",
  "Tabasco",
  "Tamaulipas",
  "Tlaxcala",
  "Veracruz",
  "Yucatán",
  "Zacatecas"
];
seed("estados", estadosMexico.map((nombre, index) => ({ _id: NumberInt(index + 1), nombre, estatus: "ACTIVO" })));

seed("niveles", [
  { _id: NumberInt(1), nombre: "Básico", estatus: "activo", fecha_creacion: ISODate("2026-05-01T00:00:00Z") },
  { _id: NumberInt(2), nombre: "Intermedio", estatus: "activo", fecha_creacion: ISODate("2026-05-01T00:00:00Z") },
  { _id: NumberInt(3), nombre: "Avanzado", estatus: "activo", fecha_creacion: ISODate("2026-05-01T00:00:00Z") }
]);

seed("categorias", [
  { _id: NumberInt(1), nombre: "Desarrollo Web", descripcion: "Cursos relacionados con frontend, backend y aplicaciones web.", estatus: "activo", fecha_creacion: ISODate("2026-05-01T00:00:00Z") },
  { _id: NumberInt(2), nombre: "Bases de Datos", descripcion: "Cursos sobre SQL, NoSQL, modelado y administración de datos.", estatus: "activo", fecha_creacion: ISODate("2026-05-01T00:00:00Z") },
  { _id: NumberInt(3), nombre: "Diseño Digital", descripcion: "Cursos de diseño gráfico, UX/UI y herramientas creativas.", estatus: "activo", fecha_creacion: ISODate("2026-05-01T00:00:00Z") },
  { _id: NumberInt(4), nombre: "Negocios", descripcion: "Cursos sobre emprendimiento, administración y marketing.", estatus: "activo", fecha_creacion: ISODate("2026-05-01T00:00:00Z") },
  { _id: NumberInt(5), nombre: "Ciberseguridad", descripcion: "Cursos de seguridad informática, redes y protección de sistemas.", estatus: "activo", fecha_creacion: ISODate("2026-05-01T00:00:00Z") }
]);

seed("instructores", [
  { _id: NumberInt(1), nombre: "Laura Mendoza", email: "laura.mendoza@email.com", estatus: "activo", fecha_creacion: ISODate("2026-05-01T00:00:00Z") },
  { _id: NumberInt(2), nombre: "Carlos Ramírez", email: "carlos.ramirez@email.com", estatus: "activo", fecha_creacion: ISODate("2026-05-01T00:00:00Z") },
  { _id: NumberInt(3), nombre: "Andrea Torres", email: "andrea.torres@email.com", estatus: "activo", fecha_creacion: ISODate("2026-05-01T00:00:00Z") },
  { _id: NumberInt(4), nombre: "Miguel Hernández", email: "miguel.hernandez@email.com", estatus: "activo", fecha_creacion: ISODate("2026-05-01T00:00:00Z") }
]);

seed("cursos", [
  { _id: NumberInt(1), id_categoria: NumberInt(1), id_nivel: NumberInt(1), id_instructor: NumberInt(1), nombre: "HTML, CSS y JavaScript desde cero", descripcion: "Curso introductorio para crear páginas web modernas.", precio: NumberDecimal("499.00"), duracion_minutos: NumberInt(300), fecha_publicacion: ISODate("2026-05-10T00:00:00Z"), estatus: "activo", fecha_creacion: ISODate("2026-05-10T00:00:00Z") },
  { _id: NumberInt(2), id_categoria: NumberInt(1), id_nivel: NumberInt(2), id_instructor: NumberInt(2), nombre: "PHP y MySQL para aplicaciones web", descripcion: "Aprende a crear sistemas web usando PHP y bases de datos MySQL.", precio: NumberDecimal("799.00"), duracion_minutos: NumberInt(420), fecha_publicacion: ISODate("2026-05-11T00:00:00Z"), estatus: "activo", fecha_creacion: ISODate("2026-05-11T00:00:00Z") },
  { _id: NumberInt(3), id_categoria: NumberInt(2), id_nivel: NumberInt(1), id_instructor: NumberInt(3), nombre: "Fundamentos de SQL", descripcion: "Curso básico para aprender consultas, tablas, relaciones y filtros en SQL.", precio: NumberDecimal("399.00"), duracion_minutos: NumberInt(240), fecha_publicacion: ISODate("2026-05-12T00:00:00Z"), estatus: "activo", fecha_creacion: ISODate("2026-05-12T00:00:00Z") },
  { _id: NumberInt(4), id_categoria: NumberInt(2), id_nivel: NumberInt(2), id_instructor: NumberInt(4), nombre: "MongoDB para principiantes", descripcion: "Aprende documentos, colecciones, filtros y modelado NoSQL.", precio: NumberDecimal("599.00"), duracion_minutos: NumberInt(360), fecha_publicacion: ISODate("2026-05-13T00:00:00Z"), estatus: "activo", fecha_creacion: ISODate("2026-05-13T00:00:00Z") },
  { _id: NumberInt(5), id_categoria: NumberInt(3), id_nivel: NumberInt(2), id_instructor: NumberInt(1), nombre: "UX/UI para interfaces modernas", descripcion: "Diseña interfaces atractivas y fáciles de usar.", precio: NumberDecimal("699.00"), duracion_minutos: NumberInt(390), fecha_publicacion: ISODate("2026-05-14T00:00:00Z"), estatus: "activo", fecha_creacion: ISODate("2026-05-14T00:00:00Z") },
  { _id: NumberInt(6), id_categoria: NumberInt(5), id_nivel: NumberInt(1), id_instructor: NumberInt(2), nombre: "Ciberseguridad básica", descripcion: "Introducción a amenazas, contraseñas, redes y buenas prácticas de seguridad.", precio: NumberDecimal("549.00"), duracion_minutos: NumberInt(330), fecha_publicacion: ISODate("2026-05-15T00:00:00Z"), estatus: "inactivo", fecha_creacion: ISODate("2026-05-15T00:00:00Z") }
]);

for (const nombre of ["empleados", "estados", "categorias", "niveles", "instructores", "cursos"]) {
  const [ultimo] = db.getCollection(nombre).find({}, { _id: 1 }).sort({ _id: -1 }).limit(1).toArray();
  db.contadores.updateOne({ _id: nombre }, { $max: { valor: ultimo?._id ?? NumberInt(0) } }, { upsert: true });
}
print("Base " + db.getName() + " lista. Estados: " + db.estados.countDocuments() + ", sexos: " + db.sexos.countDocuments());
