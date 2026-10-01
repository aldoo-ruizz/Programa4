using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SistemaUsuarios.Data;

/// <summary>
/// Fábrica de contexto utilizada exclusivamente por las herramientas de línea de
/// comandos de Entity Framework Core (<c>dotnet ef migrations</c> /
/// <c>dotnet ef database update</c>).
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    /// <inheritdoc />
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var cadenaConexion = ConfiguracionBaseDatos.NormalizarCadenaConexion(
            $"Data Source={ConfiguracionBaseDatos.NombreArchivoPredeterminado}",
            Directory.GetCurrentDirectory());

        var opciones = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(cadenaConexion)
            .Options;

        return new ApplicationDbContext(opciones);
    }
}
