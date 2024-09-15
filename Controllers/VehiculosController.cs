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
            return View("Index", vehiculos); // Asegúrate de que el nombre de la vista sea correcto
        }


        //metodo para editar

        // GET: Vehiculos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null)
            {
                return NotFound();
            }

            ViewBag.Usuarios = new SelectList(_context.Usuarios, "Id", "Nombre", vehiculo.UsuarioId);

            return View(vehiculo);
        }

        // POST: Vehiculos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UsuarioId,Marca,Modelo,Año,Placa,Descripcion")] Vehiculo vehiculo)
        {
            if (id != vehiculo.Id)
            {
                return NotFound();
            }
            Console.WriteLine("Hasta este punto funciona");
            if (ModelState.IsValid)
            {
                Console.WriteLine("Entramos al if de modelo valido");

                try
                {
                    Console.WriteLine("Hasta este punto funciona 2");
                    _context.Update(vehiculo);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Vehículo actualizado correctamente.";
                    Console.WriteLine("Hasta este punto funciona 3");
                   
                    return RedirectToAction(nameof(Index));
                }

                catch (DbUpdateConcurrencyException)
                {
                    if (!VehiculoExists(vehiculo.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            Console.WriteLine("En teoria aqui editamos ya");

            if (!ModelState.IsValid)
            {
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                }
            }

            Console.WriteLine(vehiculo.UsuarioId);

            Console.WriteLine("El modelo no es valido");

            ViewBag.Usuarios = new SelectList(_context.Usuarios, "Id", "Nombre", vehiculo.UsuarioId);
            return View(vehiculo);
        }

        private bool VehiculoExists(int id)
        {
            return _context.Vehiculos.Any(e => e.Id == id);
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
