namespace NoSqlU.Data.Entities;

public class DireccionUsuario
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int EstadoId { get; set; }
    public string Calle { get; set; } = null!;
    public string NumeroExterior { get; set; } = null!;
    public string? NumeroInterior { get; set; }
    public string CodigoPostal { get; set; } = null!;
    public string Colonia { get; set; } = null!;
    public string Municipio { get; set; } = null!;

    public Usuario Usuario { get; set; } = null!;
    public Estado Estado { get; set; } = null!;
}