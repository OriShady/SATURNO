const API_USUARIOS = "http://localhost:62391/api/usuarios";

let usuarioId;

document.addEventListener("DOMContentLoaded", async () => {
    const form = document.querySelector("#formAsignacion");
    const idParam = new URLSearchParams(window.location.search).get("id");
    usuarioId = Number(idParam);

    if (!Number.isInteger(usuarioId) || usuarioId < 1) {
        mostrarMensaje("No se recibió un usuario válido. Regresa al listado e inténtalo de nuevo.", "error");
        document.querySelector("#usuarioNombre").textContent = "Usuario no válido";
        return;
    }

    form.addEventListener("submit", asignarCurso);
    document.querySelector("#usuarioId").textContent = `ID ${usuarioId}`;

    const resultados = await Promise.allSettled([cargarUsuario(), cargarCursos()]);
    if (resultados[0].status === "rejected") {
        document.querySelector("#usuarioNombre").textContent = `Usuario #${usuarioId}`;
        document.querySelector("#usuarioCorreo").textContent = "No se pudo cargar la información del usuario";
    }
    if (resultados[1].status === "rejected") {
        mostrarMensaje(resultados[1].reason.message, "error");
    }
});

async function obtenerJson(url) {
    const response = await fetch(url);
    const data = await response.json().catch(() => ({}));

    if (!response.ok) {
        throw new Error(data.error || "No se pudo completar la solicitud.");
    }

    return data;
}

async function cargarUsuario() {
    const usuarios = await obtenerJson(API_USUARIOS);
    const usuario = usuarios.find(item => Number(item.id ?? item.Id) === usuarioId);

    if (!usuario) {
        throw new Error("No se encontró el usuario seleccionado.");
    }

    document.querySelector("#usuarioNombre").textContent = usuario.nombreCompleto ?? usuario.NombreCompleto ?? `Usuario #${usuarioId}`;
    document.querySelector("#usuarioCorreo").textContent = usuario.correoEmpresarial ?? usuario.CorreoEmpresarial ?? "Sin correo registrado";
}

async function cargarCursos() {
    const cursos = await obtenerJson(`${API_USUARIOS}/cursos-disponibles`);
    const select = document.querySelector("#cursoId");
    const boton = document.querySelector("#btnAsignar");

    select.innerHTML = '<option value="">Selecciona un curso...</option>';

    cursos.forEach(curso => {
        const id = curso.id ?? curso.Id;
        const nombre = curso.nombre ?? curso.Nombre;
        if (id == null || !nombre) return;

        const option = document.createElement("option");
        option.value = id;
        option.textContent = nombre;
        select.appendChild(option);
    });

    select.disabled = select.options.length < 2;
    boton.disabled = select.disabled;

    if (select.disabled) {
        document.querySelector("#cursoResumen").textContent = "No hay cursos activos disponibles para asignar.";
    }

    select.addEventListener("change", () => {
        const curso = select.selectedOptions[0];
        document.querySelector("#cursoResumen").textContent = curso.value
            ? `Se asignará: ${curso.textContent}`
            : "Elige un curso para continuar.";
        boton.disabled = !curso.value;
        document.querySelector("#mensajeAsignacion").hidden = true;
    });
}

async function asignarCurso(event) {
    event.preventDefault();

    const select = document.querySelector("#cursoId");
    const boton = document.querySelector("#btnAsignar");
    const cursoId = Number(select.value);
    if (!Number.isInteger(cursoId) || cursoId < 1) return;

    boton.disabled = true;
    boton.textContent = "Asignando...";

    try {
        const response = await fetch(`${API_USUARIOS}/asignar-curso`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ usuarioId, cursoId })
        });
        const data = await response.json().catch(() => ({}));

        if (!response.ok) {
            throw new Error(data.error || "No se pudo asignar el curso.");
        }

        const cursoAsignado = select.selectedOptions[0];
        cursoAsignado.disabled = true;
        mostrarMensaje(data.mensaje || "Curso asignado correctamente.", "success");
        select.value = "";
        document.querySelector("#cursoResumen").textContent = "Elige otro curso para continuar.";
    } catch (error) {
        mostrarMensaje(error.message, "error");
    } finally {
        boton.textContent = "Asignar curso";
        boton.disabled = select.disabled || !select.value;
    }
}

function mostrarMensaje(texto, tipo) {
    const mensaje = document.querySelector("#mensajeAsignacion");
    mensaje.textContent = texto;
    mensaje.className = `assignment-feedback is-${tipo}`;
    mensaje.hidden = false;
}