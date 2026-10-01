using SistemaUsuarios.DTOs;
using SistemaUsuarios.Models;
using SistemaUsuarios.Models.Comun;

namespace SistemaUsuarios.Repositories;

/// <summary>
/// Contrato de acceso a datos de la entidad <see cref="Usuario"/>.
/// </summary>
public interface IUsuarioRepository
{
    /// <summary>Obtiene el listado paginado de usuarios según los criterios recibidos.</summary>
    Task<ResultadoPaginado<UsuarioDto>> ObtenerPaginadoAsync(
        UsuarioFiltroDto filtro,
        CancellationToken cancelacion = default);

    /// <summary>Obtiene un usuario por su identificador como DTO de consulta.</summary>
    Task<UsuarioDto?> ObtenerPorIdAsync(int usuarioId, CancellationToken cancelacion = default);

    /// <summary>Obtiene la entidad <see cref="Usuario"/> por su identificador.</summary>
    Task<Usuario?> ObtenerEntidadPorIdAsync(int usuarioId, CancellationToken cancelacion = default);

    /// <summary>Verifica si el correo ya está registrado, excluyendo opcionalmente un usuario.</summary>
    Task<bool> ExisteCorreoAsync(
        string correo,
        int? excluirUsuarioId = null,
        CancellationToken cancelacion = default);

    /// <summary>Registra un nuevo usuario y devuelve su identificador generado.</summary>
    Task<int> AgregarAsync(Usuario usuario, CancellationToken cancelacion = default);

    /// <summary>Actualiza los datos de un usuario existente.</summary>
    Task<bool> ActualizarAsync(Usuario usuario, CancellationToken cancelacion = default);

    /// <summary>Elimina un usuario existente.</summary>
    Task<bool> EliminarAsync(int usuarioId, CancellationToken cancelacion = default);

    /// <summary>Obtiene los indicadores generales del padrón de usuarios.</summary>
    Task<EstadisticasUsuariosDto> ObtenerEstadisticasAsync(CancellationToken cancelacion = default);
}
