using Microsoft.EntityFrameworkCore;
using SistemaUsuarios.DTOs;
using SistemaUsuarios.Models;
using SistemaUsuarios.Models.Comun;
using SistemaUsuarios.Repositories;
using SistemaUsuarios.Validators;

namespace SistemaUsuarios.Services;

/// <summary>
/// Servicio de usuarios: aplica las reglas de negocio del sistema
/// (correo obligatorio y único, nombre obligatorio, sin fechas futuras)
/// antes de delegar la persistencia al repositorio.
/// </summary>
public sealed class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repositorio;
    private readonly ITipoUsuarioRepository _repositorioTiposUsuario;
    private readonly ILogger<UsuarioService> _registro;

    /// <summary>Inicializa el servicio con el repositorio de usuarios y el logger.</summary>
    public UsuarioService(
        IUsuarioRepository repositorio,
        ITipoUsuarioRepository repositorioTiposUsuario,
        ILogger<UsuarioService> registro)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        _repositorioTiposUsuario = repositorioTiposUsuario ?? throw new ArgumentNullException(nameof(repositorioTiposUsuario));
        _registro = registro ?? throw new ArgumentNullException(nameof(registro));
    }

    /// <inheritdoc />
    public Task<ResultadoPaginado<UsuarioDto>> ObtenerListadoAsync(
        UsuarioFiltroDto filtro,
        CancellationToken cancelacion = default)
    {
        ArgumentNullException.ThrowIfNull(filtro);

        return _repositorio.ObtenerPaginadoAsync(filtro, cancelacion);
    }

    /// <inheritdoc />
    public async Task<UsuarioDto?> ObtenerPorIdAsync(int usuarioId, CancellationToken cancelacion = default)
    {
        if (usuarioId <= 0)
        {
            return null;
        }

        return await _repositorio.ObtenerPorIdAsync(usuarioId, cancelacion);
    }

    /// <inheritdoc />
    public async Task<ResultadoOperacion<int>> CrearAsync(
        UsuarioCreacionDto dto,
        CancellationToken cancelacion = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        Normalizar(dto);

        var errores = ValidadorModelos.Validar(dto);

        if (errores.Count == 0
            && await _repositorio.ExisteCorreoAsync(dto.Correo, null, cancelacion))
        {
            errores[nameof(UsuarioCreacionDto.Correo)] = new[]
            {
                "El correo electrónico ya está registrado por otro usuario."
            };
        }

        if (errores.Count == 0)
        {
            var errorTelefono = ValidarTelefono(dto.Telefono);

            if (errorTelefono is not null)
            {
                errores[nameof(UsuarioCreacionDto.Telefono)] = new[] { errorTelefono };
            }
        }

        if (errores.Count == 0)
        {
            var errorTipo = await ValidarTipoUsuarioAsync(dto.TipoUsuarioId, cancelacion);

            if (errorTipo is not null)
            {
                errores[nameof(UsuarioCreacionDto.TipoUsuarioId)] = new[] { errorTipo };
            }
        }

        if (errores.Count > 0)
        {
            return ResultadoOperacion<int>.Error(
                "No fue posible registrar al usuario. Revise los datos capturados.",
                errores);
        }

        var usuario = MapearEntidad(dto);

        try
        {
            var usuarioId = await _repositorio.AgregarAsync(usuario, cancelacion);

            return ResultadoOperacion<int>.Ok(
                usuarioId,
                $"El usuario {usuario.Nombre} {usuario.ApellidoPaterno} fue registrado correctamente.");
        }
        catch (DbUpdateException excepcion)
        {
            _registro.LogError(
                excepcion,
                "Error de persistencia al registrar el usuario con correo {Correo}.",
                dto.Correo);

            var erroresPersistencia = new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                [nameof(UsuarioCreacionDto.Correo)] = new[]
                {
                    "No fue posible registrar el usuario: el correo ya está registrado."
                }
            };

            return ResultadoOperacion<int>.Error(
                "Ocurrió un error al guardar el usuario.",
                erroresPersistencia);
        }
    }

    /// <inheritdoc />
    public async Task<ResultadoOperacion> ActualizarAsync(
        UsuarioActualizacionDto dto,
        CancellationToken cancelacion = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        Normalizar(dto);

        var errores = ValidadorModelos.Validar(dto);

        if (dto.UsuarioId <= 0)
        {
            errores[nameof(UsuarioActualizacionDto.UsuarioId)] = new[]
            {
                "El identificador del usuario no es válido."
            };
        }

        if (errores.Count > 0)
        {
            return ResultadoOperacion.Error(
                "No fue posible actualizar al usuario. Revise los datos capturados.",
                errores);
        }

        var existente = await _repositorio.ObtenerEntidadPorIdAsync(dto.UsuarioId, cancelacion);

        if (existente is null)
        {
            return ResultadoOperacion.Error(
                $"El usuario con identificador {dto.UsuarioId} no existe o fue eliminado.");
        }

        if (await _repositorio.ExisteCorreoAsync(dto.Correo, dto.UsuarioId, cancelacion))
        {
            var erroresCorreo = new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                [nameof(UsuarioActualizacionDto.Correo)] = new[]
                {
                    "El correo electrónico ya está registrado por otro usuario."
                }
            };

            return ResultadoOperacion.Error(
                "No fue posible actualizar al usuario.",
                erroresCorreo);
        }

        var errorTipoUsuario = await ValidarTipoUsuarioAsync(dto.TipoUsuarioId, cancelacion);

        if (errorTipoUsuario is not null)
        {
            var erroresTipo = new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                [nameof(UsuarioActualizacionDto.TipoUsuarioId)] = new[] { errorTipoUsuario }
            };

            return ResultadoOperacion.Error(
                "No fue posible actualizar al usuario.",
                erroresTipo);
        }

        var errorTelefono = ValidarTelefono(dto.Telefono);

        if (errorTelefono is not null)
        {
            var erroresTelefono = new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                [nameof(UsuarioActualizacionDto.Telefono)] = new[] { errorTelefono }
            };

            return ResultadoOperacion.Error(
                "No fue posible actualizar al usuario.",
                erroresTelefono);
        }

        var usuario = MapearEntidad(dto, dto.UsuarioId);

        try
        {
            var actualizado = await _repositorio.ActualizarAsync(usuario, cancelacion);

            return actualizado
                ? ResultadoOperacion.Ok(
                    $"El usuario {usuario.Nombre} {usuario.ApellidoPaterno} fue actualizado correctamente.")
                : ResultadoOperacion.Error(
                    $"El usuario con identificador {dto.UsuarioId} ya no existe.");
        }
        catch (DbUpdateException excepcion)
        {
            _registro.LogError(
                excepcion,
                "Error de persistencia al actualizar el usuario {UsuarioId}.",
                dto.UsuarioId);

            return ResultadoOperacion.Error("Ocurrió un error al guardar los cambios del usuario.");
        }
    }

    /// <inheritdoc />
    public async Task<ResultadoOperacion> EliminarAsync(int usuarioId, CancellationToken cancelacion = default)
    {
        if (usuarioId <= 0)
        {
            return ResultadoOperacion.Error("El identificador del usuario no es válido.");
        }

        var existente = await _repositorio.ObtenerEntidadPorIdAsync(usuarioId, cancelacion);

        if (existente is null)
        {
            return ResultadoOperacion.Error(
                $"El usuario con identificador {usuarioId} no existe o ya fue eliminado.");
        }

        var eliminado = await _repositorio.EliminarAsync(usuarioId, cancelacion);

        return eliminado
            ? ResultadoOperacion.Ok(
                $"El usuario {existente.Nombre} {existente.ApellidoPaterno} fue eliminado correctamente.")
            : ResultadoOperacion.Error($"No fue posible eliminar al usuario {usuarioId}.");
    }

    /// <inheritdoc />
    public Task<EstadisticasUsuariosDto> ObtenerEstadisticasAsync(CancellationToken cancelacion = default) =>
        _repositorio.ObtenerEstadisticasAsync(cancelacion);

    /// <summary>
    /// Normaliza los datos de captura antes de validar y persistir: recorta espacios,
    /// convierte el correo a minúsculas y limpia el formato del teléfono.
    /// </summary>
    private static void Normalizar(UsuarioBaseDto dto)
    {
        dto.Nombre = ValidadorModelos.NormalizarTexto(dto.Nombre);
        dto.ApellidoPaterno = ValidadorModelos.NormalizarTexto(dto.ApellidoPaterno);
        dto.ApellidoMaterno = ValidadorModelos.NormalizarTextoOpcional(dto.ApellidoMaterno);
        dto.Correo = ValidadorModelos.NormalizarCorreo(dto.Correo) ?? string.Empty;
        dto.Telefono = ValidadorModelos.NormalizarTelefono(dto.Telefono);

        if (dto.FechaNacimiento.HasValue)
        {
            dto.FechaNacimiento = dto.FechaNacimiento.Value.Date;
        }
    }

    /// <summary>
    /// Regla de negocio: el teléfono, cuando se captura, debe contener
    /// exactamente 10 dígitos una vez normalizado (se permiten espacios,
    /// guiones y paréntesis en la captura).
    /// </summary>
    private static string? ValidarTelefono(string? telefono)
    {
        if (string.IsNullOrWhiteSpace(telefono))
        {
            return null;
        }

        var soloDigitos = new string(telefono.Where(char.IsDigit).ToArray());

        return soloDigitos.Length == 10
            ? null
            : "El teléfono debe contener exactamente 10 dígitos.";
    }

    /// <summary>
    /// Valida que el tipo de usuario seleccionado exista en el catálogo.
    /// Devuelve <c>null</c> cuando la selección es válida.
    /// </summary>
    private async Task<string?> ValidarTipoUsuarioAsync(int? tipoUsuarioId, CancellationToken cancelacion)
    {
        if (!tipoUsuarioId.HasValue || tipoUsuarioId.Value <= 0)
        {
            return "Seleccione el tipo de usuario.";
        }

        var tipoUsuario = await _repositorioTiposUsuario
            .ObtenerEntidadPorIdAsync(tipoUsuarioId.Value, cancelacion);

        return tipoUsuario is null
            ? "El tipo de usuario seleccionado no existe en el catálogo."
            : null;
    }

    /// <summary>Convierte un DTO de alta o edición en la entidad <see cref="Usuario"/>.</summary>
    private static Usuario MapearEntidad(UsuarioBaseDto dto, int? usuarioId = null)
    {
        var usuario = new Usuario
        {
            Nombre = dto.Nombre,
            ApellidoPaterno = dto.ApellidoPaterno,
            ApellidoMaterno = dto.ApellidoMaterno,
            Correo = dto.Correo,
            Telefono = dto.Telefono,
            FechaNacimiento = dto.FechaNacimiento ?? DateTime.Today,
            Activo = dto.Activo,
            TipoUsuarioId = dto.TipoUsuarioId
        };

        if (usuarioId.HasValue)
        {
            usuario.UsuarioId = usuarioId.Value;
        }

        return usuario;
    }
}

