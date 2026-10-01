namespace SistemaUsuarios.Models;

/// <summary>
/// Entidad que representa a un usuario registrado en el sistema.
/// Corresponde a la tabla <c>Usuarios</c> de la base de datos SQLite.
/// </summary>
public class Usuario
{
    /// <summary>Identificador único del usuario (llave primaria, autoincremental).</summary>
    public int UsuarioId { get; set; }

    /// <summary>Nombre(s) del usuario. Obligatorio.</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Apellido paterno del usuario. Obligatorio.</summary>
    public string ApellidoPaterno { get; set; } = string.Empty;

    /// <summary>Apellido materno del usuario. Opcional.</summary>
    public string? ApellidoMaterno { get; set; }

    /// <summary>Correo electrónico del usuario. Obligatorio y único.</summary>
    public string Correo { get; set; } = string.Empty;

    /// <summary>Teléfono de contacto de 10 dígitos. Opcional.</summary>
    public string? Telefono { get; set; }

    /// <summary>Fecha de nacimiento. No se permiten fechas futuras.</summary>
    public DateTime FechaNacimiento { get; set; }

    /// <summary>Indica si el usuario se encuentra activo en el sistema.</summary>
    public bool Activo { get; set; } = true;

    /// <summary>Identificador del tipo (perfil) de usuario asociado.</summary>
    public int? TipoUsuarioId { get; set; }

    /// <summary>Navegación al tipo de usuario asociado.</summary>
    public TipoUsuario? TipoUsuario { get; set; }
}
