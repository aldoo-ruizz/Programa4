using SistemaUsuarios.DTOs;
using SistemaUsuarios.Models.Comun;

namespace SistemaUsuarios.Services;

/// <summary>
/// Contrato de la capa de servicios del catálogo de tipos de usuario.
/// </summary>
public interface ITipoUsuarioService
{
    /// <summary>Obtiene el listado paginado de tipos de usuario registrados.</summary>
    Task<ResultadoPaginado<TipoUsuarioDto>> ObtenerListadoAsync(
        TipoUsuarioFiltroDto filtro,
        CancellationToken cancelacion = default);

    /// <summary>Obtiene un tipo de usuario por su identificador.</summary>
    Task<TipoUsuarioDto?> ObtenerPorIdAsync(int tipoUsuarioId, CancellationToken cancelacion = default);

    /// <summary>Registra un tipo de usuario nuevo aplicando las reglas de negocio.</summary>
    Task<ResultadoOperacion<int>> CrearAsync(
        TipoUsuarioCreacionDto dto,
        CancellationToken cancelacion = default);

    /// <summary>Actualiza un tipo de usuario existente aplicando las reglas de negocio.</summary>
    Task<ResultadoOperacion> ActualizarAsync(
        TipoUsuarioActualizacionDto dto,
        CancellationToken cancelacion = default);

    /// <summary>Elimina un tipo de usuario existente.</summary>
    Task<ResultadoOperacion> EliminarAsync(int tipoUsuarioId, CancellationToken cancelacion = default);

    /// <summary>Obtiene el total de tipos de usuario registrados.</summary>
    Task<int> ContarAsync(CancellationToken cancelacion = default);
}
