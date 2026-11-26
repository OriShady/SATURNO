using NoSqlU.Data.Entities;

namespace NoSqlU.Api.Models.ViewModels;

public sealed record EmpleadoFilaViewModel(int Id, string Nombres, string? ApellidoPaterno, string? ApellidoMaterno,
    string CorreoEmpresarial, DateTime FechaRegistro, string Estatus);

public sealed record DomicilioViewModel(string Calle, string NumeroExterior, string? NumeroInterior, string CodigoPostal,
    string? Colonia, string? Municipio, string? Estado);

public sealed record CursoHistorialViewModel(string NombreCurso, int? DuracionMinutos, DateTime? FechaInicio,
    DateTime? FechaEntregaConstancia);

public sealed record EmpleadoDetalleViewModel(int Id, string Nombres, string? ApellidoPaterno, string? ApellidoMaterno,
    DateTime FechaNacimiento, DateTime FechaRegistro, string? Sexo, string CorreoEmpresarial, string? Telefono,
    string Estatus, IReadOnlyList<DomicilioViewModel> Direcciones, IReadOnlyList<CursoHistorialViewModel> CursosUsuarios)
{
    internal static EmpleadoDetalleViewModel From(Usuario usuario) =>
        new(usuario.Id, usuario.Nombres, usuario.ApellidoPaterno, usuario.ApellidoMaterno, usuario.FechaNacimiento,
            usuario.FechaRegistro, usuario.Sexo?.Descripcion, usuario.CorreoEmpresarial, usuario.Telefono, usuario.Estatus,
            usuario.Direcciones.Select(d => new DomicilioViewModel(d.Calle, d.NumeroExterior, d.NumeroInterior,
                d.CodigoPostal, d.Colonia, d.Municipio, d.Estado?.NombreEstado)).ToList(),
            usuario.CursosUsuarios.Select(c => new CursoHistorialViewModel(c.NombreCurso, c.DuracionMinutos,
                c.FechaInicio, c.FechaEntregaConstancia ?? c.FechaFinalizacion)).ToList());
}
