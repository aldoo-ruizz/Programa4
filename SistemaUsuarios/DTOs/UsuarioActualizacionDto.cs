using System.ComponentModel.DataAnnotations;

namespace SistemaUsuarios.DTOs;

/// <summary>
/// DTO utilizado para la edición de un usuario existente.
/// </summary>
public sealed class UsuarioActualizacionDto : UsuarioBaseDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El identificador del usuario no es válido.")]
    public int UsuarioId { get; set; }
}
