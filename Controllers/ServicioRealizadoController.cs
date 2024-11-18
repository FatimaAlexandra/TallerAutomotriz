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
        public async Task<IActionResult> Index()
        {
            var serviciosRealizados = await _context.ServicioRealizado
                .Include(s => s.Servicio)
                .Include(s => s.Usuario)
                .Include(s => s.Vehiculo)
                .ToListAsync();

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
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al cargar el formulario: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,ServicioId,UsuarioId,VehiculoId,Precio,Fecha,Estado")] ServicioRealizado servicioRealizado)
        {
            try
            {
                servicioRealizado.Estado = 1; // Estado inicial
                _context.Add(servicioRealizado);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Servicio creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al crear el servicio: {ex.Message}";
                return View(servicioRealizado);
            }
        }



        // Agregar este método al ServicioRealizadoController existente
        public async Task<JsonResult> GetServicioDetalles(int id)
        {
            var servicio = await _context.ServicioRealizado
                .Include(s => s.Servicio)
                .Include(s => s.Vehiculo)
                .Where(s => s.id == id)
                .Select(s => new {
                    ServicioNombre = s.Servicio.Nombre,
                    VehiculoInfo = $"{s.Vehiculo.Marca} {s.Vehiculo.Modelo} ({s.Vehiculo.Placa})",
                    Fecha = s.Fecha,
                    Estado = s.Estado
                })
                .FirstOrDefaultAsync();

            return Json(servicio);
        }




        // GET: ServicioRealizado/GetVehiculosPorUsuario/5
        public async Task<JsonResult> GetVehiculosPorUsuario(int usuarioId)
        {
            try
            {
                var vehiculos = await _context.Vehiculos
                    .Where(v => v.UsuarioId == usuarioId)
                    .Select(v => new { v.Id, v.Marca, v.Modelo, v.Placa })
                    .ToListAsync();

                return Json(new { success = true, data = vehiculos });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: ServicioRealizado/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var servicioRealizado = await _context.ServicioRealizado
                .Include(s => s.Servicio)
                .Include(s => s.Usuario)
                .Include(s => s.Vehiculo)
                .FirstOrDefaultAsync(s => s.id == id);

            if (servicioRealizado == null)
            {
                return NotFound();
            }

            ViewBag.Servicios = new SelectList(_context.Servicios, "Id", "Nombre", servicioRealizado.ServicioId);
            ViewBag.Usuarios = new SelectList(_context.Usuarios.Where(u => u.Rol == 3), "Id", "Nombre", servicioRealizado.UsuarioId);
            ViewBag.Vehiculos = new SelectList(_context.Vehiculos.Where(v => v.UsuarioId == servicioRealizado.UsuarioId), "Id", "Placa", servicioRealizado.VehiculoId);

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
                _context.Update(servicioRealizado);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Servicio actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al actualizar el servicio: {ex.Message}";
                return View(servicioRealizado);
            }
        }

        // GET: ServicioRealizado/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var servicioRealizado = await _context.ServicioRealizado
                .Include(s => s.Servicio)
                .Include(s => s.Usuario)
                .Include(s => s.Vehiculo)
                .FirstOrDefaultAsync(m => m.id == id);

            if (servicioRealizado == null)
            {
                return NotFound();
            }

            return View(servicioRealizado);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var servicioRealizado = await _context.ServicioRealizado.FindAsync(id);
                if (servicioRealizado != null)
                {
                    _context.ServicioRealizado.Remove(servicioRealizado);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Servicio eliminado exitosamente.";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al eliminar el servicio: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [Authorize]
        public async Task<IActionResult> Historial()
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                // Cargar servicios con sus relaciones
                var serviciosRealizados = await _context.ServicioRealizado
                    .Include(sr => sr.Servicio)
                    .Include(sr => sr.Vehiculo)
                    .Include(sr => sr.Usuario)
                    .Where(sr => sr.UsuarioId == usuarioId)
                    .OrderByDescending(sr => sr.Fecha)
                    .ToListAsync();

                // Cargar comentarios relacionados
                var comentariosUsuario = await _context.Comentarios
                    .Where(c => c.UsuarioId == usuarioId && c.Estado)
                    .ToListAsync();

                // Guardar en ViewBag para la vista
                ViewBag.Comentarios = comentariosUsuario;

                return View(serviciosRealizados);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al cargar el historial: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        private bool ServicioRealizadoExists(int id)
        {
            return _context.ServicioRealizado.Any(e => e.id == id);
        }
    }
}