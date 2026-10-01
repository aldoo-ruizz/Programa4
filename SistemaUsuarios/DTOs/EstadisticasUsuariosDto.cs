namespace SistemaUsuarios.DTOs;

/// <summary>
/// Indicadores generales del padrón de usuarios mostrados en el tablero inicial.
/// </summary>
public sealed class EstadisticasUsuariosDto
{
    public int TotalUsuarios { get; init; }

    public int UsuariosActivos { get; init; }

    public int UsuariosInactivos => TotalUsuarios - UsuariosActivos;

    /// <summary>Edad promedio del padrón (0 cuando no hay registros).</summary>
    public decimal EdadPromedio { get; init; }
}
