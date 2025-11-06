using System;
using System.Collections.Generic;

namespace NotebookApp.Models;

public partial class PrestamoDetalle
{
    public int PrestamoDetalleId { get; set; }

    public int PrestamoId { get; set; }

    public int EquipoId { get; set; }

    public virtual Equipo Equipo { get; set; } = null!;

    public virtual Prestamo Prestamo { get; set; } = null!;
}
