document.addEventListener("DOMContentLoaded", function() {
    cargarUsuarios();
});

async function cargarUsuarios() {
    try {
        const res = await fetch('http://localhost:62391/api/usuarios');
        const data = await res.json();
        
        const tbody = document.querySelector("#tablaUsuarios tbody");
        const totalUsuarios = document.querySelector("#totalUsuarios");
        tbody.innerHTML = "";

        if (totalUsuarios) {
            totalUsuarios.textContent = `${data.length} ${data.length === 1 ? "registro" : "registros"}`;
        }
        
        data.forEach(function(usuario) {
            // Fila principal
            const tr = document.createElement("tr");
            
            // Columna ID
            const tdId = document.createElement("td");
            tdId.textContent = usuario.id;
            tr.appendChild(tdId);
            
            // Columna Nombre
            const tdNombre = document.createElement("td");
            tdNombre.textContent = usuario.nombreCompleto;
            tr.appendChild(tdNombre);
            
            // Columna Correo
            const tdCorreo = document.createElement("td");
            tdCorreo.textContent = usuario.correoEmpresarial;
            tr.appendChild(tdCorreo);
            
            // Columna Teléfono
            const tdTelefono = document.createElement("td");
            tdTelefono.textContent = usuario.telefono || "N/A";
            tr.appendChild(tdTelefono);
            
            // Columna Estatus
            const tdEstatus = document.createElement("td");
            const status = document.createElement("span");
            const statusText = usuario.estatus || "Sin estatus";
            status.textContent = statusText;
            status.className = `status ${statusText.toLowerCase()}`;
            tdEstatus.appendChild(status);
            tr.appendChild(tdEstatus);

            // Columna de Acciones
            const tdAcciones = document.createElement("td");
            tdAcciones.className = "actions-cell user-row-actions";
            const btnDetalle = document.createElement("button");
            btnDetalle.type = "button";
            btnDetalle.textContent = "Ver detalle";
            btnDetalle.className = "btn btn-secondary";
            btnDetalle.onclick = function() {
                mostrarDetalleUsuario(usuario.id);
            };
            tdAcciones.appendChild(btnDetalle);

            const btnEstatus = document.createElement("button");

            // Ajustar el texto y clase del botón según el estatus
            const esActivo = usuario.estatus === "ACTIVO";
            const accion = esActivo ? "Desactivar" : "Activar";
            const iconoAccion = document.createElement("span");
            iconoAccion.className = "status-action-icon";
            iconoAccion.setAttribute("aria-hidden", "true");
            iconoAccion.textContent = esActivo ? "−" : "+";

            btnEstatus.className = `status-action ${esActivo ? "is-active" : "is-inactive"}`;
            btnEstatus.type = "button";
            btnEstatus.title = `${accion} a ${usuario.nombreCompleto}`;
            btnEstatus.setAttribute("aria-label", `${accion} a ${usuario.nombreCompleto}`);
            btnEstatus.appendChild(iconoAccion);
            btnEstatus.appendChild(document.createTextNode(accion));

            btnEstatus.onclick = function() {
                cambiarEstatus(usuario.id);
            };

            tdAcciones.appendChild(btnEstatus);

           //  Asignar Curso
            const btnAsignar = document.createElement("button");
            btnAsignar.textContent = "Asignar Curso";
            btnAsignar.className = "btn btn-primary";

            // Manda a la nueva pantalla pasando el ID en la URL
            btnAsignar.onclick = function() {
                window.location.href = `asignacion.html?id=${usuario.id}`;
            };

            tdAcciones.appendChild(btnAsignar);

            const btnEliminar = document.createElement("button");
            btnEliminar.type = "button";
            btnEliminar.textContent = "Eliminar";
            btnEliminar.className = "delete-user-action";
            btnEliminar.onclick = function() {
                eliminarUsuario(usuario.id, usuario.nombreCompleto);
            };
            tdAcciones.appendChild(btnEliminar);

            tr.appendChild(tdAcciones);
            
            // Metrar la fila completa a la tabla
            tbody.appendChild(tr);
        });
    } catch (error) {
        console.error("Error al cargar usuarios:", error);
    }
}

async function cambiarEstatus(id) {
    if (!confirm("¿Seguro que deseas cambiar el estatus de este usuario?")) return;

    try {
        const respuesta = await fetch(`http://localhost:62391/api/usuarios/${id}/estatus`, {
            method: 'PUT'
        });

        if (respuesta.ok) {
            cargarUsuarios(); // Recarga la tabla para mostrar el nuevo estatus
        } else {
            alert("Error al actualizar el estatus.");
        }
    } catch (error) {
        console.error("Error de conexión:", error);
    }
}

async function mostrarDetalleUsuario(id) {
    const dialog = document.querySelector("#detalleUsuarioDialog");
    dialog.showModal();
    document.querySelector("#detalleUsuarioTitulo").textContent = `Usuario #${id}`;
    document.querySelectorAll(".user-detail-grid dd").forEach(function(elemento) {
        elemento.textContent = "Cargando...";
    });
    document.querySelector("#detalleDirecciones").replaceChildren();
    document.querySelector("#detalleCursos").replaceChildren();

    try {
        const respuesta = await fetch(`http://localhost:62391/api/usuarios/${id}`);
        if (!respuesta.ok) throw new Error("No se pudo consultar la ficha del usuario.");
        const usuario = await respuesta.json();
        const nombreCompleto = [usuario.nombres, usuario.apellidoPaterno, usuario.apellidoMaterno]
            .filter(Boolean)
            .join(" ");

        document.querySelector("#detalleUsuarioTitulo").textContent = `Usuario #${usuario.id}: ${nombreCompleto}`;
        document.querySelector("#detalleNombre").textContent = nombreCompleto || "No disponible";
        document.querySelector("#detalleNacimiento").textContent = formatearFecha(usuario.fechaNacimiento);
        document.querySelector("#detalleSexo").textContent = usuario.sexo || "No disponible";
        document.querySelector("#detalleCorreo").textContent = usuario.correoEmpresarial || "No disponible";
        document.querySelector("#detalleTelefono").textContent = usuario.telefono || "No registrado";
        document.querySelector("#detalleLogin").textContent = usuario.usuarioLogin || "No disponible";
        document.querySelector("#detalleEstatus").textContent = usuario.estatus || "No disponible";
        document.querySelector("#detalleRegistro").textContent = formatearFecha(usuario.fechaRegistro);

        const direcciones = document.querySelector("#detalleDirecciones");
        if (usuario.direcciones.length === 0) {
            agregarDetalleVacio(direcciones, "Sin domicilio registrado.");
        } else {
            usuario.direcciones.forEach(function(direccion) {
                const numeroInterior = direccion.numeroInterior ? `, Int. ${direccion.numeroInterior}` : "";
                const elemento = document.createElement("li");
                elemento.textContent = `${direccion.calle} ${direccion.numeroExterior}${numeroInterior}, ${direccion.colonia}, ${direccion.municipio}, ${direccion.estado}, C.P. ${direccion.codigoPostal}`;
                direcciones.appendChild(elemento);
            });
        }

        const cursos = document.querySelector("#detalleCursos");
        if (usuario.cursos.length === 0) {
            agregarDetalleVacio(cursos, "No tiene cursos inscritos.");
        } else {
            usuario.cursos.forEach(function(curso) {
                const elemento = document.createElement("li");
                elemento.textContent = `${curso.curso} | ${curso.estatus} | Inicio: ${formatearFecha(curso.fechaInicio)} | Fin: ${formatearFecha(curso.fechaFinalizacion)}`;
                cursos.appendChild(elemento);
            });
        }
    } catch (error) {
        console.error("Error al consultar usuario:", error);
        document.querySelector("#detalleUsuarioTitulo").textContent = "No se pudo cargar la ficha";
    }
}

function agregarDetalleVacio(lista, texto) {
    const elemento = document.createElement("li");
    elemento.textContent = texto;
    lista.appendChild(elemento);
}

function formatearFecha(fecha) {
    if (!fecha) return "No disponible";
    const [anio, mes, dia] = fecha.slice(0, 10).split("-");
    return `${dia}/${mes}/${anio}`;
}

async function eliminarUsuario(id, nombre) {
    const confirmado = confirm(`Se eliminarán permanentemente los datos, domicilio e inscripciones de ${nombre}. Los cursos se conservarán. ¿Continuar?`);
    if (!confirmado) return;

    try {
        const respuesta = await fetch(`http://localhost:62391/api/usuarios/${id}`, {
            method: "DELETE"
        });
        if (!respuesta.ok) throw new Error("No se pudo eliminar el usuario.");
        await cargarUsuarios();
    } catch (error) {
        console.error("Error al eliminar usuario:", error);
        alert("No se pudo eliminar el usuario. Revisa que la API esté ejecutándose.");
    }
}

document.querySelector("#cerrarDetalleUsuario").addEventListener("click", function() {
    document.querySelector("#detalleUsuarioDialog").close();
});