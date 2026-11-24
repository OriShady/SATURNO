using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

using System;

namespace NoSqlU.Data.Entities;

public class CursoUsuario
{

    [BsonElement("id_curso")]
    public int? CursoId { get; set; }
    [BsonElement("nombre_curso")]
    public string NombreCurso { get; set; } = null!;
    [BsonElement("institucion")]
    public string? Institucion { get; set; }
    [BsonElement("fecha_inicio")]
    public DateTime? FechaInicio { get; set; }
    [BsonElement("fecha_finalizacion")]
    public DateTime? FechaFinalizacion { get; set; }
    [BsonElement("estatus")]
    public string Estatus { get; set; } = "EN CURSO";

    [BsonIgnore]
    public Usuario Usuario { get; set; } = null!;
    [BsonIgnore]
    public Curso? Curso { get; set; }
}