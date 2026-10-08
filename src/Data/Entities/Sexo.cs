namespace NoSqlU.Data.Entities;

public class Sexo
{
    public char Id { get; set; }
    public string Descripcion { get; set; } = null!;
    public string Estatus { get; set; } = "ACTIVO";
}