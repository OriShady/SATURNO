console.log("CATEGORIAS JS FUNCIONANDO");

// Backend URL - if served from API (ports 5000/5001 or the .NET app ports) use current origin,
// otherwise fallback to the local .NET API address used in development (adjust if needed)
const BACKEND_URL = 'http://localhost:62391';
const BASE_API = `${BACKEND_URL}/api`;
const API = `${BASE_API}/categorias`;

document.addEventListener("DOMContentLoaded", () => {
  loadCategorias();

  const form = document.querySelector("#formCategoria");
  if (!form) {
    console.error("No se encontró #formCategoria");
    return;
  }

  form.addEventListener("submit", createCategoria);
});

async function loadCategorias() {
  try {
    const res = await fetch(API);
    const data = await res.json();

    if (!res.ok) {
      throw new Error(data.error || "Error al cargar categorías");
    }

    const tbody = document.querySelector("#tabla-categorias");
    const total = document.querySelector("#totalCategorias");

    const items = Array.isArray(data) ? data.map(cat => ({
      _id: cat._id ?? cat.id ?? cat.id_categoria,
      nombre: cat.nombre ?? cat.Nombre ?? '',
      descripcion: cat.descripcion ?? cat.Descripcion ?? '',
      estatus: cat.estatus ?? cat.Estatus ?? 'activo'
    })) : [];

    tbody.innerHTML = "";
    total.innerText = `${items.length} registros`;

    if (!items.length) {
      tbody.innerHTML = `
        <tr>
          <td colspan="5" class="text-center">No hay categorías registradas.</td>
        </tr>
      `;
      return;
    }

    items.forEach((cat, i) => {
      tbody.innerHTML += `
        <tr>
          <td>${i + 1}</td>
          <td><strong>${cat.nombre}</strong></td>
          <td>${cat.descripcion || "-"}</td>
          <td><span class="status ${cat.estatus}">${cat.estatus.toUpperCase()}</span></td>
          <td>
            <button class="btn-icon edit" onclick="toggleStatus('${cat._id}', '${cat.estatus}')">⚡</button>
            <button class="btn-icon delete" onclick="deleteCategoria('${cat._id}')">🗑</button>
          </td>
        </tr>
      `;
    });
  } catch (error) {
    console.error("Error cargando categorías:", error);
    alert("No se pudieron cargar las categorías. Revisa consola.");
  }
}

async function createCategoria(e) {
  e.preventDefault();

  const nombre = document.querySelector("#nombre").value.trim();
  const descripcion = document.querySelector("#descripcion").value.trim();

  if (!nombre) {
    alert("El nombre es obligatorio");
    return;
  }

  try {
    const res = await fetch(API, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ nombre, descripcion })
    });

    const data = await res.json();

    if (!res.ok) {
      throw new Error(data.error || "Error al crear categoría");
    }

    e.target.reset();
    await loadCategorias();
    alert("Categoría creada correctamente");
  } catch (error) {
    console.error("Error creando categoría:", error);
    alert("No se pudo crear la categoría. Revisa consola.");
  }
}

async function toggleStatus(id, current) {
  const estatus = current === "activo" ? "inactivo" : "activo";

  try {
    const res = await fetch(`${API}/${id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ estatus })
    });

    if (!res.ok) throw new Error("Error al actualizar estatus");
    await loadCategorias();
  } catch (error) {
    console.error(error);
    alert("No se pudo actualizar el estatus");
  }
}

async function deleteCategoria(id) {
  if (!confirm("¿Desactivar esta categoría?")) return;

  try {
    const res = await fetch(`${API}/${id}`, { method: "DELETE" });
    if (!res.ok) throw new Error("Error al desactivar categoría");
    await loadCategorias();
  } catch (error) {
    console.error(error);
    alert("No se pudo desactivar la categoría");
  }
}
