// Backend URL - if served from API (ports 5000/5001 or the .NET app ports) use current origin,
// otherwise fallback to the local .NET API address used in development (adjust if needed)
const BACKEND_URL = 'http://localhost:62391';
const BASE_API = `${BACKEND_URL}/api`;
const API = `${BASE_API}/cursos`;
const API_CAT = `${BASE_API}/categorias`;
const API_NIVELES = `${BASE_API}/niveles`;
const API_INSTRUCTORES = `${BASE_API}/instructores`;

document.addEventListener("DOMContentLoaded", () => {
  loadCategoriasSelect();
  loadNivelesSelect();
  loadInstructoresDataList();

  const form = document.querySelector("#formCurso");
  if (form) {
    form.addEventListener("submit", createCurso);
  }
});

async function loadCategoriasSelect() {
  try {
    const res = await fetch(API_CAT);
    const data = await res.json();

    const select = document.querySelector("#categoria");
    select.innerHTML = `Selecciona una categoría`;

    const items = Array.isArray(data) ? data.map(cat => ({ 
        _id: cat._id ?? cat.id ?? cat.id_categoria, 
        nombre: cat.nombre ?? cat.Nombre ?? '', 
        estatus: cat.estatus ?? cat.Estatus ?? 'activo' 
    })) : [];

    // Filtrado robusto y creación segura del DOM
    items
      .filter(cat => cat.estatus.toLowerCase() !== "inactivo")
      .forEach(cat => {
        const option = document.createElement("option");
        option.value = cat._id;
        option.textContent = cat.nombre;
        select.appendChild(option);
      });
  } catch (error) {
    console.error("Error cargando categorías:", error);
    alert("No se pudieron cargar las categorías");
  }
}

async function loadNivelesSelect() {
  try {
    const res = await fetch(API_NIVELES);
    const data = await res.json();

    const select = document.querySelector("#nivel");
    select.innerHTML = `<option value="">Selecciona un nivel</option>`;

    const items = Array.isArray(data) ? data.map(n => ({ nombre: n.nombre ?? n.Nombre ?? '', estatus: n.estatus ?? n.Estatus ?? 'activo' })) : [];

    items
      .filter(n => n.estatus !== "inactivo")
      .forEach(n => {
        select.innerHTML += `<option value="${n.nombre}">${n.nombre}</option>`;
      });
  } catch (error) {
    console.error("Error cargando niveles:", error);
    alert("No se pudieron cargar los niveles");
  }
}

async function loadInstructoresDataList() {
  try {
    const res = await fetch(API_INSTRUCTORES);
    const data = await res.json();

    const list = document.querySelector("#listaInstructores");
    if (!list) return;

    list.innerHTML = "";
    const items = Array.isArray(data) ? data.map(i => ({ nombre: i.nombre ?? i.Nombre ?? '', estatus: i.estatus ?? i.Estatus ?? 'activo' })) : [];
    items
      .filter(i => i.estatus !== "inactivo")
      .forEach(i => {
        list.innerHTML += `<option value="${i.nombre}"></option>`;
      });
  } catch (error) {
    console.error("Error cargando instructores:", error);
  }
}

function validateCurso(body) {
  if (!body.categoria_id) return "Selecciona una categoría";
  if (!body.nombre) return "El nombre del curso es obligatorio";
  if (!body.descripcion) return "La descripción es obligatoria";
  if (!body.nivel) return "Selecciona un nivel";
  if (!body.instructor) return "El instructor es obligatorio";
  if (!Number.isFinite(body.precio) || body.precio <= 0) return "El precio debe ser mayor a 0";
  if (!Number.isInteger(body.duracion_minutos) || body.duracion_minutos <= 0) return "La duración debe ser mayor a 0";
  if (!body.fecha_publicacion) return "La fecha de publicación es obligatoria";
  return null;
}

async function createCurso(e) {
  e.preventDefault();

  const body = {
    nombre: document.querySelector("#nombre").value.trim(),
    descripcion: document.querySelector("#descripcion").value.trim(),
    precio: Number(document.querySelector("#precio").value),
    duracionMinutos: Number(document.querySelector("#duracion").value),
    categoriaId: document.querySelector("#categoria").value,
    nivel: document.querySelector("#nivel").value,
    instructor: document.querySelector("#instructor").value.trim(),
    fecha_publicacion: document.querySelector("#fecha").value
  };

  const validationError = validateCurso(body);
  if (validationError) {
    alert(validationError);
    return;
  }

  try {
    const payload = {
      nombre: body.nombre,
      descripcion: body.descripcion,
      precio: body.precio,
      duracionMinutos: body.duracionMinutos,
      fechaPublicacion: body.fecha_publicacion,
      categoriaId: body.categoriaId,
      nivel: body.nivel,
      instructor: body.instructor
    };

    const res = await fetch(API, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    });

    const data = await res.json();

    if (!res.ok || !data.ok) {
      throw new Error(data.error || "No se pudo crear el curso");
    }

    alert("Curso creado correctamente. Puedes verlo en el Dashboard.");
    e.target.reset();
    await loadInstructoresDataList();
  } catch (error) {
    console.error("Error creando curso:", error);
    alert(error.message);
  }
}
