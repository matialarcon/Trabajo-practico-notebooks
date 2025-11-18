using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NotebookApp.Models;

public partial class Profesor
{
    public int ProfesorId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string Dni { get; set; } = null!;

    public virtual ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
}
