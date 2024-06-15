using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using amazon.Models;
using Microsoft.EntityFrameworkCore;


namespace amazon.Controllers
{
    public class ServicioRealizadoController : Controller
    {
        private readonly DbamazonContext _context;

        public ServicioRealizadoController(DbamazonContext context)
        {
            _context = context;
        }

        // GET: ServicioRealizado
        public IActionResult Index()
        {
            var serviciosRealizados = _context.ServicioRealizado
                .Include(s => s.Servicio)
                .Include(s => s.Usuario)
                .ToList();
            return View(serviciosRealizados);
        }

        // GET: ServicioRealizado/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var servicioRealizado = _context.ServicioRealizado
                .Include(s => s.Servicio)
                .Include(s => s.Usuario)
                .FirstOrDefault(m => m.Id == id);

            if (servicioRealizado == null)
            {
                return NotFound();
            }

            return View(servicioRealizado);
        }

        // GET: ServicioRealizado/Create
        public IActionResult Create()
        {
            ViewBag.ServicioId = new SelectList(_context.Servicios, "Id", "Nombre");
            ViewBag.UsuarioId = new SelectList(_context.Usuarios, "Id", "Nombre");
            return View();
        }

        // POST: ServicioRealizado/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,ServicioId,UsuarioId,Precio,Fecha")] ServicioRealizado servicioRealizado)
        {
            if (ModelState.IsValid)
            {
                _context.Add(servicioRealizado);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.ServicioId = new SelectList(_context.Servicios, "Id", "Nombre", servicioRealizado.ServicioId);
            ViewBag.UsuarioId = new SelectList(_context.Usuarios, "Id", "Nombre", servicioRealizado.UsuarioId);
            return View(servicioRealizado);
        }

        // GET: ServicioRealizado/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var servicioRealizado = _context.ServicioRealizado.Find(id);
            if (servicioRealizado == null)
            {
                return NotFound();
            }
            ViewBag.ServicioId = new SelectList(_context.Servicios, "Id", "Nombre", servicioRealizado.ServicioId);
            ViewBag.UsuarioId = new SelectList(_context.Usuarios, "Id", "Nombre", servicioRealizado.UsuarioId);
            return View(servicioRealizado);
        }

        // POST: ServicioRealizado/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id,ServicioId,UsuarioId,Precio,Fecha")] ServicioRealizado servicioRealizado)
        {
            if (id != servicioRealizado.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(servicioRealizado);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServicioRealizadoExists(servicioRealizado.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.ServicioId = new SelectList(_context.Servicios, "Id", "Nombre", servicioRealizado.ServicioId);
            ViewBag.UsuarioId = new SelectList(_context.Usuarios, "Id", "Nombre", servicioRealizado.UsuarioId);
            return View(servicioRealizado);
        }

        // GET: ServicioRealizado/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var servicioRealizado = _context.ServicioRealizado
                .Include(s => s.Servicio)
                .Include(s => s.Usuario)
                .FirstOrDefault(m => m.Id == id);

            if (servicioRealizado == null)
            {
                return NotFound();
            }

            return View(servicioRealizado);
        }

        // POST: ServicioRealizado/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var servicioRealizado = _context.ServicioRealizado.Find(id);
            _context.ServicioRealizado.Remove(servicioRealizado);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        private bool ServicioRealizadoExists(int id)
        {
            return _context.ServicioRealizado.Any(e => e.Id == id);
        }
    }
}
