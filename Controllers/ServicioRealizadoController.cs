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
            
     

            ViewBag.Servicios = new SelectList(_context.Servicios, "Id", "Nombre", servicioRealizado.ServicioId);
            return View(servicioRealizado);
        }


        // GET: ServicioRealizado/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var servicioRealizado = await _context.ServicioRealizado.FindAsync(id);
            if (servicioRealizado == null)
            {
                return NotFound();
            }

            ViewBag.Servicios = new SelectList(_context.Servicios, "Id", "Nombre", servicioRealizado.ServicioId);

            // Agregar los estados disponibles al ViewBag
            var estados = new List<SelectListItem>
    {
        new SelectListItem { Value = "1", Text = "En Proceso" },
        new SelectListItem { Value = "2", Text = "Completado" },
        // Agregar otros estados según sea necesario
    };
            ViewBag.Estados = new SelectList(estados, "Value", "Text", servicioRealizado.Estado);

            return View(servicioRealizado);
        }

        // POST: ServicioRealizado/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,ServicioId,Precio,Fecha,Estado")] ServicioRealizado servicioRealizado)
        {
            if (id != servicioRealizado.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (userIdClaim == null)
                    {
                        ModelState.AddModelError(string.Empty, "User ID not found.");
                        ViewBag.Servicios = new SelectList(_context.Servicios, "Id", "Nombre", servicioRealizado.ServicioId);
                        return View(servicioRealizado);
                    }

                    servicioRealizado.UsuarioId = int.Parse(userIdClaim);

                    _context.Update(servicioRealizado);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Servicio realizado actualizado correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServicioRealizadoExists(servicioRealizado.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Servicios = new SelectList(_context.Servicios, "Id", "Nombre", servicioRealizado.ServicioId);

            var estados = new List<SelectListItem>
    {
        new SelectListItem { Value = "1", Text = "En Proceso" },
        new SelectListItem { Value = "2", Text = "Completado" },
    };
            ViewBag.Estados = new SelectList(estados, "Value", "Text", servicioRealizado.Estado);

            return View(servicioRealizado);
        }

        private bool ServicioRealizadoExists(int id)
        {
            return _context.ServicioRealizado.Any(e => e.id == id);
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
