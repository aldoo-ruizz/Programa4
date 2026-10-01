using SistemaUsuarios.DTOs;
using SistemaUsuarios.Models;
using SistemaUsuarios.Models.Comun;

namespace SistemaUsuarios.Repositories;

/// <summary>
/// Contrato de acceso a datos del catálogo de tipos de usuario.
/// </summary>
public interface ITipoUsuarioRepository
{
    /// <summary>Obtiene el listado paginado de tipos de usuario según los criterios recibidos.</summary>
    Task<ResultadoPaginado<TipoUsuarioDto>> ObtenerPaginadoAsync(
        TipoUsuarioFiltroDto filtro,
        CancellationToken cancelacion = default);

    /// <summary>Obtiene un tipo de usuario por su identificador como DTO de consulta.</summary>
    Task<TipoUsuarioDto?> ObtenerPorIdAsync(int tipoUsuarioId, CancellationToken cancelacion = default);

    /// <summary>Obtiene la entidad <see cref="TipoUsuario"/> por su identificador.</summary>
    Task<TipoUsuario?> ObtenerEntidadPorIdAsync(int tipoUsuarioId, CancellationToken cancelacion = default);

    /// <summary>Verifica si el nombre ya está registrado, excluyendo opcionalmente un tipo de usuario.</summary>
    Task<bool> ExisteNombreAsync(
        string nombre,
        int? excluirTipoUsuarioId = null,
        CancellationToken cancelacion = default);

    /// <summary>Registra un nuevo tipo de usuario y devuelve su identificador generado.</summary>
    Task<int> AgregarAsync(TipoUsuario tipoUsuario, CancellationToken cancelacion = default);

    /// <summary>Actualiza los datos de un tipo de usuario existente.</summary>
    Task<bool> ActualizarAsync(TipoUsuario tipoUsuario, CancellationToken cancelacion = default);

    /// <summary>Elimina un tipo de usuario existente.</summary>
    Task<bool> EliminarAsync(int tipoUsuarioId, CancellationToken cancelacion = default);

    /// <summary>Obtiene el total de tipos de usuario registrados.</summary>
    Task<int> ContarAsync(CancellationToken cancelacion = default);

    /// <summary>Obtiene la cantidad de usuarios asignados a un tipo de usuario.</summary>
    Task<int> ContarUsuariosAsignadosAsync(int tipoUsuarioId, CancellationToken cancelacion = default);
}
