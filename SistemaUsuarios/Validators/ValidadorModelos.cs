using System.ComponentModel.DataAnnotations;

namespace SistemaUsuarios.Validators;

/// <summary>
/// Validador central de los modelos de transferencia (DTO) del sistema.
/// Ejecuta las reglas declarativas (DataAnnotations) y normaliza los datos
/// antes de persistirlos.
/// </summary>
public static class ValidadorModelos
{
    /// <summary>
    /// Ejecuta las reglas de validación declarativas del modelo recibido.
    /// </summary>
    /// <param name="modelo">Instancia del DTO a validar.</param>
    /// <returns>Diccionario de errores agrupados por nombre de propiedad.</returns>
    public static Dictionary<string, string[]> Validar(object modelo)
    {
        ArgumentNullException.ThrowIfNull(modelo);

        var resultados = new List<ValidationResult>();
        var contexto = new ValidationContext(modelo);

        var esValido = Validator.TryValidateObject(
            modelo,
            contexto,
            resultados,
            validateAllProperties: true);

        if (esValido)
        {
            return new Dictionary<string, string[]>(StringComparer.Ordinal);
        }

        var errores = new Dictionary<string, string[]>(StringComparer.Ordinal);

        foreach (var resultado in resultados)
        {
            var mensaje = resultado.ErrorMessage ?? "El valor capturado no es válido.";

            if (resultado.MemberNames is null || !resultado.MemberNames.Any())
            {
                Agregar(errores, string.Empty, mensaje);
                continue;
            }

            foreach (var miembro in resultado.MemberNames)
            {
                Agregar(errores, miembro, mensaje);
            }
        }

        return errores;
    }

    /// <summary>
    /// Normaliza el correo electrónico (sin espacios y en minúsculas) para
    /// garantizar la unicidad del dato.
    /// </summary>
    public static string? NormalizarCorreo(string? correo) =>
        string.IsNullOrWhiteSpace(correo) ? null : correo.Trim().ToLowerInvariant();

    /// <summary>
    /// Normaliza el teléfono eliminando espacios, guiones y paréntesis.
    /// </summary>
    public static string? NormalizarTelefono(string? telefono)
    {
        if (string.IsNullOrWhiteSpace(telefono))
        {
            return null;
        }

        var caracteres = telefono.Where(char.IsDigit).ToArray();
        return caracteres.Length == 0 ? null : new string(caracteres);
    }

    /// <summary>
    /// Normaliza el texto de captura eliminando espacios al inicio y al final.
    /// </summary>
    public static string NormalizarTexto(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? string.Empty : valor.Trim();

    /// <summary>
    /// Normaliza un texto opcional, devolviendo <c>null</c> cuando queda vacío.
    /// </summary>
    public static string? NormalizarTextoOpcional(string? valor)
    {
        var normalizado = NormalizarTexto(valor);
        return normalizado.Length == 0 ? null : normalizado;
    }

    /// <summary>
    /// Normaliza un nombre propio o de catálogo: elimina espacios extremos y
    /// reduce los espacios internos repetidos a uno solo.
    /// </summary>
    public static string NormalizarNombre(string? valor)
    {
        var normalizado = NormalizarTexto(valor);

        if (normalizado.Length == 0)
        {
            return string.Empty;
        }

        return string.Join(' ', normalizado.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static void Agregar(IDictionary<string, string[]> errores, string clave, string mensaje)
    {
        if (errores.TryGetValue(clave, out var existentes))
        {
            errores[clave] = existentes.Append(mensaje).ToArray();
            return;
        }

        errores[clave] = new[] { mensaje };
    }
}
