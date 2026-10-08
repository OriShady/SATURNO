using System.ComponentModel.DataAnnotations;
using NoSqlU.Data.Entities;

namespace NoSqlU.Api.Models.ViewModels;

public class DashboardViewModel
{
    public int TotalCursos { get; set; }
    public int TotalCategorias { get; set; }
    public int CursosActivos { get; set; }
    public IReadOnlyList<Curso> Cursos { get; set; } = Array.Empty<Curso>();
}

public class CategoriaCreateViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Descripcion { get; set; }
}

public class CategoriasViewModel
{
    public CategoriaCreateViewModel NuevaCategoria { get; set; } = new();
    public IReadOnlyList<Categoria> Categorias { get; set; } = Array.Empty<Categoria>();
}

public class CursoCreateViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    public string Descripcion { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "99999999", ErrorMessage = "El precio debe ser mayor a cero.")]
    public decimal Precio { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La duración debe ser mayor a cero.")]
    public int DuracionMinutos { get; set; }

    [Required(ErrorMessage = "La fecha de publicación es obligatoria.")]
    [DataType(DataType.Date)]
    public DateTime? FechaPublicacion { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona una categoría.")]
    public int CategoriaId { get; set; }

    [Required(ErrorMessage = "Selecciona un nivel.")]
    public string Nivel { get; set; } = string.Empty;

    [Required(ErrorMessage = "El instructor es obligatorio.")]
    [StringLength(150)]
    public string Instructor { get; set; } = string.Empty;

    public IReadOnlyList<Categoria> Categorias { get; set; } = Array.Empty<Categoria>();
    public IReadOnlyList<Nivel> Niveles { get; set; } = Array.Empty<Nivel>();
    public IReadOnlyList<Instructor> Instructores { get; set; } = Array.Empty<Instructor>();
}

public class BuscarCursosViewModel
{
    public string? Q { get; set; }
    public string? Categoria { get; set; }
    public string? Nivel { get; set; }
    public string? Instructor { get; set; }
    public string? Estatus { get; set; }
    public decimal? Min { get; set; }
    public decimal? Max { get; set; }
    public IReadOnlyList<Categoria> Categorias { get; set; } = Array.Empty<Categoria>();
    public IReadOnlyList<Nivel> Niveles { get; set; } = Array.Empty<Nivel>();
    public IReadOnlyList<Instructor> Instructores { get; set; } = Array.Empty<Instructor>();
    public IReadOnlyList<Curso> Resultados { get; set; } = Array.Empty<Curso>();
}

public class RegistroUsuarioViewModel
{
    [Required, StringLength(100)]
    public string Nombres { get; set; } = string.Empty;
    [StringLength(50)]
    public string? ApellidoPaterno { get; set; }
    [StringLength(50)]
    public string? ApellidoMaterno { get; set; }
    [Required, DataType(DataType.Date)]
    public DateTime? FechaNacimiento { get; set; }
    [Required]
    public char? SexoId { get; set; }
    [Required, EmailAddress, StringLength(150)]
    public string CorreoEmpresarial { get; set; } = string.Empty;
    [StringLength(20)]
    public string? Telefono { get; set; }
    [Required, StringLength(50)]
    public string UsuarioLogin { get; set; } = string.Empty;
    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un estado.")]
    public int EstadoId { get; set; }
    [Required, StringLength(150)]
    public string Calle { get; set; } = string.Empty;
    [Required, StringLength(20)]
    public string NumeroExterior { get; set; } = string.Empty;
    [StringLength(20)]
    public string? NumeroInterior { get; set; }
    [Required, StringLength(10)]
    public string CodigoPostal { get; set; } = string.Empty;
    [Required, StringLength(100)]
    public string Colonia { get; set; } = string.Empty;
    [Required, StringLength(100)]
    public string Municipio { get; set; } = string.Empty;
    public IReadOnlyList<Estado> Estados { get; set; } = Array.Empty<Estado>();
}

public class AsignarCursoViewModel
{
    public int UsuarioId { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string CorreoUsuario { get; set; } = string.Empty;
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un curso.")]
    public int CursoId { get; set; }
    public IReadOnlyList<Curso> Cursos { get; set; } = Array.Empty<Curso>();
}
