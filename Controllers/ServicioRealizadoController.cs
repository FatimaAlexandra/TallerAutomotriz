using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using amazon.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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


        // GET: ServicioRealizado/Create
        public IActionResult Create()
        {
            //Obtener usuario
            var user_id = User.FindFirst(ClaimTypes.NameIdentifier);
            var name = User.FindFirst(ClaimTypes.Name);
            if (name != null)
            {
                TempData["user_id"] = name.Value;
            }


            ViewBag.Servicios = new SelectList(_context.Servicios, "Id", "Nombre");
            return View();
        }

    

        // POST: ServicioRealizado/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,ServicioId,Precio,Fecha,Estado")] ServicioRealizado servicioRealizado)
        {
           
                try
                {
                    // Obtener el ID del usuario que ha iniciado sesión
                    var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    if (userIdClaim == null)
                    {
                        ModelState.AddModelError(string.Empty, "User ID not found.");
                        ViewBag.Servicios = new SelectList(_context.Servicios, "Id", "Nombre", servicioRealizado.ServicioId);
                        return View(servicioRealizado);
                    }

                    // Asignar el ID del usuario y estado por defecto
                    servicioRealizado.UsuarioId = int.Parse(userIdClaim); // Asegúrate de que el ID del usuario es un entero
                    servicioRealizado.Estado = 1;

                    _context.Add(servicioRealizado);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Servicio realizado creado correctamente.";

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                }
            
            TempData["user_id"] = "sds";

            ViewBag.Servicios = new SelectList(_context.Servicios, "Id", "Nombre", servicioRealizado.ServicioId);
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

        // GET: ServicioRealizado/Historial
        [Authorize]
        public async Task<IActionResult> Historial()
        {
            // Obtener el ID del usuario que ha iniciado sesión
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim);

            var serviciosRealizados = await _context.ServicioRealizado
                .Include(sr => sr.Servicio)
                .Where(sr => sr.UsuarioId == userId)
                .ToListAsync();

            return View("~/Views/Historial/Historial.cshtml", serviciosRealizados);
        }
    }

}
