using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NotebookApp.Models;

public partial class Profesor
{
    public int ProfesorId { get; set; }

    [Required(ErrorMessage = "El nombre del profesor es obligatorio.")]
    public string Nombre { get; set; } = null!;

    [Required(ErrorMessage = "El apellido del profesor es obligatorio.")]
    public string Apellido { get; set; } = null!;

    [Required(ErrorMessage = "El dni del profesor es obligatorio.")]
    public string Dni { get; set; } = null!;

    public virtual ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
}
