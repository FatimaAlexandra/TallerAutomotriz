using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using amazon.Models;
using System.Linq;
using System.Threading.Tasks;

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
            return View("Index", vehiculos); // Asegúrate de que el nombre de la vista sea correcto
        }


        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehiculo = _context.Vehiculos
                .FirstOrDefault(v =>v.Id  == id);

            if (vehiculo == null)
            {
                return NotFound();
            }

            return View(vehiculo);
        }

        // POST: vehiculos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var vehiculo = _context.Vehiculos.Find(id);
            if (vehiculo == null)
            {
                return NotFound();
            }

            _context.Vehiculos.Remove(vehiculo);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }


}
