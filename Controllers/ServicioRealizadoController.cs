using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using amazon.Models;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

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
                .ToList();

            return View(serviciosRealizados);
        }

        
        // GET: ServicioRealizado/Create
        public IActionResult Create()
        {
            

            return View();
        }

        // POST: ServicioRealizado/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,ServicioId,UsuarioId,Precio,Fecha,Estado")] ServicioRealizado servicioRealizado)
        {
            if (ModelState.IsValid)
            {
                _context.Add(servicioRealizado);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
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
                .FirstOrDefault(sr => sr.id == id);

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
            if (servicioRealizado == null)
            {
                return NotFound();
            }

            _context.ServicioRealizado.Remove(servicioRealizado);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        private bool ServicioRealizadoExists(int id)
        {
            return _context.ServicioRealizado.Any(sr => sr.id == id);
        }
    }
}
