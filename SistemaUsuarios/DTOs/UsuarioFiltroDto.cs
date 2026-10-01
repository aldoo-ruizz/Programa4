namespace SistemaUsuarios.DTOs;

/// <summary>
/// Criterios de búsqueda, filtrado y paginación del listado de usuarios.
/// </summary>
public sealed class UsuarioFiltroDto
{
    /// <summary>Texto libre que se compara contra nombre, apellidos y correo.</summary>
    public string? Texto { get; set; }

    /// <summary>Filtro de estatus: <c>null</c> = todos, <c>true</c> = activos, <c>false</c> = inactivos.</summary>
    public bool? Activo { get; set; }

    /// <summary>Filtro por tipo de usuario: <c>null</c> = todos los tipos.</summary>
    public int? TipoUsuarioId { get; set; }

    /// <summary>Indica si solo deben mostrarse los usuarios sin tipo asignado.</summary>
    public bool SoloSinTipoAsignado { get; set; }

    /// <summary>Número de página solicitada (base 1).</summary>
    public int Pagina { get; set; } = 1;

    /// <summary>Cantidad de registros por página (1 a 100).</summary>
    public int TamanoPagina { get; set; } = 10;

    /// <summary>Registros que deben omitirse para llegar a la página solicitada.</summary>
    public int RegistrosOmitidos => (PaginaNormalizada - 1) * TamanoPaginaNormalizado;

    /// <summary>Página normalizada (nunca menor a 1).</summary>
    public int PaginaNormalizada => Pagina < 1 ? 1 : Pagina;

    /// <summary>Tamaño de página normalizado (entre 1 y 100 registros).</summary>
    public int TamanoPaginaNormalizado => TamanoPagina switch
    {
        < 1 => 10,
        > 100 => 100,
        _ => TamanoPagina
    };

    /// <summary>Texto de búsqueda normalizado (sin espacios extremos).</summary>
    public string TextoNormalizado => string.IsNullOrWhiteSpace(Texto) ? string.Empty : Texto.Trim();
}
