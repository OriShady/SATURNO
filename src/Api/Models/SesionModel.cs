using Microsoft.AspNetCore.Identity;
using NoSqlU.Data;
using NoSqlU.Data.Entities;

namespace NoSqlU.Api.Models;

public sealed class SesionModel
{
    private readonly SaturnoStore store;
    private readonly IPasswordHasher<Administrador> hasher;

    public SesionModel(SaturnoStore store, IPasswordHasher<Administrador> hasher)
    {
        this.store = store;
        this.hasher = hasher;
    }

    internal async Task<string?> AutenticarAsync(string usuario, string password)
    {
        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrEmpty(password)) return null;

        var administrador = await store.BuscarAdministradorAsync(usuario.Trim().ToLowerInvariant());
        if (administrador is null ||
            hasher.VerifyHashedPassword(administrador, administrador.PasswordHash, password) == PasswordVerificationResult.Failed)
            return null;

        return administrador.Usuario;
    }

    internal Task CrearAdministradorInicialAsync(string usuario, string password)
    {
        var administrador = new Administrador { Usuario = usuario };
        administrador.PasswordHash = hasher.HashPassword(administrador, password);
        return store.CrearAdministradorInicialAsync(administrador);
    }
}
