using amazon.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TuNamespace.Models;

namespace amazon.Controllers
{
    public class FacturacionController : Controller
    {
        private readonly DbamazonContext _context;

        public FacturacionController(DbamazonContext context)
        {
            _context = context;
        }

        // GET: Facturacion
        public IActionResult Index()
        {
            var facturaciones = _context.Facturacion
                .Include(f => f.Usuario) // Incluir la relación con Usuario
                .ToList(); 

            return View(facturaciones);
        }

        // GET: Facturacion/Details/5
        public IActionResult Details(int id)
        {
            // Simular que encuentras la factura con el id proporcionado
            var facturacion = "aqui se mostrará detalle de factura"; // Mensaje simple para mostrar en el modal

            if (facturacion == null)
            {
                return NotFound();
            }

            // Retorna la vista parcial con el contenido
            return PartialView("_DetailsPartial", facturacion);
        }





        public IActionResult Create()
        {
            // Obtener los usuarios con rol = 3 (Cliente)
            var usuarios = _context.Usuarios
                .Where(u => u.Rol == 3) // Filtra usuarios con Rol = 3
                .ToList();

            ViewData["Usuarios"] = usuarios; // Almacena los usuarios en ViewData
            return View();
        }


        // POST: Facturacion/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Facturacion facturacion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(facturacion);
                _context.SaveChanges(); 
                return RedirectToAction(nameof(Index));
            }
            return View(facturacion);
        }

        // GET: Facturacion/Edit/5
        public IActionResult Edit(int id)
        {
            var facturacion = _context.Facturacion.Find(id);
            if (facturacion == null)
            {
                return NotFound();
            }
            return View(facturacion);
        }

        // POST: Facturacion/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Facturacion facturacion)
        {
            if (id != facturacion.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(facturacion);
                    _context.SaveChanges(); // Cambiado a SaveChanges() para no usar asincronía
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FacturacionExists(facturacion.Id))
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
            return View(facturacion);
        }

        // GET: Facturacion/Delete/5
        public IActionResult Delete(int id)
        {
            var facturacion = _context.Facturacion
                .Include(f => f.Usuario)
                .FirstOrDefault(m => m.Id == id);
            if (facturacion == null)
            {
                return NotFound();
            }

            return View(facturacion);
        }

        // POST: Facturacion/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var facturacion = _context.Facturacion.Find(id);
            if (facturacion != null)
            {
                _context.Facturacion.Remove(facturacion);
                _context.SaveChanges(); // Cambiado a SaveChanges() para no usar asincronía
            }
            return RedirectToAction(nameof(Index));
        }

        private bool FacturacionExists(int id)
        {
            return _context.Facturacion.Any(e => e.Id == id);
        }
    }
}
