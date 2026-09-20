// Backend URL - if served from API (ports 5000/5001 or the .NET app ports) use current origin,
// otherwise fallback to the local .NET API address used in development (adjust if needed)
const BACKEND_URL = 'http://localhost:62391';
const BASE_API = `${BACKEND_URL}/api`;
const API_CURSOS = `${BASE_API}/cursos`;
const API_CAT = `${BASE_API}/categorias`;

document.addEventListener("DOMContentLoaded", () => {
  loadStats();
  loadCoursesDashboard();
});

async function getCursosYCategorias() {
  const [cursosRes, catRes] = await Promise.all([
    fetch(API_CURSOS),
    fetch(API_CAT)
  ]);

  const cursosRaw = await cursosRes.json();
  const categoriasRaw = await catRes.json();

  // Normalizar formato de respuesta entre la API antigua (PHP) y la nueva (C#)
  const normalizeEntity = (e) => {
    if (!e) return null;
    return {
      _id: (e._id) ? String(e._id) : (e.id ? String(e.id) : (e.id_curso ? String(e.id_curso) : null)),
      nombre: e.nombre ?? e.Nombre ?? '',
      descripcion: e.descripcion ?? e.Descripcion ?? '',
      precio: e.precio ?? e.Precio ?? 0,
      duracion_minutos: e.duracion_minutos ?? e.DuracionMinutos ?? 0,
      fecha_publicacion: e.fecha_publicacion ?? (e.FechaPublicacion ? (Array.isArray(e.FechaPublicacion) ? e.FechaPublicacion[0] : e.FechaPublicacion) : null),
      estatus: e.estatus ?? e.Estatus ?? 'activo',
      fecha_creacion: e.fecha_creacion ?? e.FechaCreacion ?? null,
      categoria: e.categoria ? { _id: (e.categoria._id ?? (e.categoria.id ?? e.categoria.id_categoria) ? String(e.categoria._id ?? e.categoria.id ?? e.categoria.id_categoria) : null), nombre: e.categoria.nombre ?? e.categoria.Nombre ?? '' } : (e.categoria_nombre ? { _id: e.id_categoria, nombre: e.categoria_nombre } : null),
      nivel: e.nivel ? { _id: (e.nivel._id ?? (e.nivel.id ?? e.nivel.id_nivel) ? String(e.nivel._id ?? e.nivel.id ?? e.nivel.id_nivel) : null), nombre: e.nivel.nombre ?? e.nivel.Nombre ?? '' } : (e.nivel_nombre ? { _id: e.id_nivel, nombre: e.nivel_nombre } : null),
      instructor: e.instructor ? { _id: (e.instructor._id ?? (e.instructor.id ?? e.instructor.id_instructor) ? String(e.instructor._id ?? e.instructor.id ?? e.instructor.id_instructor) : null), nombre: e.instructor.nombre ?? e.instructor.Nombre ?? '', apellido: e.instructor.apellido ?? e.instructor.Apellido ?? '', email: e.instructor.email ?? e.instructor.Email ?? '' } : (e.instructor_nombre ? { _id: e.id_instructor, nombre: e.instructor_nombre } : null)
    };
  };

  const cursos = Array.isArray(cursosRaw) ? cursosRaw.map(normalizeEntity) : [];
  const categorias = Array.isArray(categoriasRaw) ? categoriasRaw.map(c => ({ _id: (c._id ?? c.id ?? c.id_categoria) ? String(c._id ?? c.id ?? c.id_categoria) : null, nombre: c.nombre ?? c.Nombre ?? '', descripcion: c.descripcion ?? c.Descripcion ?? '' })) : [];

  return {
    cursos: Array.isArray(cursos) ? cursos : [],
    categorias: Array.isArray(categorias) ? categorias : []
  };
}

async function loadStats() {
  try {
    const { cursos, categorias } = await getCursosYCategorias();

    document.getElementById("totalCursos").innerText = cursos.length;
    document.getElementById("totalCategorias").innerText = categorias.length;
    document.getElementById("cursosActivos").innerText = cursos.filter(c => c.estatus === "activo").length;
  } catch (error) {
    console.error("Error cargando estadísticas:", error);
  }
}

async function loadCoursesDashboard() {
  const container = document.getElementById("coursesDashboard");
  const total = document.getElementById("totalCursosListado");

  if (!container) return;

  try {
    const res = await fetch(API_CURSOS);
    const data = await res.json();
    const cursosRaw = Array.isArray(data) ? data : [];
    const cursos = cursosRaw.map(c => ({
      _id: (c._id) ? String(c._id) : (c.id ? String(c.id) : (c.id_curso ? String(c.id_curso) : null)),
      nombre: c.nombre ?? c.Nombre ?? '',
      descripcion: c.descripcion ?? c.Descripcion ?? '',
      precio: c.precio ?? c.Precio ?? 0,
      duracion_minutos: c.duracion_minutos ?? c.DuracionMinutos ?? 0,
      fecha_publicacion: c.fecha_publicacion ?? c.FechaPublicacion ?? null,
      estatus: c.estatus ?? c.Estatus ?? 'activo',
      categoria: c.categoria ? { nombre: c.categoria.nombre ?? c.categoria.Nombre ?? '' } : (c.categoria_nombre ? { nombre: c.categoria_nombre } : null),
      nivel: c.nivel ? { nombre: c.nivel.nombre ?? c.nivel.Nombre ?? '' } : (c.nivel_nombre ? { nombre: c.nivel_nombre } : null),
      instructor: c.instructor ? { nombre: c.instructor.nombre ?? c.instructor.Nombre ?? '' } : (c.instructor_nombre ? { nombre: c.instructor_nombre } : null),
      fecha_creacion: c.fecha_creacion ?? c.FechaCreacion ?? null
    }));

    container.innerHTML = "";
    if (total) total.innerText = `${cursos.length} registros`;

    if (!cursos.length) {
      container.innerHTML = `<p>No hay cursos registrados.</p>`;
      return;
    }

    cursos.forEach(c => {
      const inactive = c.estatus === "inactivo";
      container.innerHTML += `
        <div class="card ${inactive ? "course-inactive" : ""}">
          <div class="table-header">
            <h3>${c.nombre}</h3>
            <span class="status ${c.estatus}">${String(c.estatus).toUpperCase()}</span>
          </div>
          <p>${c.descripcion || "Sin descripción"}</p>
          <p><strong>Categoría:</strong> ${c.categoria?.nombre || "Sin categoría"}</p>
          <p><strong>Nivel:</strong> ${c.nivel?.nombre || "Sin nivel"}</p>
          <p><strong>Instructor:</strong> ${c.instructor?.nombre || "Sin instructor"}</p>
          <p><strong>Precio:</strong> $${Number(c.precio || 0).toFixed(2)}</p>
          <p><strong>Duración:</strong> ${c.duracion_minutos || 0} min</p>
          <p><strong>Publicación:</strong> ${c.fecha_publicacion || "Sin fecha"}</p>
          <button onclick="toggleCursoStatus('${c._id}', '${c.estatus}')" class="btn ${inactive ? "btn-primary" : "btn-secondary"}">
            ${inactive ? "Reactivar" : "Desactivar"}
          </button>
        </div>
      `;
    });
  } catch (error) {
    console.error("Error cargando cursos del dashboard:", error);
    container.innerHTML = `<p>No se pudieron cargar los cursos.</p>`;
  }
}

async function toggleCursoStatus(id, currentStatus) {
  const nuevoEstatus = currentStatus === "inactivo" ? "activo" : "inactivo";
  const accion = nuevoEstatus === "activo" ? "reactivar" : "desactivar";

  if (!confirm(`¿Seguro que deseas ${accion} este curso?`)) return;

  try {
    const res = await fetch(`${API_CURSOS}/${id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ estatus: nuevoEstatus })
    });

    const data = await res.json();

    if (!res.ok || !data.ok) {
      throw new Error(data.error || `No se pudo ${accion} el curso`);
    }

    await loadStats();
    await loadCoursesDashboard();
    alert(`Curso ${nuevoEstatus === "activo" ? "reactivado" : "desactivado"} correctamente`);
  } catch (error) {
    console.error(error);
    alert(error.message);
  }
}
