using Microsoft.EntityFrameworkCore;
using SistemaUsuarios.Data;
using SistemaUsuarios.DTOs;
using SistemaUsuarios.Models;
using SistemaUsuarios.Models.Comun;

namespace SistemaUsuarios.Repositories;

/// <summary>
/// Implementación del repositorio de usuarios sobre Entity Framework Core y SQLite.
/// Cada operación trabaja con su propio <see cref="ApplicationDbContext"/> creado a
/// partir de <see cref="IDbContextFactory{TContext}"/>, evitando el uso compartido de
/// contextos entre circuitos de Blazor Server.
/// </summary>
public sealed class UsuarioRepository : IUsuarioRepository
{
    private const string CaracterEscape = "\\";

    private readonly IDbContextFactory<ApplicationDbContext> _fabricaContextos;
    private readonly ILogger<UsuarioRepository> _registro;

    /// <summary>Inicializa el repositorio con la fábrica de contextos y el logger.</summary>
    public UsuarioRepository(
        IDbContextFactory<ApplicationDbContext> fabricaContextos,
        ILogger<UsuarioRepository> registro)
    {
        _fabricaContextos = fabricaContextos ?? throw new ArgumentNullException(nameof(fabricaContextos));
        _registro = registro ?? throw new ArgumentNullException(nameof(registro));
    }

    /// <inheritdoc />
    public async Task<ResultadoPaginado<UsuarioDto>> ObtenerPaginadoAsync(
        UsuarioFiltroDto filtro,
        CancellationToken cancelacion = default)
    {
        ArgumentNullException.ThrowIfNull(filtro);

        var pagina = filtro.PaginaNormalizada;
        var tamanoPagina = filtro.TamanoPaginaNormalizado;

        await using var contexto = await _fabricaContextos.CreateDbContextAsync(cancelacion);

        var consulta = AplicarFiltros(
            contexto.Usuarios.AsNoTracking(),
            filtro.TextoNormalizado,
            filtro.Activo,
            filtro.TipoUsuarioId,
            filtro.SoloSinTipoAsignado);

        var totalRegistros = await consulta.CountAsync(cancelacion);

        var elementos = await consulta
            .OrderBy(usuario => usuario.ApellidoPaterno)
            .ThenBy(usuario => usuario.Nombre)
            .ThenBy(usuario => usuario.UsuarioId)
            .Skip(filtro.RegistrosOmitidos)
            .Take(tamanoPagina)
            .Select(usuario => new UsuarioDto
            {
                UsuarioId = usuario.UsuarioId,
                Nombre = usuario.Nombre,
                ApellidoPaterno = usuario.ApellidoPaterno,
                ApellidoMaterno = usuario.ApellidoMaterno,
                Correo = usuario.Correo,
                Telefono = usuario.Telefono,
                FechaNacimiento = usuario.FechaNacimiento,
                Activo = usuario.Activo,
                TipoUsuarioId = usuario.TipoUsuarioId,
                TipoUsuarioNombre = usuario.TipoUsuario != null ? usuario.TipoUsuario.Nombre : null
            })
            .ToListAsync(cancelacion);

        _registro.LogDebug(
            "Consulta de usuarios: {Total} registro(s) encontrados en la página {Pagina}.",
            totalRegistros,
            pagina);

        return new ResultadoPaginado<UsuarioDto>
        {
            Elementos = elementos,
            Pagina = pagina,
            TamanoPagina = tamanoPagina,
            TotalRegistros = totalRegistros
        };
    }


    /// <inheritdoc />
    public async Task<UsuarioDto?> ObtenerPorIdAsync(int usuarioId, CancellationToken cancelacion = default)
    {
        await using var contexto = await _fabricaContextos.CreateDbContextAsync(cancelacion);

        return await contexto.Usuarios
            .AsNoTracking()
            .Where(usuario => usuario.UsuarioId == usuarioId)
            .Select(usuario => new UsuarioDto
            {
                UsuarioId = usuario.UsuarioId,
                Nombre = usuario.Nombre,
                ApellidoPaterno = usuario.ApellidoPaterno,
                ApellidoMaterno = usuario.ApellidoMaterno,
                Correo = usuario.Correo,
                Telefono = usuario.Telefono,
                FechaNacimiento = usuario.FechaNacimiento,
                Activo = usuario.Activo,
                TipoUsuarioId = usuario.TipoUsuarioId,
                TipoUsuarioNombre = usuario.TipoUsuario != null ? usuario.TipoUsuario.Nombre : null
            })
            .FirstOrDefaultAsync(cancelacion);
    }

    /// <inheritdoc />
    public async Task<Usuario?> ObtenerEntidadPorIdAsync(int usuarioId, CancellationToken cancelacion = default)
    {
        await using var contexto = await _fabricaContextos.CreateDbContextAsync(cancelacion);

        return await contexto.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(usuario => usuario.UsuarioId == usuarioId, cancelacion);
    }

    /// <inheritdoc />
    public async Task<bool> ExisteCorreoAsync(
        string correo,
        int? excluirUsuarioId = null,
        CancellationToken cancelacion = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(correo);

        await using var contexto = await _fabricaContextos.CreateDbContextAsync(cancelacion);

        var consulta = contexto.Usuarios.AsNoTracking();

        if (excluirUsuarioId.HasValue)
        {
            var idExcluido = excluirUsuarioId.Value;

            return await consulta.AnyAsync(
                usuario => usuario.Correo == correo && usuario.UsuarioId != idExcluido,
                cancelacion);
        }

        return await consulta.AnyAsync(usuario => usuario.Correo == correo, cancelacion);
    }

    /// <inheritdoc />
    public async Task<int> AgregarAsync(Usuario usuario, CancellationToken cancelacion = default)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        await using var contexto = await _fabricaContextos.CreateDbContextAsync(cancelacion);

        await contexto.Usuarios.AddAsync(usuario, cancelacion);
        await contexto.SaveChangesAsync(cancelacion);

        _registro.LogInformation(
            "Usuario {UsuarioId} registrado con el correo {Correo}.",
            usuario.UsuarioId,
            usuario.Correo);

        return usuario.UsuarioId;
    }

    /// <inheritdoc />
    public async Task<bool> ActualizarAsync(Usuario usuario, CancellationToken cancelacion = default)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        await using var contexto = await _fabricaContextos.CreateDbContextAsync(cancelacion);

        var existente = await contexto.Usuarios
            .FirstOrDefaultAsync(actual => actual.UsuarioId == usuario.UsuarioId, cancelacion);

        if (existente is null)
        {
            _registro.LogWarning(
                "No se encontró el usuario {UsuarioId} para actualizar.",
                usuario.UsuarioId);

            return false;
        }

        existente.Nombre = usuario.Nombre;
        existente.ApellidoPaterno = usuario.ApellidoPaterno;
        existente.ApellidoMaterno = usuario.ApellidoMaterno;
        existente.Correo = usuario.Correo;
        existente.Telefono = usuario.Telefono;
        existente.FechaNacimiento = usuario.FechaNacimiento;
        existente.Activo = usuario.Activo;
        existente.TipoUsuarioId = usuario.TipoUsuarioId;

        await contexto.SaveChangesAsync(cancelacion);

        _registro.LogInformation("Usuario {UsuarioId} actualizado correctamente.", usuario.UsuarioId);

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> EliminarAsync(int usuarioId, CancellationToken cancelacion = default)
    {
        await using var contexto = await _fabricaContextos.CreateDbContextAsync(cancelacion);

        var existente = await contexto.Usuarios
            .FirstOrDefaultAsync(usuario => usuario.UsuarioId == usuarioId, cancelacion);

        if (existente is null)
        {
            _registro.LogWarning("No se encontró el usuario {UsuarioId} para eliminar.", usuarioId);

            return false;
        }

        contexto.Usuarios.Remove(existente);
        await contexto.SaveChangesAsync(cancelacion);

        _registro.LogInformation("Usuario {UsuarioId} eliminado correctamente.", usuarioId);

        return true;
    }

    /// <inheritdoc />
    public async Task<EstadisticasUsuariosDto> ObtenerEstadisticasAsync(CancellationToken cancelacion = default)
    {
        await using var contexto = await _fabricaContextos.CreateDbContextAsync(cancelacion);

        var total = await contexto.Usuarios.AsNoTracking().CountAsync(cancelacion);
        var activos = await contexto.Usuarios.AsNoTracking().CountAsync(usuario => usuario.Activo, cancelacion);

        var fechasNacimiento = await contexto.Usuarios
            .AsNoTracking()
            .Select(usuario => usuario.FechaNacimiento)
            .ToListAsync(cancelacion);

        var edadPromedio = fechasNacimiento.Count == 0
            ? 0m
            : Math.Round((decimal)fechasNacimiento.Average(CalculadoraEdad.Calcular), 1);

        return new EstadisticasUsuariosDto
        {
            TotalUsuarios = total,
            UsuariosActivos = activos,
            EdadPromedio = edadPromedio
        };
    }

    private static IQueryable<Usuario> AplicarFiltros(
        IQueryable<Usuario> consulta,
        string? texto,
        bool? activo,
        int? tipoUsuarioId,
        bool soloSinTipoAsignado)
    {
        if (!string.IsNullOrWhiteSpace(texto))
        {
            var patron = $"%{EscaparParaLike(texto)}%";

            consulta = consulta.Where(usuario =>
                EF.Functions.Like(usuario.Nombre, patron, CaracterEscape)
                || EF.Functions.Like(usuario.ApellidoPaterno, patron, CaracterEscape)
                || (usuario.ApellidoMaterno != null
                    && EF.Functions.Like(usuario.ApellidoMaterno, patron, CaracterEscape))
                || EF.Functions.Like(usuario.Correo, patron, CaracterEscape));
        }

        if (activo.HasValue)
        {
            var estatus = activo.Value;

            consulta = consulta.Where(usuario => usuario.Activo == estatus);
        }

        if (soloSinTipoAsignado)
        {
            consulta = consulta.Where(usuario => usuario.TipoUsuarioId == null);
        }
        else if (tipoUsuarioId.HasValue)
        {
            var tipo = tipoUsuarioId.Value;

            consulta = consulta.Where(usuario => usuario.TipoUsuarioId == tipo);
        }

        return consulta;
    }

    private static string EscaparParaLike(string texto) =>
        texto
            .Replace(CaracterEscape, CaracterEscape + CaracterEscape, StringComparison.Ordinal)
            .Replace("%", CaracterEscape + "%", StringComparison.Ordinal)
            .Replace("_", CaracterEscape + "_", StringComparison.Ordinal);
}
