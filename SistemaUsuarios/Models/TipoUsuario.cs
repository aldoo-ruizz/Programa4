namespace SistemaUsuarios.Models;

/// <summary>
/// Catálogo de tipos (perfiles) de usuario del sistema.
/// Corresponde a la tabla <c>TiposUsuario</c> de la base de datos SQLite.
/// </summary>
public class TipoUsuario
{
    /// <summary>Identificador único del tipo de usuario (llave primaria, autoincremental).</summary>
    public int TipoUsuarioId { get; set; }

    /// <summary>Nombre del tipo de usuario (por ejemplo: Administrador, Docente, Alumno). Obligatorio y único.</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Descripción o alcance del tipo de usuario. Opcional.</summary>
    public string? Descripcion { get; set; }

    /// <summary>Indica si el tipo de usuario se encuentra activo en el sistema.</summary>
    public bool Activo { get; set; } = true;

    /// <summary>Usuarios clasificados con este tipo de usuario.</summary>
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
