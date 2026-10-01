namespace SistemaUsuarios.Models.Comun;

/// <summary>
/// Representa el resultado de una operación de negocio ejecutada por la capa de servicios.
/// </summary>
public class ResultadoOperacion
{
    /// <summary>Indica si la operación finalizó correctamente.</summary>
    public bool Exito { get; init; }

    /// <summary>Mensaje descriptivo del resultado.</summary>
    public string Mensaje { get; init; } = string.Empty;

    /// <summary>Errores de validación agrupados por nombre de propiedad.</summary>
    public IReadOnlyDictionary<string, string[]> Errores { get; init; } =
        new Dictionary<string, string[]>(StringComparer.Ordinal);

    /// <summary>Indica si el resultado contiene errores de validación.</summary>
    public bool TieneErrores => Errores.Count > 0;

    /// <summary>Obtiene todos los mensajes de error en una sola secuencia.</summary>
    public IEnumerable<string> TodosLosErrores =>
        Errores.Values.SelectMany(mensajes => mensajes);

    /// <summary>Crea un resultado exitoso.</summary>
    public static ResultadoOperacion Ok(string mensaje) =>
        new() { Exito = true, Mensaje = mensaje };

    /// <summary>Crea un resultado fallido con un mensaje general.</summary>
    public static ResultadoOperacion Error(string mensaje) =>
        new() { Exito = false, Mensaje = mensaje };

    /// <summary>Crea un resultado fallido con errores de validación por campo.</summary>
    public static ResultadoOperacion Error(string mensaje, IDictionary<string, string[]> errores) =>
        new()
        {
            Exito = false,
            Mensaje = mensaje,
            Errores = new Dictionary<string, string[]>(errores, StringComparer.Ordinal)
        };
}
