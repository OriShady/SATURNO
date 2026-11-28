using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

using System;

namespace NoSqlU.Domain.Entities;

public class CursoUsuario
{

    [BsonElement("id_curso")]
    public int? CursoId { get; set; }
    [BsonElement("nombre_curso")]
    public string NombreCurso { get; set; } = null!;
    [BsonElement("duracion_minutos")]
    public int? DuracionMinutos { get; set; }
    [BsonElement("fecha_entrega_constancia")]
    public DateTime? FechaEntregaConstancia { get; set; }
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
}
