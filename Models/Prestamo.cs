using System;
using System.Collections.Generic;

namespace NotebookApp.Models;

public partial class Prestamo
{
    public int PrestamoId { get; set; }

    public int ProfesorId { get; set; }

    public DateTime FechaSalida { get; set; }

    public DateTime? FechaEntrada { get; set; }

    public virtual ICollection<PrestamoDetalle> PrestamoDetalles { get; set; } = new List<PrestamoDetalle>();

    public virtual Profesor Profesor { get; set; } = null!;
}
