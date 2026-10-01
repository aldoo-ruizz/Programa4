using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace SistemaUsuarios.Validators;

/// <summary>
/// Regla de negocio: la fecha de nacimiento no puede ser futura y debe ser
/// posterior al año mínimo configurado (1900 por omisión).
/// Los valores nulos se consideran válidos para permitir que la regla
/// <see cref="RequiredAttribute"/> genere el mensaje de obligatoriedad.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
    AllowMultiple = false)]
public sealed class FechaNacimientoValidaAttribute : ValidationAttribute
{
    /// <summary>Año mínimo aceptado para una fecha de nacimiento.</summary>
    public int AnioMinimo { get; set; } = 1900;

    /// <inheritdoc />
    public override bool IsValid(object? value)
    {
        if (value is null)
        {
            // La obligatoriedad se valida con [Required] para no duplicar mensajes.
            return true;
        }

        if (value is not DateTime fecha)
        {
            return false;
        }

        var fechaNormalizada = fecha.Date;

        return fechaNormalizada.Year >= AnioMinimo && fechaNormalizada <= DateTime.Today;
    }

    /// <inheritdoc />
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (IsValid(value))
        {
            return ValidationResult.Success;
        }

        var miembros = string.IsNullOrEmpty(validationContext.MemberName)
            ? null
            : new[] { validationContext.MemberName };

        return new ValidationResult(
            $"La fecha de nacimiento debe estar entre el 01/01/{AnioMinimo.ToString(CultureInfo.InvariantCulture)} y la fecha actual.",
            miembros);
    }
}
