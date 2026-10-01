using Microsoft.EntityFrameworkCore;
using SistemaUsuarios.Data;
using SistemaUsuarios.DTOs;
using SistemaUsuarios.Models;
using SistemaUsuarios.Models.Comun;

namespace SistemaUsuarios.Repositories;

/// <summary>
/// Implementación del repositorio del catálogo de tipos de usuario sobre
/// Entity Framework Core y SQLite. Cada operación utiliza su propio
/// <see cref="ApplicationDbContext"/> creado a partir de la fábrica de contextos.
/// </summary>
public sealed class TipoUsuarioRepository : ITipoUsuarioRepository
{
    private const string CaracterEscape = "\\";

    private readonly IDbContextFactory<ApplicationDbContext> _fabricaContextos;
    private readonly ILogger<TipoUsuarioRepository> _registro;

    /// <summary>Inicializa el repositorio con la fábrica de contextos y el logger.</summary>
    public TipoUsuarioRepository(
        IDbContextFactory<ApplicationDbContext> fabricaContextos,
        ILogger<TipoUsuarioRepository> registro)
    {
        _fabricaContextos = fabricaContextos ?? throw new ArgumentNullException(nameof(fabricaContextos));
        _registro = registro ?? throw new ArgumentNullException(nameof(registro));
    }

    /// <inheritdoc />
    public async Task<ResultadoPaginado<TipoUsuarioDto>> ObtenerPaginadoAsync(
        TipoUsuarioFiltroDto filtro,
        CancellationToken cancelacion = default)
    {
        ArgumentNullException.ThrowIfNull(filtro);

        var pagina = filtro.PaginaNormalizada;
        var tamanoPagina = filtro.TamanoPaginaNormalizado;

        await using var contexto = await _fabricaContextos.CreateDbContextAsync(cancelacion);

        var consulta = AplicarFiltros(
            contexto.TiposUsuario.AsNoTracking(),
            filtro.TextoNormalizado,
            filtro.Activo);

        var totalRegistros = await consulta.CountAsync(cancelacion);

        var elementos = await consulta
            .OrderBy(tipo => tipo.Nombre)
            .ThenBy(tipo => tipo.TipoUsuarioId)
            .Skip(filtro.RegistrosOmitidos)
            .Take(tamanoPagina)
            .Select(tipo => new TipoUsuarioDto
            {
                TipoUsuarioId = tipo.TipoUsuarioId,
                Nombre = tipo.Nombre,
                Descripcion = tipo.Descripcion,
                Activo = tipo.Activo,
                UsuariosAsignados = tipo.Usuarios.Count
            })
            .ToListAsync(cancelacion);

        _registro.LogDebug(
            "Consulta de tipos de usuario: {Total} registro(s) encontrados en la página {Pagina}.",
            totalRegistros,
            pagina);

        return new ResultadoPaginado<TipoUsuarioDto>
        {
            Elementos = elementos,
            Pagina = pagina,
            TamanoPagina = tamanoPagina,
            TotalRegistros = totalRegistros
        };
    }

    /// <inheritdoc />
    public async Task<TipoUsuarioDto?> ObtenerPorIdAsync(int tipoUsuarioId, CancellationToken cancelacion = default)
    {
        await using var contexto = await _fabricaContextos.CreateDbContextAsync(cancelacion);

        return await contexto.TiposUsuario
            .AsNoTracking()
            .Where(tipo => tipo.TipoUsuarioId == tipoUsuarioId)
            .Select(tipo => new TipoUsuarioDto
            {
                TipoUsuarioId = tipo.TipoUsuarioId,
                Nombre = tipo.Nombre,
                Descripcion = tipo.Descripcion,
                Activo = tipo.Activo,
                UsuariosAsignados = tipo.Usuarios.Count
            })
            .FirstOrDefaultAsync(cancelacion);
    }

    /// <inheritdoc />
    public async Task<TipoUsuario?> ObtenerEntidadPorIdAsync(int tipoUsuarioId, CancellationToken cancelacion = default)
    {
        await using var contexto = await _fabricaContextos.CreateDbContextAsync(cancelacion);

        return await contexto.TiposUsuario
            .AsNoTracking()
            .FirstOrDefaultAsync(tipo => tipo.TipoUsuarioId == tipoUsuarioId, cancelacion);
    }

    /// <inheritdoc />
    public async Task<bool> ExisteNombreAsync(
        string nombre,
        int? excluirTipoUsuarioId = null,
        CancellationToken cancelacion = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);

        await using var contexto = await _fabricaContextos.CreateDbContextAsync(cancelacion);

        // La columna Nombre usa collation NOCASE (comparación sin distinguir mayúsculas).
        var consulta = contexto.TiposUsuario.AsNoTracking();

        if (excluirTipoUsuarioId.HasValue)
        {
            var idExcluido = excluirTipoUsuarioId.Value;

            return await consulta.AnyAsync(
                tipo => tipo.Nombre == nombre && tipo.TipoUsuarioId != idExcluido,
                cancelacion);
        }

        return await consulta.AnyAsync(tipo => tipo.Nombre == nombre, cancelacion);
    }

    /// <inheritdoc />
    public async Task<int> AgregarAsync(TipoUsuario tipoUsuario, CancellationToken cancelacion = default)
    {
        ArgumentNullException.ThrowIfNull(tipoUsuario);

        await using var contexto = await _fabricaContextos.CreateDbContextAsync(cancelacion);

        await contexto.TiposUsuario.AddAsync(tipoUsuario, cancelacion);
        await contexto.SaveChangesAsync(cancelacion);

        _registro.LogInformation(
            "Tipo de usuario {TipoUsuarioId} registrado con el nombre {Nombre}.",
            tipoUsuario.TipoUsuarioId,
            tipoUsuario.Nombre);

        return tipoUsuario.TipoUsuarioId;
    }

    /// <inheritdoc />
    public async Task<bool> ActualizarAsync(TipoUsuario tipoUsuario, CancellationToken cancelacion = default)
    {
        ArgumentNullException.ThrowIfNull(tipoUsuario);

        await using var contexto = await _fabricaContextos.CreateDbContextAsync(cancelacion);

        var existente = await contexto.TiposUsuario
            .FirstOrDefaultAsync(actual => actual.TipoUsuarioId == tipoUsuario.TipoUsuarioId, cancelacion);

        if (existente is null)
        {
            _registro.LogWarning(
                "No se encontró el tipo de usuario {TipoUsuarioId} para actualizar.",
                tipoUsuario.TipoUsuarioId);

            return false;
        }

        existente.Nombre = tipoUsuario.Nombre;
        existente.Descripcion = tipoUsuario.Descripcion;
        existente.Activo = tipoUsuario.Activo;

        await contexto.SaveChangesAsync(cancelacion);

        _registro.LogInformation(
            "Tipo de usuario {TipoUsuarioId} actualizado correctamente.",
            tipoUsuario.TipoUsuarioId);

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> EliminarAsync(int tipoUsuarioId, CancellationToken cancelacion = default)
    {
        await using var contexto = await _fabricaContextos.CreateDbContextAsync(cancelacion);

        var existente = await contexto.TiposUsuario
            .FirstOrDefaultAsync(tipo => tipo.TipoUsuarioId == tipoUsuarioId, cancelacion);

        if (existente is null)
        {
            _registro.LogWarning(
                "No se encontró el tipo de usuario {TipoUsuarioId} para eliminar.",
                tipoUsuarioId);

            return false;
        }

        contexto.TiposUsuario.Remove(existente);
        await contexto.SaveChangesAsync(cancelacion);

        _registro.LogInformation("Tipo de usuario {TipoUsuarioId} eliminado correctamente.", tipoUsuarioId);

        return true;
    }

    /// <inheritdoc />
    public async Task<int> ContarAsync(CancellationToken cancelacion = default)
    {
        await using var contexto = await _fabricaContextos.CreateDbContextAsync(cancelacion);

        return await contexto.TiposUsuario.AsNoTracking().CountAsync(cancelacion);
    }

    /// <inheritdoc />
    public async Task<int> ContarUsuariosAsignadosAsync(int tipoUsuarioId, CancellationToken cancelacion = default)
    {
        await using var contexto = await _fabricaContextos.CreateDbContextAsync(cancelacion);

        return await contexto.Usuarios
            .AsNoTracking()
            .CountAsync(usuario => usuario.TipoUsuarioId == tipoUsuarioId, cancelacion);
    }

    private static IQueryable<TipoUsuario> AplicarFiltros(
        IQueryable<TipoUsuario> consulta,
        string? texto,
        bool? activo)
    {
        if (!string.IsNullOrWhiteSpace(texto))
        {
            var patron = $"%{EscaparParaLike(texto)}%";

            consulta = consulta.Where(tipo =>
                EF.Functions.Like(tipo.Nombre, patron, CaracterEscape)
                || (tipo.Descripcion != null
                    && EF.Functions.Like(tipo.Descripcion, patron, CaracterEscape)));
        }

        if (activo.HasValue)
        {
            var estatus = activo.Value;

            consulta = consulta.Where(tipo => tipo.Activo == estatus);
        }

        return consulta;
    }

    private static string EscaparParaLike(string texto) =>
        texto
            .Replace(CaracterEscape, CaracterEscape + CaracterEscape, StringComparison.Ordinal)
            .Replace("%", CaracterEscape + "%", StringComparison.Ordinal)
            .Replace("_", CaracterEscape + "_", StringComparison.Ordinal);
}

