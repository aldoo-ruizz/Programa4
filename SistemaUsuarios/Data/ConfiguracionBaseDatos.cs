using Microsoft.Data.Sqlite;

namespace SistemaUsuarios.Data;

/// <summary>
/// Utilidades de configuración de la base de datos SQLite.
/// </summary>
public static class ConfiguracionBaseDatos
{
    /// <summary>Nombre del archivo de base de datos creado automáticamente.</summary>
    public const string NombreArchivoPredeterminado = "usuarios.db";

    /// <summary>Nombre de la cadena de conexión definida en appsettings.json.</summary>
    public const string NombreCadenaConexion = "UsuariosConnection";

    /// <summary>
    /// Normaliza la cadena de conexión de SQLite y convierte las rutas relativas
    /// en rutas absolutas respecto a la raíz de contenido de la aplicación, de
    /// forma que el archivo <c>usuarios.db</c> siempre se cree en el mismo lugar.
    /// </summary>
    /// <param name="cadenaConexion">Cadena configurada en appsettings.json.</param>
    /// <param name="rutaRaizContenido">Ruta raíz de contenido de la aplicación.</param>
    /// <returns>Cadena de conexión lista para usar con EF Core.</returns>
    public static string NormalizarCadenaConexion(string? cadenaConexion, string rutaRaizContenido)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaRaizContenido);

        var cadena = string.IsNullOrWhiteSpace(cadenaConexion)
            ? $"Data Source={NombreArchivoPredeterminado}"
            : cadenaConexion;

        var constructor = new SqliteConnectionStringBuilder(cadena);

        if (string.IsNullOrWhiteSpace(constructor.DataSource))
        {
            constructor.DataSource = NombreArchivoPredeterminado;
        }

        var esArchivo = !string.Equals(constructor.DataSource, ":memory:", StringComparison.OrdinalIgnoreCase)
            && !constructor.DataSource.StartsWith("file:", StringComparison.OrdinalIgnoreCase);

        if (esArchivo && !Path.IsPathRooted(constructor.DataSource))
        {
            var rutaCompleta = Path.GetFullPath(Path.Combine(rutaRaizContenido, constructor.DataSource));
            var directorio = Path.GetDirectoryName(rutaCompleta);

            if (!string.IsNullOrWhiteSpace(directorio))
            {
                Directory.CreateDirectory(directorio);
            }

            constructor.DataSource = rutaCompleta;
        }

        return constructor.ToString();
    }
}
