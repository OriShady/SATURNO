namespace NoSqlU.Data.Entities;

public class UsuarioContrasena
{
    public int UsuarioId { get; set; }
    public string UsuarioLogin { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
}