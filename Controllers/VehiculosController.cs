using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using amazon.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

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
        // GET: Vehiculos
        // GET: Vehiculos
        // GET: Vehiculos
        public async Task<IActionResult> Index()
        {
            // Obtener el ID del usuario autenticado y el rol desde los Claims
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value); // Obtiene el ID del usuario
            var rol = int.Parse(User.FindFirst(ClaimTypes.Role).Value); // Obtiene el rol del usuario

            List<Vehiculo> vehiculos;

            // Si el rol es 1 o 2, el usuario puede ver todos los vehículos
            if (rol == 1 || rol == 2)
            {
                vehiculos = await _context.Vehiculos
                    .Include(v => v.Usuario) // Incluye la relación con el usuario
                    .ToListAsync(); // Muestra todos los vehículos
            }
            else
            {
                // Si el rol no es 1 o 2 (por ejemplo, rol 3), muestra solo los vehículos del usuario autenticado
                vehiculos = await _context.Vehiculos
                    .Include(v => v.Usuario)
                    .Where(v => v.UsuarioId == usuarioId) // Filtrar por el ID del usuario autenticado
                    .ToListAsync();
            }

            return View(vehiculos);
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
    // Verificar si la placa ya está registrada
    var existePlaca = await _context.Vehiculos.AnyAsync(v => v.Placa == vehiculo.Placa);
    
    if (existePlaca)
    {
        // Si la placa ya existe, agregar un error al ModelState
        ModelState.AddModelError("Placa", "La placa ingresada ya está registrada.");
    }

    else
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
