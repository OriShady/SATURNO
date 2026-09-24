using System.Collections.Generic;

namespace NoSqlU.Data.Entities;

public class Sexo
{
    public char Id { get; set; }
    public string Descripcion { get; set; } = null!;
    public string Estatus { get; set; } = "ACTIVO";
   public ICollection <DireccionUsuario> Direcciones { get; set; } = new List <DireccionUsuario>();
}