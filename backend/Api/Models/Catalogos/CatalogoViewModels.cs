using System.ComponentModel.DataAnnotations;

namespace NoSqlU.Api.Models.Catalogos;

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
