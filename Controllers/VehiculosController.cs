using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using amazon.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace amazon.Controllers
{
    public class VehiculosController : Controller
    {
        private readonly DbamazonContext _context;

        public VehiculosController(DbamazonContext context)
        {
            _context = context;
        }

        // GET: Vehiculos
        public async Task<IActionResult> Index()
        {
            var vehiculos = await _context.Vehiculos
                .Include(v => v.Usuario)
                .ToListAsync();
            return View(vehiculos); // Asegúrate de que el nombre de la vista sea correcto
        }

        // GET: Vehiculos/Create
        public IActionResult Create()
        {
            ViewBag.Usuarios = new SelectList(_context.Usuarios, "Id", "Nombre");
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create([Bind("Marca,Modelo,Año,Placa,Descripcion,UsuarioId")] Vehiculo vehiculo)
        {
            
                try
                {
                    _context.Add(vehiculo);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            

            // Repoblar el dropdown si hay un error
            ViewBag.Usuarios = new SelectList(_context.Usuarios, "Id", "Nombre", vehiculo.UsuarioId);
            return View(vehiculo);
        }



        // GET: Vehiculos/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehiculo = _context.Vehiculos
                .FirstOrDefault(v => v.Id == id);

            if (vehiculo == null)
            {
                return NotFound();
            }

            return View(vehiculo);
        }

        // POST: Vehiculos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null)
            {
                return NotFound();
            }

            _context.Vehiculos.Remove(vehiculo);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
