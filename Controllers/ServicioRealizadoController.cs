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
                .Include(s => s.Vehiculo)
                .ToList();



            return View(serviciosRealizados);
        }


        // GET: ServicioRealizado/Create
        public IActionResult Create()
        {
            try
            {
                // Obtener usuarios con rol 3
                var usuarios = _context.Usuarios
                    .Where(u => u.Rol == 3)
                    .Select(u => new { u.Id, u.Nombre })
                    .ToList();

                ViewBag.Servicios = new SelectList(_context.Servicios, "Id", "Nombre");
                ViewBag.Usuarios = new SelectList(usuarios, "Id", "Nombre");
            }
            catch (Exception ex)
            {
                // Manejar la excepción
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                ViewBag.Servicios = new SelectList(new List<Servicio>(), "Id", "Nombre");
                ViewBag.Usuarios = new SelectList(new List<Usuario>(), "Id", "Nombre");
            }

            return View();
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,ServicioId,UsuarioId,VehiculoId,Precio,Fecha,Estado")] ServicioRealizado servicioRealizado)
        {
            try
            {
                // Asignar estado por defecto
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

            // Asegúrate de que ViewBag.Servicios, ViewBag.Usuarios y los vehículos estén inicializados
            ViewBag.Servicios = new SelectList(_context.Servicios, "Id", "Nombre", servicioRealizado.ServicioId);
            ViewBag.Usuarios = new SelectList(_context.Usuarios.Where(u => u.Rol == 3).ToList(), "Id", "Nombre", servicioRealizado.UsuarioId);

            // Obtén los vehículos del usuario seleccionado
            if (servicioRealizado.UsuarioId != 0)
            {
                ViewBag.Vehiculos = new SelectList(_context.Vehiculos.Where(v => v.UsuarioId == servicioRealizado.UsuarioId).ToList(), "Id", "Placa", servicioRealizado.VehiculoId);
            }
            else
            {
                ViewBag.Vehiculos = new SelectList(new List<Vehiculo>(), "Id", "Placa");
            }

            return View(servicioRealizado);
        }



        // GET: ServicioRealizado/GetVehiculosPorUsuario
        public JsonResult GetVehiculosPorUsuario(int usuarioId)
        {
            try
            {
                var vehiculos = _context.Vehiculos
                    .Where(v => v.UsuarioId == usuarioId)
                    .Select(v => new { v.Id, v.Marca, v.Modelo, v.Placa })
                    .ToList();

                return Json(vehiculos);
            }
            catch (Exception ex)
            {
                // Manejar la excepción
                return Json(new { error = $"Error: {ex.Message}" });
            }
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

            // Obtener usuarios con rol 3
            var usuarios = _context.Usuarios
                .Where(u => u.Rol == 3)
                .Select(u => new { u.Id, u.Nombre })
                .ToList();

            ViewBag.Servicios = new SelectList(_context.Servicios, "Id", "Nombre", servicioRealizado.ServicioId);
            ViewBag.Usuarios = new SelectList(usuarios, "Id", "Nombre", servicioRealizado.UsuarioId);

            // Obtener vehículos del usuario seleccionado
            var vehiculos = _context.Vehiculos
                .Where(v => v.UsuarioId == servicioRealizado.UsuarioId)
                .Select(v => new { v.Id, v.Placa })
                .ToList();

            ViewBag.Vehiculos = new SelectList(vehiculos, "Id", "Placa", servicioRealizado.VehiculoId);



            return View(servicioRealizado);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,ServicioId,UsuarioId,VehiculoId,Precio,Fecha,Estado")] ServicioRealizado servicioRealizado)
        {
            if (id != servicioRealizado.id)
            {
                return NotFound();
            }

            try
            {
                // Actualizar el servicio realizado
                _context.Update(servicioRealizado);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Servicio realizado actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
            }

            // Asegúrate de que ViewBag.Servicios, ViewBag.Usuarios y ViewBag.Vehiculos estén inicializados
            ViewBag.Servicios = new SelectList(_context.Servicios, "Id", "Nombre", servicioRealizado.ServicioId);
            ViewBag.Usuarios = new SelectList(_context.Usuarios.Where(u => u.Rol == 3).ToList(), "Id", "Nombre", servicioRealizado.UsuarioId);

            var vehiculos = _context.Vehiculos
                .Where(v => v.UsuarioId == servicioRealizado.UsuarioId)
                .Select(v => new { v.Id, v.Placa })
                .ToList();

            ViewBag.Vehiculos = new SelectList(vehiculos, "Id", "Placa", servicioRealizado.VehiculoId);

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

            // Obtener los servicios realizados por el usuario autenticado
            var serviciosRealizados = await _context.ServicioRealizado
                .Include(sr => sr.Servicio)
                .Where(sr => sr.UsuarioId == userId)
                .Select(sr => new ServicioRealizado
                {
                    id = sr.id,
                    ServicioId = sr.ServicioId,
                    Servicio = sr.Servicio,
                    Precio = sr.Precio,
                    Fecha = sr.Fecha,
                    Estado = sr.Estado
                })
                .ToListAsync();

            // Renderizar la vista con los servicios filtrados
            return View("~/Views/ServicioRealizado/Historial.cshtml", serviciosRealizados);
        }


    }

}