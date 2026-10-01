using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaUsuarios.Models;

namespace SistemaUsuarios.Data.Configurations;

/// <summary>
/// Configuración Fluent API de la entidad <see cref="Usuario"/> (tabla Usuarios).
/// </summary>
public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Usuarios");

        builder.HasKey(usuario => usuario.UsuarioId)
            .HasName("PK_Usuarios");

        builder.Property(usuario => usuario.UsuarioId)
            .ValueGeneratedOnAdd();

        builder.Property(usuario => usuario.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(usuario => usuario.ApellidoPaterno)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(usuario => usuario.ApellidoMaterno)
            .HasMaxLength(100);

        builder.Property(usuario => usuario.Correo)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(usuario => usuario.Telefono)
            .HasMaxLength(20);

        builder.Property(usuario => usuario.FechaNacimiento)
            .IsRequired();

        builder.Property(usuario => usuario.Activo)
            .IsRequired()
            .HasDefaultValue(true);

        // Relación con el catálogo de tipos de usuario (no permite eliminar un
        // tipo de usuario que tenga usuarios asignados).
        builder.HasOne(usuario => usuario.TipoUsuario)
            .WithMany(tipo => tipo.Usuarios)
            .HasForeignKey(usuario => usuario.TipoUsuarioId)
            .HasConstraintName("FK_Usuarios_TiposUsuario")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(usuario => usuario.TipoUsuarioId)
            .HasDatabaseName("IX_Usuarios_TipoUsuarioId");

        // Regla de negocio: el correo electrónico es único.
        builder.HasIndex(usuario => usuario.Correo)
            .IsUnique()
            .HasDatabaseName("IX_Usuarios_Correo");

        builder.HasIndex(usuario => new { usuario.ApellidoPaterno, usuario.Nombre })
            .HasDatabaseName("IX_Usuarios_NombreCompleto");
    }
}
