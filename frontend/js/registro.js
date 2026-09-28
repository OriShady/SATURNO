const API_USUARIOS = 'http://localhost:62391/api/usuarios';

document.addEventListener("DOMContentLoaded", function(event) {
    const form = document.querySelector("#formRegistro");
    if (form) {
        form.addEventListener("submit", registrarUsuario);
        cargarEstados();
    }
});

async function cargarEstados() {
    const selector = document.querySelector("#estadoId");

    try {
        const res = await fetch(`${API_USUARIOS}/estados`);
        if (!res.ok) {
            throw new Error("No se pudieron cargar los estados.");
        }

        const estados = await res.json();
        selector.innerHTML = '<option value="">Seleccione...</option>';

        estados.forEach(estado => {
            const opcion = document.createElement("option");
            opcion.value = estado.id;
            opcion.textContent = estado.nombreEstado;
            selector.appendChild(opcion);
        });
    } catch (error) {
        console.error("Error al cargar los estados:", error);
        selector.innerHTML = '<option value="">Error al cargar estados</option>';
    }
}

async function registrarUsuario(e) {
    e.preventDefault();

    const body = {
        nombres: document.querySelector("#nombres").value.trim(),
        apellidoPaterno: document.querySelector("#apellidoPaterno").value.trim(),
        apellidoMaterno: document.querySelector("#apellidoMaterno").value.trim(),
        fechaNacimiento: document.querySelector("#fechaNacimiento").value,
        sexoId: document.querySelector("#sexoId").value,
        correoEmpresarial: document.querySelector("#correoEmpresarial").value.trim(),
        telefono: document.querySelector("#telefono").value.trim(),
        usuarioLogin: document.querySelector("#usuarioLogin").value.trim(),
        password: document.querySelector("#password").value,
        estadoId: Number(document.querySelector("#estadoId").value),
        calle: document.querySelector("#calle").value.trim(),
        numeroExterior: document.querySelector("#numeroExterior").value.trim(),
        numeroInterior: document.querySelector("#numeroInterior").value.trim() || null,
        codigoPostal: document.querySelector("#codigoPostal").value.trim(),
        colonia: document.querySelector("#colonia").value.trim(),
        municipio: document.querySelector("#municipio").value.trim()
    };

    if (!body.apellidoPaterno && !body.apellidoMaterno) {
        alert("Debes proporcionar al menos un apellido (paterno o materno).");
        return;
    }

    try {
        const res = await fetch(`${API_USUARIOS}/registro`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body)
        });

        const data = await res.json();

        if (res.ok === false) {
            throw new Error(data.error || "Error al registrar el usuario");
        }

        alert("Usuario registrado exitosamente con ID: " + data.idUsuario);
        window.location.href = 'index.html';
        e.target.reset();
        
    } catch (error) {
        console.error("Error en el registro:", error);
        alert(error.message);
    }
}