using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NotebookApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotebookApp.Controllers
{
    [Authorize]
    public class PrestamoController : Controller
    {
        private readonly NotebooksContext _context;

        public PrestamoController(NotebooksContext context)
        {
            _context = context;
        }

        // GET: Prestamo
        public async Task<IActionResult> Index()
        {
            var notebooksContext = _context.Prestamos.Include(p => p.Profesor)
                .Include(p => p.PrestamoDetalles)
                .ThenInclude(p => p.Equipo).ToListAsync();

            return View(await notebooksContext);
        }

        // GET: Prestamo/Create
        public async Task<IActionResult> Create(string? dni)
        {
            ViewBag.EquiposDisponibles = await _context.Equipos
                .Where(e => e.Disponibilidad)
                .ToListAsync();

            var prestamo = new Prestamo();

            if (!string.IsNullOrEmpty(dni))
            {
                var profesor = await _context.Profesores.FirstOrDefaultAsync(p => p.Dni == dni);

                if (profesor != null)
                {
                    prestamo.Profesor = profesor;
                    ViewBag.ProfesorEncontrado = true;
                    ViewBag.Mensaje = $"Profesor encontrado: {profesor.Nombre} {profesor.Apellido}";
                }
                else
                {
                    ViewBag.ProfesorEncontrado = false;
                    ViewBag.Mensaje = "❌ No se encontró un profesor con ese DNI.";
                }
            }

            return View(prestamo);
        }

        // POST: Prestamo/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Prestamo prestamo, List<int> equiposSeleccionados)
        {

            if (equiposSeleccionados == null || !equiposSeleccionados.Any())
            {
                ModelState.AddModelError("", "No se selecciono ningun equipo");
                ViewBag.Mensaje = "❌ No se selecciono ningun equipo.";
            }
            else
            {
                var profesorExistente = await _context.Profesores
                    .FirstOrDefaultAsync(p => p.Dni == prestamo.Profesor.Dni);

                if (profesorExistente == null)
                {
                    ModelState.AddModelError("", "El profesor ingresado no existe. No se puede registrar el préstamo.");
                }
                else
                {
                    if (ModelState.IsValid)
                    {
                        prestamo.ProfesorId = profesorExistente.ProfesorId;
                        prestamo.Profesor = profesorExistente;
                        prestamo.FechaSalida = DateTime.Now;

                        _context.Prestamos.Add(prestamo);
                        await _context.SaveChangesAsync();

                        foreach (var idEquipo in equiposSeleccionados)
                        {
                            var detalle = new PrestamoDetalle
                            {
                                PrestamoId = prestamo.PrestamoId,
                                EquipoId = idEquipo
                            };
                            _context.PrestamoDetalles.Add(detalle);

                            var equipo = await _context.Equipos.FindAsync(idEquipo);
                            if (equipo != null)
                            {
                                equipo.Disponibilidad = false;
                                _context.Update(equipo);
                            }
                        }

                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            return View(prestamo);
        }

        // GET: Prestamos/Restore/5
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null) return NotFound();

            var prestamo = await _context.Prestamos
                .Include(p => p.Profesor)
                .Include(p => p.PrestamoDetalles)
                    .ThenInclude(d => d.Equipo)
                .FirstOrDefaultAsync(p => p.PrestamoId == id);

            if (prestamo == null) return NotFound();

            prestamo.PrestamoDetalles = prestamo.PrestamoDetalles
                .Where(d => !d.Devuelto)
                .ToList();

            return View(prestamo);
        }

        // POST: Prestamos/Restore
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int PrestamoId, int[]? equiposSeleccionados)
        {
            var prestamo = await _context.Prestamos
                .Include(p => p.PrestamoDetalles)
                    .ThenInclude(d => d.Equipo)
                .FirstOrDefaultAsync(p => p.PrestamoId == PrestamoId);

            if (prestamo == null)
                return NotFound();

            if (equiposSeleccionados == null || equiposSeleccionados.Length == 0)
            {
                TempData["Error"] = "Debe seleccionar al menos un equipo para devolver.";
                return RedirectToAction(nameof(Restore), new { id = PrestamoId });
            }

            foreach (var detalle in prestamo.PrestamoDetalles)
            {
                if (equiposSeleccionados.Contains(detalle.EquipoId))
                {
                    detalle.Devuelto = true;
                    detalle.Equipo.Disponibilidad = true;
                }
            }

            if (prestamo.PrestamoDetalles.All(d => d.Devuelto))
            {
                prestamo.FechaEntrada = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PrestamoExists(int id)
        {
            return _context.Prestamos.Any(e => e.PrestamoId == id);
        }
    }
}