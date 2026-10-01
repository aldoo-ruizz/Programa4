using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaUsuarios.Models;

namespace SistemaUsuarios.Data.Configurations;

/// <summary>
/// Configuración Fluent API de la entidad <see cref="TipoUsuario"/> (tabla TiposUsuario).
/// </summary>
public sealed class TipoUsuarioConfiguration : IEntityTypeConfiguration<TipoUsuario>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TipoUsuario> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("TiposUsuario");

        builder.HasKey(tipo => tipo.TipoUsuarioId)
            .HasName("PK_TiposUsuario");

        builder.Property(tipo => tipo.TipoUsuarioId)
            .ValueGeneratedOnAdd();

        // Collation NOCASE: la unicidad del nombre se valida sin distinguir
        // mayúsculas/minúsculas (Administrador = administrador).
        builder.Property(tipo => tipo.Nombre)
            .IsRequired()
            .HasMaxLength(60)
            .UseCollation("NOCASE");

        builder.Property(tipo => tipo.Descripcion)
            .HasMaxLength(250);

        builder.Property(tipo => tipo.Activo)
            .IsRequired()
            .HasDefaultValue(true);

        // Regla de negocio: el nombre del tipo de usuario es único.
        builder.HasIndex(tipo => tipo.Nombre)
            .IsUnique()
            .HasDatabaseName("IX_TiposUsuario_Nombre");
    }
}
