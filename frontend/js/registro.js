const API_REGISTRO = 'http://localhost:62391/api/usuarios/registro';

document.addEventListener("DOMContentLoaded", function(event) {
    const form = document.querySelector("#formRegistro");
    if (form) {
        form.addEventListener("submit", registrarUsuario);
    }
});

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
        password: document.querySelector("#password").value
    };

    if (!body.apellidoPaterno && !body.apellidoMaterno) {
        alert("Debes proporcionar al menos un apellido (paterno o materno).");
        return;
    }

    try {
        const res = await fetch(API_REGISTRO, {
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