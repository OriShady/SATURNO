// Backend URL - if served from API (ports 5000/5001 or the .NET app ports) use current origin,
// otherwise fallback to the local .NET API address used in development (adjust if needed)
const BACKEND_URL = 'http://localhost:62391';
const BASE_API = `${BACKEND_URL}/api`;
const API_FILTROS = `${BASE_API}/filtro`;
const API_CATEGORIAS = `${BASE_API}/categorias`;
const API_NIVELES = `${BASE_API}/niveles`;
const API_INSTRUCTORES = `${BASE_API}/instructores`;

window.addEventListener("DOMContentLoaded", () => {
  loadCategoriasFiltro();
  loadNivelesFiltro();
  loadInstructoresFiltro();

  const form = document.getElementById("formBusqueda");
  if (form) {
    form.addEventListener("submit", buscar);
    form.addEventListener("reset", () => {
      setTimeout(() => render([]), 0);
    });
  }
});

async function loadCategoriasFiltro() {
  try {
    const res = await fetch(API_CATEGORIAS);
    const categorias = await res.json();
    const select = document.getElementById("categoria");

    if (!select) return;

    select.innerHTML = `<option value="">Todas</option>`;
    categorias
      .map(c => ({
        id: c._id ?? c.id ?? c.id_categoria,
        nombre: c.nombre ?? c.Nombre ?? '' ,
        estatus: c.estatus ?? c.Estatus ?? 'activo'
      }))
      .filter(cat => cat.estatus !== "inactivo")
      .forEach(cat => {
        select.innerHTML += `<option value="${cat.nombre}">${cat.nombre}</option>`;
      });
  } catch (error) {
    console.error("Error cargando categorías para filtro:", error);
  }
}

async function loadNivelesFiltro() {
  try {
    const res = await fetch(API_NIVELES);
    const niveles = await res.json();
    const select = document.getElementById("nivel");

    if (!select) return;

    select.innerHTML = `<option value="">Todos</option>`;
    niveles
      .filter(n => n.estatus !== "inactivo")
      .forEach(n => {
        select.innerHTML += `<option value="${n.nombre}">${n.nombre}</option>`;
      });
  } catch (error) {
    console.error("Error cargando niveles para filtro:", error);
  }
}

async function loadInstructoresFiltro() {
  try {
    const res = await fetch(API_INSTRUCTORES);
    const instructores = await res.json();
    const select = document.getElementById("instructor");

    if (!select) return;

    select.innerHTML = `<option value="">Todos</option>`;
    instructores
      .filter(i => i.estatus !== "inactivo")
      .forEach(i => {
        select.innerHTML += `<option value="${i.nombre}">${i.nombre}</option>`;
      });
  } catch (error) {
    console.error("Error cargando instructores para filtro:", error);
  }
}

async function buscar(e) {
  e.preventDefault();

  const params = new URLSearchParams();

  const q = document.getElementById("q")?.value.trim() || "";
  const categoria = document.getElementById("categoria")?.value || "";
  const nivel = document.getElementById("nivel")?.value || "";
  const instructor = document.getElementById("instructor")?.value || "";
  const estatus = document.getElementById("estatus")?.value || "";
  const min = document.getElementById("min")?.value || "";
  const max = document.getElementById("max")?.value || "";

  if (q) params.append("q", q);
  if (categoria) params.append("categoria", categoria);
  if (nivel) params.append("nivel", nivel);
  if (instructor) params.append("instructor", instructor);
  if (estatus) params.append("estatus", estatus);
  if (min) params.append("min", min);
  if (max) params.append("max", max);

  const url = `${API_FILTROS}/buscar?${params.toString()}`;

  try {
    const res = await fetch(url);
    const data = await res.json();

    if (!res.ok) {
      throw new Error(data.error || "No se pudo realizar la búsqueda");
    }

    // normalize to expected shape
    const items = Array.isArray(data) ? data.map(d => ({
      _id: d._id ?? d.id ?? d.id_curso,
      nombre: d.nombre ?? d.Nombre ?? '',
      descripcion: d.descripcion ?? d.Descripcion ?? '',
      precio: d.precio ?? d.Precio ?? 0,
      estatus: d.estatus ?? d.Estatus ?? 'activo',
      categoria: d.categoria ?? (d.categoria_nombre ? { nombre: d.categoria_nombre } : null),
      nivel: d.nivel ?? (d.nivel_nombre ? { nombre: d.nivel_nombre } : null),
      instructor: d.instructor ?? (d.instructor_nombre ? { nombre: d.instructor_nombre } : null)
    })) : [];

    render(items);
  } catch (error) {
    console.error("Error buscando cursos:", error);
    alert("No se pudo realizar la búsqueda");
  }
}

function render(data) {
  const container = document.querySelector("#resultados");
  if (!container) return;

  container.innerHTML = "";

  if (!data.length) {
    container.innerHTML = `<p>No se encontraron cursos.</p>`;
    return;
  }

  data.forEach(curso => {
    container.innerHTML += `
      <div class="card ${curso.estatus === "inactivo" ? "course-inactive" : ""}">
        <div class="table-header">
          <h3>${curso.nombre}</h3>
          <span class="status ${curso.estatus}">${String(curso.estatus).toUpperCase()}</span>
        </div>
        <p>${curso.descripcion || "Sin descripción"}</p>
        <p><strong>Categoría:</strong> ${curso.categoria?.nombre || "Sin categoría"}</p>
        <p><strong>Nivel:</strong> ${curso.nivel?.nombre || "Sin nivel"}</p>
        <p><strong>Instructor:</strong> ${curso.instructor?.nombre || "Sin instructor"}</p>
        <p><strong>Precio:</strong> $${curso.precio}</p>
      </div>
    `;
  });
}
