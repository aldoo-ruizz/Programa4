using SistemaUsuarios.Models.Comun;

namespace SistemaUsuarios.DTOs;


/// <summary>
/// DTO de consulta de usuarios registrados. Se utiliza para el listado
/// y para la carga previa de la pantalla de edición.
/// </summary>
public sealed class UsuarioDto
{
    public int UsuarioId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string ApellidoPaterno { get; set; } = string.Empty;

    public string? ApellidoMaterno { get; set; }

    public string Correo { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    public DateTime FechaNacimiento { get; set; }

    public bool Activo { get; set; }

    /// <summary>Identificador del tipo de usuario asociado.</summary>
    public int? TipoUsuarioId { get; set; }

    /// <summary>Nombre del tipo de usuario asociado.</summary>
    public string? TipoUsuarioNombre { get; set; }

    /// <summary>Nombre del tipo de usuario listo para mostrarse en pantalla.</summary>
    public string TipoUsuarioMostrar =>
        string.IsNullOrWhiteSpace(TipoUsuarioNombre) ? "Sin tipo asignado" : TipoUsuarioNombre;

    /// <summary>Nombre completo del usuario listo para mostrarse en pantalla.</summary>
    public string NombreCompleto =>
        string.Join(' ', new[] { Nombre, ApellidoPaterno, ApellidoMaterno }
            .Where(parte => !string.IsNullOrWhiteSpace(parte)));

    /// <summary>Edad calculada a partir de la fecha de nacimiento.</summary>
    public int Edad => CalculadoraEdad.Calcular(FechaNacimiento);

    /// <summary>Iniciales del usuario utilizadas en las vistas de resumen.</summary>
    public string Iniciales
    {
        get
        {
            var inicialNombre = Nombre.Length > 0 ? Nombre[0].ToString() : string.Empty;
            var inicialApellido = ApellidoPaterno.Length > 0 ? ApellidoPaterno[0].ToString() : string.Empty;
            return $"{inicialNombre}{inicialApellido}".ToUpperInvariant();
        }
    }

}
