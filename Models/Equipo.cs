using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NotebookApp.Models;

public partial class Equipo
{
    public int EquipoId { get; set; }

    [Required(ErrorMessage = "El número de inventario es obligatorio.")]
    public string NumeroInventario { get; set; } = null!;

    public string? Marca { get; set; }

    public string? Modelo { get; set; }

    public bool Disponibilidad { get; set; } = true;

    public virtual ICollection<PrestamoDetalle> PrestamoDetalles { get; set; } = new List<PrestamoDetalle>();
}
