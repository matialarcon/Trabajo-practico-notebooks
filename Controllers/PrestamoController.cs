using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NotebookApp.Models;

namespace NotebookApp.Controllers
{
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
        public async Task<IActionResult> Create()
        {
            ViewBag.equiposDisponibles = await _context.Equipos
               .Where(e => e.Disponibilidad)
               .ToListAsync();

            return View();
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
                ModelState.AddModelError("", "Debe seleccionar al menos un equipo.");
            }

            if (ModelState.IsValid)
            {
                // Buscar profesor por DNI
                var profesorExistente = await _context.Profesores
                    .FirstOrDefaultAsync(p => p.Dni == prestamo.Profesor.Dni);

                if (profesorExistente != null)
                {
                    prestamo.ProfesorId = profesorExistente.ProfesorId;
                    prestamo.Profesor = profesorExistente;
                }
                else
                {
                    // Crear nuevo profesor
                    var nuevoProfesor = new Profesor
                    {
                        Nombre = prestamo.Profesor.Nombre,
                        Apellido = prestamo.Profesor.Apellido,
                        Dni = prestamo.Profesor.Dni
                    };

                    _context.Profesores.Add(nuevoProfesor);
                    await _context.SaveChangesAsync();

                    prestamo.ProfesorId = nuevoProfesor.ProfesorId;
                    prestamo.Profesor = nuevoProfesor;
                }

                prestamo.FechaSalida = DateTime.Now;
                _context.Prestamos.Add(prestamo);
                await _context.SaveChangesAsync();
                foreach (var (idEquipo, detalle) in
                // Agregar detalles del préstamo
                from idEquipo in equiposSeleccionados
                let detalle = new PrestamoDetalle
                {
                    PrestamoId = prestamo.PrestamoId,
                    EquipoId = idEquipo
                }
                select (idEquipo, detalle))
                {
                    _context.PrestamoDetalles.Add(detalle);
                    // Marcar equipo como no disponible
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

            ViewBag.EquiposDisponibles = await _context.Equipos
                .Where(e => e.Disponibilidad)
                .ToListAsync();

            return View(prestamo);
        }

        // GET: Prestamos/Restore/5
        public async Task<IActionResult> Restore(int id)
        {
            var prestamo = await _context.Prestamos
                .Include(p => p.Profesor)
                .Include(p => p.PrestamoDetalles)
                    .ThenInclude(d => d.Equipo)
                .FirstOrDefaultAsync(p => p.PrestamoId == id);

            if (prestamo == null)
                return NotFound();

            return View(prestamo);
        }

        // POST: Prestamos/Restore
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            var prestamo = await _context.Prestamos
                .Include(p => p.PrestamoDetalles)
                .FirstOrDefaultAsync(p => p.PrestamoId == id);

            if (prestamo == null)
                return NotFound();

            prestamo.FechaEntrada = DateTime.Now;
            _context.Update(prestamo);

            // Marcar los equipos como disponibles
            foreach (var detalle in prestamo.PrestamoDetalles)
            {
                var equipo = await _context.Equipos.FindAsync(detalle.EquipoId);
                if (equipo != null)
                {
                    equipo.Disponibilidad = true;
                    _context.Update(equipo);
                }
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