namespace NoSqlU.Api.Models;

internal enum AdminEstado { Guardado, NoEncontrado, DatosInvalidos, Conflicto, FormularioInvalido }

internal sealed record ErrorFormulario(string Campo, string Mensaje);

internal sealed record AdminResultado(AdminEstado Estado, int? Id = null, string? Mensaje = null)
{
    public object? Formulario { get; private init; }
    public IReadOnlyList<ErrorFormulario> Errores { get; private init; } = Array.Empty<ErrorFormulario>();

    public static AdminResultado Guardado(int? id = null) => new(AdminEstado.Guardado, Id: id);
    public static AdminResultado NoEncontrado() => new(AdminEstado.NoEncontrado);
    public static AdminResultado DatosInvalidos() => new(AdminEstado.DatosInvalidos);
    public static AdminResultado Conflicto(string mensaje) => new(AdminEstado.Conflicto, Mensaje: mensaje);
    public static AdminResultado Invalido(object formulario, IReadOnlyList<ErrorFormulario> errores) =>
        new(AdminEstado.FormularioInvalido) { Formulario = formulario, Errores = errores };
}
