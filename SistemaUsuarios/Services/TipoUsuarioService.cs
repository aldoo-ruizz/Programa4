using Microsoft.EntityFrameworkCore;
using SistemaUsuarios.DTOs;
using SistemaUsuarios.Models;
using SistemaUsuarios.Models.Comun;
using SistemaUsuarios.Repositories;
using SistemaUsuarios.Validators;

namespace SistemaUsuarios.Services;

/// <summary>
/// Servicio del catálogo de tipos de usuario: aplica las reglas de negocio
/// (nombre obligatorio y único, longitud controlada) antes de delegar la
/// persistencia al repositorio.
/// </summary>
public sealed class TipoUsuarioService : ITipoUsuarioService
{
    private readonly ITipoUsuarioRepository _repositorio;
    private readonly ILogger<TipoUsuarioService> _registro;

    /// <summary>Inicializa el servicio con el repositorio y el logger.</summary>
    public TipoUsuarioService(
        ITipoUsuarioRepository repositorio,
        ILogger<TipoUsuarioService> registro)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        _registro = registro ?? throw new ArgumentNullException(nameof(registro));
    }

    /// <inheritdoc />
    public Task<ResultadoPaginado<TipoUsuarioDto>> ObtenerListadoAsync(
        TipoUsuarioFiltroDto filtro,
        CancellationToken cancelacion = default)
    {
        ArgumentNullException.ThrowIfNull(filtro);

        return _repositorio.ObtenerPaginadoAsync(filtro, cancelacion);
    }

    /// <inheritdoc />
    public async Task<TipoUsuarioDto?> ObtenerPorIdAsync(int tipoUsuarioId, CancellationToken cancelacion = default)
    {
        if (tipoUsuarioId <= 0)
        {
            return null;
        }

        return await _repositorio.ObtenerPorIdAsync(tipoUsuarioId, cancelacion);
    }

    /// <inheritdoc />
    public async Task<ResultadoOperacion<int>> CrearAsync(
        TipoUsuarioCreacionDto dto,
        CancellationToken cancelacion = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        Normalizar(dto);

        var errores = ValidadorModelos.Validar(dto);

        if (errores.Count == 0
            && await _repositorio.ExisteNombreAsync(dto.Nombre, null, cancelacion))
        {
            errores[nameof(TipoUsuarioCreacionDto.Nombre)] = new[]
            {
                "El nombre del tipo de usuario ya está registrado."
            };
        }

        if (errores.Count > 0)
        {
            return ResultadoOperacion<int>.Error(
                "No fue posible registrar el tipo de usuario. Revise los datos capturados.",
                errores);
        }

        var tipoUsuario = MapearEntidad(dto);

        try
        {
            var tipoUsuarioId = await _repositorio.AgregarAsync(tipoUsuario, cancelacion);

            return ResultadoOperacion<int>.Ok(
                tipoUsuarioId,
                $"El tipo de usuario {tipoUsuario.Nombre} fue registrado correctamente.");
        }
        catch (DbUpdateException excepcion)
        {
            _registro.LogError(
                excepcion,
                "Error de persistencia al registrar el tipo de usuario {Nombre}.",
                dto.Nombre);

            var erroresPersistencia = new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                [nameof(TipoUsuarioCreacionDto.Nombre)] = new[]
                {
                    "No fue posible registrar el tipo de usuario: el nombre ya está registrado."
                }
            };

            return ResultadoOperacion<int>.Error(
                "Ocurrió un error al guardar el tipo de usuario.",
                erroresPersistencia);
        }
    }

    /// <inheritdoc />
    public async Task<ResultadoOperacion> ActualizarAsync(
        TipoUsuarioActualizacionDto dto,
        CancellationToken cancelacion = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        Normalizar(dto);

        var errores = ValidadorModelos.Validar(dto);

        if (dto.TipoUsuarioId <= 0)
        {
            errores[nameof(TipoUsuarioActualizacionDto.TipoUsuarioId)] = new[]
            {
                "El identificador del tipo de usuario no es válido."
            };
        }

        if (errores.Count > 0)
        {
            return ResultadoOperacion.Error(
                "No fue posible actualizar el tipo de usuario. Revise los datos capturados.",
                errores);
        }

        var existente = await _repositorio.ObtenerEntidadPorIdAsync(dto.TipoUsuarioId, cancelacion);

        if (existente is null)
        {
            return ResultadoOperacion.Error(
                $"El tipo de usuario con identificador {dto.TipoUsuarioId} no existe o fue eliminado.");
        }

        if (await _repositorio.ExisteNombreAsync(dto.Nombre, dto.TipoUsuarioId, cancelacion))
        {
            var erroresNombre = new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                [nameof(TipoUsuarioActualizacionDto.Nombre)] = new[]
                {
                    "El nombre del tipo de usuario ya está registrado por otro registro."
                }
            };

            return ResultadoOperacion.Error(
                "No fue posible actualizar el tipo de usuario.",
                erroresNombre);
        }

        var tipoUsuario = MapearEntidad(dto, dto.TipoUsuarioId);

        try
        {
            var actualizado = await _repositorio.ActualizarAsync(tipoUsuario, cancelacion);

            return actualizado
                ? ResultadoOperacion.Ok(
                    $"El tipo de usuario {tipoUsuario.Nombre} fue actualizado correctamente.")
                : ResultadoOperacion.Error(
                    $"El tipo de usuario con identificador {dto.TipoUsuarioId} ya no existe.");
        }
        catch (DbUpdateException excepcion)
        {
            _registro.LogError(
                excepcion,
                "Error de persistencia al actualizar el tipo de usuario {TipoUsuarioId}.",
                dto.TipoUsuarioId);

            return ResultadoOperacion.Error("Ocurrió un error al guardar los cambios del tipo de usuario.");
        }
    }

    /// <inheritdoc />
    public async Task<ResultadoOperacion> EliminarAsync(int tipoUsuarioId, CancellationToken cancelacion = default)
    {
        if (tipoUsuarioId <= 0)
        {
            return ResultadoOperacion.Error("El identificador del tipo de usuario no es válido.");
        }

        var existente = await _repositorio.ObtenerEntidadPorIdAsync(tipoUsuarioId, cancelacion);

        if (existente is null)
        {
            return ResultadoOperacion.Error(
                $"El tipo de usuario con identificador {tipoUsuarioId} no existe o ya fue eliminado.");
        }

        // Regla de negocio: no se puede eliminar un tipo de usuario con usuarios asignados.
        var usuariosAsignados = await _repositorio.ContarUsuariosAsignadosAsync(tipoUsuarioId, cancelacion);

        if (usuariosAsignados > 0)
        {
            return ResultadoOperacion.Error(
                $"No es posible eliminar el tipo de usuario {existente.Nombre} porque tiene "
                + $"{usuariosAsignados} usuario(s) asignado(s). Reasigne o desactive este tipo en su lugar.");
        }

        var eliminado = await _repositorio.EliminarAsync(tipoUsuarioId, cancelacion);

        return eliminado
            ? ResultadoOperacion.Ok(
                $"El tipo de usuario {existente.Nombre} fue eliminado correctamente.")
            : ResultadoOperacion.Error($"No fue posible eliminar el tipo de usuario {tipoUsuarioId}.");
    }

    /// <inheritdoc />
    public Task<int> ContarAsync(CancellationToken cancelacion = default) =>
        _repositorio.ContarAsync(cancelacion);

    /// <summary>
    /// Normaliza los datos de captura: recorta espacios extremos, reduce espacios
    /// internos repetidos y convierte cadenas vacías en nulas.
    /// </summary>
    private static void Normalizar(TipoUsuarioBaseDto dto)
    {
        dto.Nombre = ValidadorModelos.NormalizarNombre(dto.Nombre);
        dto.Descripcion = ValidadorModelos.NormalizarTextoOpcional(dto.Descripcion);
    }

    /// <summary>Convierte un DTO de alta o edición en la entidad <see cref="TipoUsuario"/>.</summary>
    private static TipoUsuario MapearEntidad(TipoUsuarioBaseDto dto, int? tipoUsuarioId = null)
    {
        var tipoUsuario = new TipoUsuario
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Activo = dto.Activo
        };

        if (tipoUsuarioId.HasValue)
        {
            tipoUsuario.TipoUsuarioId = tipoUsuarioId.Value;
        }

        return tipoUsuario;
    }
}
