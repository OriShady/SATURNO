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
            tdAcciones.className = "actions-cell";
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