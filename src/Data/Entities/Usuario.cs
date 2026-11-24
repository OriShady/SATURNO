using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

using System;
using System.Collections.Generic;

namespace NoSqlU.Data.Entities;

public class Usuario
{
    public int Id { get; set; }
    [BsonElement("nombre")]
    public string Nombres { get; set; } = null!;
    [BsonElement("apellido_paterno")]
    public string? ApellidoPaterno { get; set; }
    [BsonElement("apellido_materno")]
    public string? ApellidoMaterno { get; set; }
    [BsonElement("fecha_nacimiento")]
    public DateTime FechaNacimiento { get; set; }
    [BsonElement("id_sexo")]
    [BsonRepresentation(BsonType.String)]
    public char SexoId { get; set; }
    [BsonElement("correo_institucional")]
    public string CorreoEmpresarial { get; set; } = null!;
    [BsonElement("celular")]
    public string? Telefono { get; set; }

    [BsonElement("fecha_ingreso")]
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    [BsonElement("estatus")]
    public string Estatus { get; set; } = "ACTIVO";

    [BsonIgnore]
    public Sexo Sexo { get; set; } = null!;
    [BsonElement("credencial")]
    [BsonIgnoreIfNull]
    public UsuarioContrasena? Contrasena { get; set; }
    [BsonElement("cursos")]
    public ICollection <CursoUsuario> CursosUsuarios { get; set; } = new List <CursoUsuario>();
    [BsonElement("domicilios")]
    public ICollection <DireccionUsuario> Direcciones { get; set; } = new List <DireccionUsuario>();
}