using System.ComponentModel.DataAnnotations;

namespace SistemaUsuarios.DTOs;

/// <summary>
/// Propiedades y reglas de validación compartidas por los DTO de alta y edición
/// de tipos de usuario.
/// </summary>
public abstract class TipoUsuarioBaseDto
{
    [Required(ErrorMessage = "El nombre del tipo de usuario es obligatorio.")]
    [StringLength(60, MinimumLength = 3,
        ErrorMessage = "El nombre del tipo de usuario debe tener entre 3 y 60 caracteres.")]
    [RegularExpression(@"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ0-9\s\.\-_]+$",
        ErrorMessage = "El nombre del tipo de usuario solo puede contener letras, números, espacios, puntos, guiones y guion bajo.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "La descripción no debe exceder 250 caracteres.")]
    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;
}
