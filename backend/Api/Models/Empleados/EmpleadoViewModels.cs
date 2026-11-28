using System.ComponentModel.DataAnnotations;
using NoSqlU.Api.Models.Validation;

namespace NoSqlU.Api.Models.Empleados;

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

public class EmpleadoFilaViewModel
{
    public int Id { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string? ApellidoPaterno { get; set; }
    public string? ApellidoMaterno { get; set; }
    public string CorreoEmpresarial { get; set; } = string.Empty;
    public string Estatus { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
}

public class EmpleadoDetalleViewModel : EmpleadoFilaViewModel
{
    public DateTime FechaNacimiento { get; set; }
    public string? Sexo { get; set; }
    public string? Telefono { get; set; }
    public IReadOnlyList<DireccionEmpleadoViewModel> Direcciones { get; set; } = Array.Empty<DireccionEmpleadoViewModel>();
    public IReadOnlyList<CursoEmpleadoViewModel> CursosUsuarios { get; set; } = Array.Empty<CursoEmpleadoViewModel>();
}

public class DireccionEmpleadoViewModel
{
    public string Calle { get; set; } = string.Empty;
    public string NumeroExterior { get; set; } = string.Empty;
    public string? NumeroInterior { get; set; }
    public string? Colonia { get; set; }
    public string CodigoPostal { get; set; } = string.Empty;
    public string? Municipio { get; set; }
    public string? Estado { get; set; }
}

public class CursoEmpleadoViewModel
{
    public string NombreCurso { get; set; } = string.Empty;
    public int? DuracionMinutos { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaEntregaConstancia { get; set; }
}

public class EstadoEmpleadoOpcionViewModel
{
    public int Id { get; set; }
    public string NombreEstado { get; set; } = string.Empty;
}

public class SexoEmpleadoOpcionViewModel
{
    public char Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;
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
    public IReadOnlyList<EstadoEmpleadoOpcionViewModel> Estados { get; set; } = Array.Empty<EstadoEmpleadoOpcionViewModel>();
    public IReadOnlyList<SexoEmpleadoOpcionViewModel> Sexos { get; set; } = Array.Empty<SexoEmpleadoOpcionViewModel>();
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
