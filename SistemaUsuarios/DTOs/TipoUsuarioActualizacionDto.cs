using System.ComponentModel.DataAnnotations;

namespace SistemaUsuarios.DTOs;

/// <summary>
/// DTO utilizado para la edición de un tipo de usuario existente.
/// </summary>
public sealed class TipoUsuarioActualizacionDto : TipoUsuarioBaseDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El identificador del tipo de usuario no es válido.")]
    public int TipoUsuarioId { get; set; }
}
