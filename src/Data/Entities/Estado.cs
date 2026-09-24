using System.Collections.Generic;

namespace NoSqlU.Data.Entities;

public class Estado
{
    public int Id { get; set; }
    public string NombreEstado { get; set; } = null!;
    public ICollection <Usuario> Usuarios { get; set; } = new List <Usuario>();
}