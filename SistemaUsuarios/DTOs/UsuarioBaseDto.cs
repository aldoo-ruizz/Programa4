using System.ComponentModel.DataAnnotations;
using SistemaUsuarios.Validators;

namespace SistemaUsuarios.DTOs;

/// <summary>
/// Propiedades y reglas de validación compartidas por los DTO de alta y edición.
/// </summary>
public abstract class UsuarioBaseDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, MinimumLength = 2,
        ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido paterno es obligatorio.")]
    [StringLength(100, MinimumLength = 2,
        ErrorMessage = "El apellido paterno debe tener entre 2 y 100 caracteres.")]
    public string ApellidoPaterno { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "El apellido materno no debe exceder 100 caracteres.")]
    public string? ApellidoMaterno { get; set; }

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [StringLength(150, ErrorMessage = "El correo no debe exceder 150 caracteres.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
    public string Correo { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "El teléfono no debe exceder 20 caracteres.")]
    [RegularExpression(@"^[\d\s\-\.\(\)\+]{10,20}$",
        ErrorMessage = "El teléfono debe contener 10 dígitos (puede incluir espacios, guiones o paréntesis).")]
    public string? Telefono { get; set; }

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    [DataType(DataType.Date)]
    [FechaNacimientoValida]
    public DateTime? FechaNacimiento { get; set; }

    [Required(ErrorMessage = "El tipo de usuario es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un tipo de usuario válido.")]
    public int? TipoUsuarioId { get; set; }

    public bool Activo { get; set; } = true;
}
