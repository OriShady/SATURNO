document.addEventListener("DOMContentLoaded", function() {
    cargarUsuarios();
});

async function cargarUsuarios() {
    try {
        const res = await fetch('http://localhost:62391/api/usuarios');
        const data = await res.json();
        
        const tbody = document.querySelector("#tablaUsuarios tbody");
        tbody.innerHTML = "";
        
        data.forEach(function(usuario) {
            // Creamos la fila principal
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
            tdEstatus.textContent = usuario.estatus;
            tr.appendChild(tdEstatus);
            
            // Metemos la fila completa a la tabla
            tbody.appendChild(tr);
        });
    } catch (error) {
        console.error("Error al cargar usuarios:", error);
    }
}