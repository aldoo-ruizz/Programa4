using Microsoft.EntityFrameworkCore;
using SistemaUsuarios.Models;

namespace SistemaUsuarios.Data;

/// <summary>
/// Contexto de datos de Entity Framework Core para el Sistema de Gestión de Usuarios.
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>Inicializa el contexto con las opciones configuradas en la inyección de dependencias.</summary>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>Tabla de usuarios registrados.</summary>
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    /// <summary>Catálogo de tipos (perfiles) de usuario.</summary>
    public DbSet<TipoUsuario> TiposUsuario => Set<TipoUsuario>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica todas las clases IEntityTypeConfiguration del ensamblado.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
