namespace SistemaUsuarios.Models.Comun;

/// <summary>
/// Resultado de una operación de negocio que además devuelve un dato de salida.
/// </summary>
/// <typeparam name="T">Tipo del dato devuelto por la operación.</typeparam>
public class ResultadoOperacion<T> : ResultadoOperacion
{
    /// <summary>Dato devuelto por la operación cuando esta es exitosa.</summary>
    public T? Datos { get; init; }

    /// <summary>Crea un resultado exitoso con datos.</summary>
    public static ResultadoOperacion<T> Ok(T datos, string mensaje) =>
        new() { Exito = true, Mensaje = mensaje, Datos = datos };

    /// <summary>Crea un resultado fallido con un mensaje general.</summary>
    public static new ResultadoOperacion<T> Error(string mensaje) =>
        new() { Exito = false, Mensaje = mensaje };

    /// <summary>Crea un resultado fallido con errores de validación por campo.</summary>
    public static new ResultadoOperacion<T> Error(string mensaje, IDictionary<string, string[]> errores) =>
        new()
        {
            Exito = false,
            Mensaje = mensaje,
            Errores = new Dictionary<string, string[]>(errores, StringComparer.Ordinal)
        };
}
