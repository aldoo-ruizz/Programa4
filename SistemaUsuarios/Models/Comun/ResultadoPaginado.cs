namespace SistemaUsuarios.Models.Comun;

/// <summary>
/// Resultado paginado utilizado por las consultas de listado.
/// </summary>
/// <typeparam name="T">Tipo de los elementos de la página.</typeparam>
public sealed class ResultadoPaginado<T>
{
    /// <summary>Elementos que pertenecen a la página solicitada.</summary>
    public IReadOnlyList<T> Elementos { get; init; } = Array.Empty<T>();

    /// <summary>Número de la página actual (base 1).</summary>
    public int Pagina { get; init; } = 1;

    /// <summary>Cantidad máxima de registros por página.</summary>
    public int TamanoPagina { get; init; } = 10;

    /// <summary>Total de registros que cumplen el filtro.</summary>
    public int TotalRegistros { get; init; }

    /// <summary>Total de páginas disponibles.</summary>
    public int TotalPaginas =>
        TamanoPagina <= 0 ? 0 : (int)Math.Ceiling(TotalRegistros / (double)TamanoPagina);

    /// <summary>Indica si existe una página anterior.</summary>
    public bool TienePaginaAnterior => Pagina > 1;

    /// <summary>Indica si existe una página siguiente.</summary>
    public bool TienePaginaSiguiente => Pagina < TotalPaginas;

    /// <summary>Indica si el resultado no contiene elementos.</summary>
    public bool EstaVacio => Elementos.Count == 0;

    /// <summary>Crea un resultado vacío para el tamaño de página indicado.</summary>
    public static ResultadoPaginado<T> Vacio(int tamanoPagina) =>
        new() { Pagina = 1, TamanoPagina = tamanoPagina };
}
