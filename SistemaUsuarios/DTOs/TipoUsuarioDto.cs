namespace SistemaUsuarios.DTOs;

/// <summary>
/// DTO de consulta del catálogo de tipos de usuario. Se utiliza para el listado
/// y para la carga previa de la pantalla de edición.
/// </summary>
public sealed class TipoUsuarioDto
{
    public int TipoUsuarioId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    /// <summary>Cantidad de usuarios clasificados con este tipo de usuario.</summary>
    public int UsuariosAsignados { get; set; }

    /// <summary>Indica si el tipo de usuario tiene usuarios asignados (no puede eliminarse).</summary>
    public bool TieneUsuariosAsignados => UsuariosAsignados > 0;

    /// <summary>Descripción lista para mostrarse en pantalla.</summary>
    public string DescripcionMostrar =>
        string.IsNullOrWhiteSpace(Descripcion) ? "—" : Descripcion;

    /// <summary>Estatus del tipo de usuario listo para mostrarse en pantalla.</summary>
    public string Estatus => Activo ? "Activo" : "Inactivo";
}
