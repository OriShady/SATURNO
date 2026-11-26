using System.ComponentModel.DataAnnotations;
using NoSqlU.Api.Models.Validation;

namespace NoSqlU.Api.Models.ViewModels;

public class DashboardViewModel
{
    public int TotalCursos { get; set; }
    public int TotalCategorias { get; set; }
    public int CursosActivos { get; set; }
    public int TotalCursosRegistrados { get; set; }
    public int Pagina { get; set; } = 1;
    public int TotalPaginas { get; set; } = 1;
    public IReadOnlyList<CursoDto> Cursos { get; set; } = Array.Empty<CursoDto>();
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
    public int Pagina { get; set; } = 1;
    public int TotalPaginas { get; set; } = 1;
    public int TotalCategorias { get; set; }
    public IReadOnlyList<CategoriaDto> Categorias { get; set; } = Array.Empty<CategoriaDto>();
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

    public IReadOnlyList<CategoriaDto> Categorias { get; set; } = Array.Empty<CategoriaDto>();
    public IReadOnlyList<NivelDto> Niveles { get; set; } = Array.Empty<NivelDto>();
    public IReadOnlyList<InstructorDto> Instructores { get; set; } = Array.Empty<InstructorDto>();
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
    public int Pagina { get; set; } = 1;
    public int TotalPaginas { get; set; } = 1;
    public int TotalResultados { get; set; }
    public IReadOnlyList<CategoriaDto> Categorias { get; set; } = Array.Empty<CategoriaDto>();
    public IReadOnlyList<NivelDto> Niveles { get; set; } = Array.Empty<NivelDto>();
    public IReadOnlyList<InstructorDto> Instructores { get; set; } = Array.Empty<InstructorDto>();
    public IReadOnlyList<CursoDto> Resultados { get; set; } = Array.Empty<CursoDto>();
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
    [MinimumAge(18, ErrorMessage = "El empleado debe tener al menos 18 años.")]
    public DateTime? FechaNacimiento { get; set; }
    [Required]
    public char? SexoId { get; set; }
    [Required(ErrorMessage = "El correo empresarial es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo válido, por ejemplo nombre@dominio.com.")]
    [StringLength(150, ErrorMessage = "El correo no puede exceder 150 caracteres.")]
    public string CorreoEmpresarial { get; set; } = string.Empty;
    [RegularExpression(@"^(?:[0-9]{10})?$", ErrorMessage = "Ingresa 10 dígitos, sin espacios ni guiones.")]
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
    public IReadOnlyList<EstadoDto> Estados { get; set; } = Array.Empty<EstadoDto>();
    public IReadOnlyList<SexoDto> Sexos { get; set; } = Array.Empty<SexoDto>();
}

public class AsignarCursoViewModel
{
    public int UsuarioId { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string CorreoUsuario { get; set; } = string.Empty;
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un curso.")]
    public int CursoId { get; set; }
    public IReadOnlyList<CursoDto> Cursos { get; set; } = Array.Empty<CursoDto>();
}

public class EmpleadosViewModel
{
    public string? Q { get; set; }
    public int? Numero { get; set; }
    public string? Estatus { get; set; }
    public int Pagina { get; set; } = 1;
    public int TotalPaginas { get; set; } = 1;
    public int TotalEmpleados { get; set; }
    public IReadOnlyList<EmpleadoFilaViewModel> Empleados { get; set; } = Array.Empty<EmpleadoFilaViewModel>();
}

public class EmpleadoFormViewModel
{
    public int Id { get; set; }
    [RegularExpression("^(ACTIVO|INACTIVO)$")]
    public string Estatus { get; set; } = "ACTIVO";

    [Required, StringLength(100)]
    public string Nombres { get; set; } = string.Empty;
    [Required, StringLength(50)]
    public string ApellidoPaterno { get; set; } = string.Empty;
    [StringLength(50)]
    public string? ApellidoMaterno { get; set; }
    [Required, DataType(DataType.Date)]
    [MinimumAge(18, ErrorMessage = "El empleado debe tener al menos 18 años.")]
    public DateTime? FechaNacimiento { get; set; }
    [Required, DataType(DataType.Date)]
    [DateOnOrAfter(nameof(FechaNacimiento), ErrorMessage = "La fecha de ingreso no puede ser anterior a la fecha de nacimiento.")]
    public DateTime? FechaIngreso { get; set; }
    [Required]
    public char? SexoId { get; set; }
    [Required(ErrorMessage = "El correo institucional es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo válido, por ejemplo nombre@dominio.com.")]
    [StringLength(150, ErrorMessage = "El correo no puede exceder 150 caracteres.")]
    public string CorreoEmpresarial { get; set; } = string.Empty;
    [RegularExpression(@"^(?:[0-9]{10})?$", ErrorMessage = "Ingresa 10 dígitos, sin espacios ni guiones.")]
    public string? Telefono { get; set; }
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

    public List<CursoEmpleadoFormViewModel> Cursos { get; set; } = new();
    public IReadOnlyList<EstadoDto> Estados { get; set; } = Array.Empty<EstadoDto>();
    public IReadOnlyList<SexoDto> Sexos { get; set; } = Array.Empty<SexoDto>();
}

public class CursoEmpleadoFormViewModel
{
    [Required, StringLength(200)]
    public string NombreCurso { get; set; } = string.Empty;
    [Range(1, int.MaxValue, ErrorMessage = "La duración debe ser mayor a cero.")]
    public int? DuracionMinutos { get; set; }
    [DataType(DataType.Date)]
    public DateTime? FechaInicio { get; set; }
    [DataType(DataType.Date)]
    [DateOnOrAfter(nameof(FechaInicio), ErrorMessage = "La entrega de constancia no puede ser anterior al inicio del curso.")]
    public DateTime? FechaEntregaConstancia { get; set; }
    public int? IndiceOriginal { get; set; }
}

public class CatalogosViewModel
{
    public string Tipo { get; set; } = "sexos";
    public int Pagina { get; set; } = 1;
    public int TotalPaginas { get; set; } = 1;
    public int TotalElementos { get; set; }
    public string? ClaveOriginal { get; set; }
    public bool EsEdicion { get; set; }
    public IReadOnlyList<CatalogoFilaViewModel> Elementos { get; set; } = Array.Empty<CatalogoFilaViewModel>();
    public CatalogoElementoViewModel Elemento { get; set; } = new();
    public bool MostrarFormulario { get; set; }
}

public class CatalogoFilaViewModel
{
    public bool EnUso { get; set; }
    public string Clave { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Estatus { get; set; } = string.Empty;
}

public class CatalogoElementoViewModel
{
    [StringLength(10)]
    public string? Clave { get; set; }
    [Required, StringLength(150)]
    public string Nombre { get; set; } = string.Empty;
    [StringLength(255)]
    public string? Descripcion { get; set; }
    [RegularExpression("^(activo|inactivo)$", ErrorMessage = "El estado debe ser activo o inactivo.")]
    public string Estatus { get; set; } = "activo";
}
