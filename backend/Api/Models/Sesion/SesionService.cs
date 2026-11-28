using Microsoft.AspNetCore.Identity;
using NoSqlU.Data;
using NoSqlU.Domain.Entities;

namespace NoSqlU.Api.Models.Sesion;

public sealed class SesionService
{
    private readonly SaturnoStore _store;
    private readonly IPasswordHasher<Administrador> _hasher;

    public SesionService(SaturnoStore store, IPasswordHasher<Administrador> hasher)
    {
        _store = store;
        _hasher = hasher;
    }

    public async Task<string?> AutenticarAsync(string usuario, string password)
    {
        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrEmpty(password))
            return null;

        var administrador = await _store.BuscarAdministradorAsync(usuario.Trim().ToLowerInvariant());
        if (administrador is null ||
            _hasher.VerifyHashedPassword(administrador, administrador.PasswordHash, password) == PasswordVerificationResult.Failed)
            return null;

        return administrador.Usuario;
    }

    public Task CrearAdministradorInicialAsync(string usuario, string password)
    {
        var administrador = new Administrador { Usuario = usuario };
        administrador.PasswordHash = _hasher.HashPassword(administrador, password);
        return _store.CrearAdministradorInicialAsync(administrador);
    }
}
