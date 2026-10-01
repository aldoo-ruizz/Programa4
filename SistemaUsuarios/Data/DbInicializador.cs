using Microsoft.EntityFrameworkCore;
using SistemaUsuarios.Models;

namespace SistemaUsuarios.Data;

/// <summary>
/// Inicializador de la base de datos: aplica las migraciones pendientes
/// (creando el archivo usuarios.db cuando no existe) y carga datos de ejemplo.
/// </summary>
public static class DbInicializador
{
    /// <summary>
    /// Aplica las migraciones de Entity Framework Core y carga la información
    /// inicial del sistema.
    /// </summary>
    /// <param name="servicios">Proveedor de servicios de la aplicación.</param>
    /// <param name="registro">Logger utilizado para trazar el proceso.</param>
    /// <param name="cancelacion">Token de cancelación.</param>
    public static async Task InicializarAsync(
        IServiceProvider servicios,
        ILogger registro,
        CancellationToken cancelacion = default)
    {
        ArgumentNullException.ThrowIfNull(servicios);
        ArgumentNullException.ThrowIfNull(registro);

        await using var alcance = servicios.CreateAsyncScope();

        var fabrica = alcance.ServiceProvider
            .GetRequiredService<IDbContextFactory<ApplicationDbContext>>();

        await using var contexto = await fabrica.CreateDbContextAsync(cancelacion);

        try
        {
            var pendientes = (await contexto.Database.GetPendingMigrationsAsync(cancelacion)).ToList();

            if (pendientes.Count > 0)
            {
                registro.LogInformation(
                    "Aplicando {Cantidad} migración(es) pendiente(s): {Migraciones}",
                    pendientes.Count,
                    string.Join(", ", pendientes));
            }
            else
            {
                registro.LogInformation("La base de datos ya se encuentra actualizada.");
            }

            // Crea el archivo usuarios.db (si no existe) y aplica las migraciones.
            await contexto.Database.MigrateAsync(cancelacion);

            registro.LogInformation(
                "Base de datos SQLite lista en: {Ruta}",
                contexto.Database.GetDbConnection().DataSource);

            await CargarTiposUsuarioInicialesAsync(contexto, registro, cancelacion);
            await CargarDatosInicialesAsync(contexto, registro, cancelacion);
            await AsignarTiposUsuarioAsync(contexto, registro, cancelacion);
        }
        catch (Exception excepcion)
        {
            registro.LogError(
                excepcion,
                "Ocurrió un error al inicializar la base de datos SQLite.");

            throw;
        }
    }

    private static async Task CargarDatosInicialesAsync(
        ApplicationDbContext contexto,
        ILogger registro,
        CancellationToken cancelacion)
    {
        if (await contexto.Usuarios.AnyAsync(cancelacion))
        {
            return;
        }

        var tiposUsuario = await contexto.TiposUsuario
            .AsNoTracking()
            .ToDictionaryAsync(tipo => tipo.Nombre, tipo => tipo.TipoUsuarioId, cancelacion);

        int? IdTipoUsuario(string nombre) =>
            tiposUsuario.TryGetValue(nombre, out var tipoUsuarioId) ? tipoUsuarioId : null;

        var usuarios = new List<Usuario>
        {
            new()
            {
                Nombre = "Ana María",
                ApellidoPaterno = "García",
                ApellidoMaterno = "López",
                Correo = "ana.garcia@ejemplo.com",
                Telefono = "5512345678",
                FechaNacimiento = new DateTime(1992, 4, 18, 0, 0, 0, DateTimeKind.Unspecified),
                Activo = true,
                TipoUsuarioId = IdTipoUsuario("Administrador")
            },
            new()
            {
                Nombre = "Luis Fernando",
                ApellidoPaterno = "Hernández",
                ApellidoMaterno = "Martínez",
                Correo = "luis.hernandez@ejemplo.com",
                Telefono = "5598765432",
                FechaNacimiento = new DateTime(1988, 11, 2, 0, 0, 0, DateTimeKind.Unspecified),
                Activo = true,
                TipoUsuarioId = IdTipoUsuario("Docente")
            },
            new()
            {
                Nombre = "Sofía",
                ApellidoPaterno = "Ramírez",
                ApellidoMaterno = "Castro",
                Correo = "sofia.ramirez@ejemplo.com",
                Telefono = "5544332211",
                FechaNacimiento = new DateTime(1997, 7, 25, 0, 0, 0, DateTimeKind.Unspecified),
                Activo = true,
                TipoUsuarioId = IdTipoUsuario("Alumno")
            },
            new()
            {
                Nombre = "Carlos",
                ApellidoPaterno = "Domínguez",
                ApellidoMaterno = "Santos",
                Correo = "carlos.dominguez@ejemplo.com",
                Telefono = "5511223344",
                FechaNacimiento = new DateTime(1980, 2, 9, 0, 0, 0, DateTimeKind.Unspecified),
                Activo = false,
                TipoUsuarioId = IdTipoUsuario("Invitado")
            }
        };

        contexto.Usuarios.AddRange(usuarios);
        await contexto.SaveChangesAsync(cancelacion);

        registro.LogInformation(
            "Se cargaron {Cantidad} usuarios de ejemplo en la base de datos.",
            usuarios.Count);
    }

    private static async Task CargarTiposUsuarioInicialesAsync(
        ApplicationDbContext contexto,
        ILogger registro,
        CancellationToken cancelacion)
    {
        if (await contexto.TiposUsuario.AnyAsync(cancelacion))
        {
            return;
        }

        var tiposUsuario = new List<TipoUsuario>
        {
            new()
            {
                Nombre = "Administrador",
                Descripcion = "Acceso total al sistema, incluida la administración del padrón de usuarios.",
                Activo = true
            },
            new()
            {
                Nombre = "Docente",
                Descripcion = "Consulta del padrón y registro de usuarios con perfil académico.",
                Activo = true
            },
            new()
            {
                Nombre = "Alumno",
                Descripcion = "Acceso de solo consulta a la información registrada.",
                Activo = true
            },
            new()
            {
                Nombre = "Invitado",
                Descripcion = "Tipo de usuario temporal para accesos esporádicos.",
                Activo = false
            }
        };

        contexto.TiposUsuario.AddRange(tiposUsuario);
        await contexto.SaveChangesAsync(cancelacion);

        registro.LogInformation(
            "Se cargaron {Cantidad} tipos de usuario de ejemplo en la base de datos.",
            tiposUsuario.Count);
    }

    /// <summary>
    /// Asigna de forma automática un tipo de usuario a los registros que no lo
    /// tengan definido (por ejemplo, datos creados antes de agregar la relación).
    /// La asignación es determinista: se distribuyen los usuarios entre los tipos
    /// existentes según su posición.
    /// </summary>
    private static async Task AsignarTiposUsuarioAsync(
        ApplicationDbContext contexto,
        ILogger registro,
        CancellationToken cancelacion)
    {
        var tiposDisponibles = await contexto.TiposUsuario
            .AsNoTracking()
            .OrderBy(tipo => tipo.TipoUsuarioId)
            .Select(tipo => tipo.TipoUsuarioId)
            .ToListAsync(cancelacion);

        if (tiposDisponibles.Count == 0)
        {
            return;
        }

        var usuariosSinTipo = await contexto.Usuarios
            .Where(usuario => usuario.TipoUsuarioId == null)
            .OrderBy(usuario => usuario.UsuarioId)
            .ToListAsync(cancelacion);

        if (usuariosSinTipo.Count == 0)
        {
            return;
        }

        for (var indice = 0; indice < usuariosSinTipo.Count; indice++)
        {
            usuariosSinTipo[indice].TipoUsuarioId = tiposDisponibles[indice % tiposDisponibles.Count];
        }

        await contexto.SaveChangesAsync(cancelacion);

        registro.LogInformation(
            "Se asignó automáticamente un tipo de usuario a {Cantidad} usuario(s) que no lo tenían definido.",
            usuariosSinTipo.Count);
    }
}
