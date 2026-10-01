namespace SistemaUsuarios.Models.Comun;

/// <summary>
/// Cálculos de fechas utilizados por las reglas de negocio del sistema.
/// </summary>
public static class CalculadoraEdad
{
    /// <summary>
    /// Calcula la edad exacta (en años cumplidos) a partir de la fecha de nacimiento.
    /// </summary>
    public static int Calcular(DateTime fechaNacimiento)
    {
        if (fechaNacimiento == default)
        {
            return 0;
        }

        var hoy = DateTime.Today;
        var edad = hoy.Year - fechaNacimiento.Year;

        if (fechaNacimiento.Date > hoy.AddYears(-edad))
        {
            edad--;
        }

        return edad < 0 ? 0 : edad;
    }
}
