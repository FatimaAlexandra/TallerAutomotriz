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

  
        public IActionResult Details(int id)
        {
            var factura = _context.Facturacion
                                  .Include(f => f.Usuario)  // Incluye el usuario relacionado
                                  .FirstOrDefault(f => f.Id == id);

            if (factura == null)
            {
                return NotFound();
            }

            return PartialView("_FacturaDetailsPartial", factura);  // Devolver la vista parcial con los detalles de la factura
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

        // GET: Facturacion/Edit
        public IActionResult Edit(int id)
        {
            var facturacion = _context.Facturacion.Find(id);
            if (facturacion == null)
            {
                return NotFound();
            }
            return View(facturacion);
        }

        // POST: Facturacion/Edit
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
                    _context.SaveChanges(); 
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

        // GET: Facturacion/Delete
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

        // POST: Facturacion/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var facturacion = _context.Facturacion.Find(id);
            if (facturacion != null)
            {
                _context.Facturacion.Remove(facturacion);
                _context.SaveChanges(); 
            }
            return RedirectToAction(nameof(Index));
        }

        private bool FacturacionExists(int id)
        {
            return _context.Facturacion.Any(e => e.Id == id);
        }
    }
}
