namespace NoSqlU.Data.Entities;

public class Estado
{
    public int Id { get; set; }
    public string NombreEstado { get; set; } = null!;
    public string Estatus { get; set; } = "ACTIVO";
}