using SistemaUsuarios.DTOs;
using SistemaUsuarios.Models.Comun;

namespace SistemaUsuarios.Services;

/// <summary>
/// Contrato de la capa de servicios con las reglas de negocio de usuarios.
/// </summary>
public interface IUsuarioService
{
    /// <summary>Obtiene el listado paginado de usuarios registrados.</summary>
    Task<ResultadoPaginado<UsuarioDto>> ObtenerListadoAsync(
        UsuarioFiltroDto filtro,
        CancellationToken cancelacion = default);

    /// <summary>Obtiene un usuario por su identificador.</summary>
    Task<UsuarioDto?> ObtenerPorIdAsync(int usuarioId, CancellationToken cancelacion = default);

    /// <summary>Registra un usuario nuevo aplicando las reglas de negocio.</summary>
    Task<ResultadoOperacion<int>> CrearAsync(
        UsuarioCreacionDto dto,
        CancellationToken cancelacion = default);

    /// <summary>Actualiza un usuario existente aplicando las reglas de negocio.</summary>
    Task<ResultadoOperacion> ActualizarAsync(
        UsuarioActualizacionDto dto,
        CancellationToken cancelacion = default);

    /// <summary>Elimina un usuario existente.</summary>
    Task<ResultadoOperacion> EliminarAsync(int usuarioId, CancellationToken cancelacion = default);

    /// <summary>Obtiene los indicadores generales del padrón de usuarios.</summary>
    Task<EstadisticasUsuariosDto> ObtenerEstadisticasAsync(CancellationToken cancelacion = default);
}
