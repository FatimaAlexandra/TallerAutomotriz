using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using amazon.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace amazon.Controllers
{
    [Authorize]
    public class ComentariosController : Controller
    {
        private readonly DbamazonContext _context;

        public ComentariosController(DbamazonContext context)
        {
            _context = context;
        }





        // GET: Comentarios
        [Authorize(Roles = "1,2,3")] // Permitir acceso a todos los roles
        public async Task<IActionResult> Index()
        {
            try
            {
                var comentarios = await _context.Comentarios
                    .Include(c => c.Usuario)
                    .Include(c => c.ServicioRealizado)
                        .ThenInclude(s => s.Servicio)
                    .Include(c => c.ServicioRealizado)
                        .ThenInclude(s => s.Vehiculo)
                    .Where(c => c.Estado)
                    .OrderByDescending(c => c.FechaCreacion)
                    .ToListAsync();

                return View(comentarios);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar los comentarios.";
                return RedirectToAction("Index", "Home");
            }
        }





        // GET: Comentarios/MisComentarios
        [Authorize(Roles = "3")] // Solo clientes
        public async Task<IActionResult> MisComentarios()
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var comentarios = await _context.Comentarios
                    .Include(c => c.ServicioRealizado)
                        .ThenInclude(s => s.Servicio)
                    .Include(c => c.ServicioRealizado)
                        .ThenInclude(s => s.Vehiculo)
                    .Where(c => c.UsuarioId == usuarioId && c.Estado)
                    .OrderByDescending(c => c.FechaCreacion)
                    .ToListAsync();

                return View(comentarios);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar sus comentarios.";
                return RedirectToAction("Index", "Home");
            }
        }






        // GET: Comentarios/Create
        [Authorize(Roles = "3")]
        public async Task<IActionResult> Create(int servicioRealizadoId)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                // Verificar si el servicio existe y pertenece al usuario
                var servicio = await _context.ServicioRealizado
                    .Include(s => s.Servicio)
                    .Include(s => s.Vehiculo)
                    .FirstOrDefaultAsync(s => s.id == servicioRealizadoId && s.UsuarioId == usuarioId);

                if (servicio == null)
                {
                    TempData["ErrorMessage"] = "El servicio solicitado no existe o no tiene permisos para comentarlo.";
                    return RedirectToAction("Historial", "ServicioRealizado");
                }

                // Verificar si ya existe un comentario
                var comentarioExistente = await _context.Comentarios
                    .AnyAsync(c => c.ServicioRealizadoId == servicioRealizadoId && c.Estado);

                if (comentarioExistente)
                {
                    TempData["ErrorMessage"] = "Ya existe un comentario para este servicio.";
                    return RedirectToAction(nameof(MisComentarios));
                }

                // Verificar si el servicio y el vehículo existen antes de acceder a sus propiedades
                if (servicio.Servicio != null && servicio.Vehiculo != null)
                {
                    ViewBag.ServicioNombre = servicio.Servicio.Nombre;
                    ViewBag.VehiculoInfo = $"{servicio.Vehiculo.Marca} {servicio.Vehiculo.Modelo} ({servicio.Vehiculo.Placa})";
                    ViewBag.FechaServicio = servicio.Fecha;
                }
                else
                {
                    ViewBag.ServicioNombre = "N/A";
                    ViewBag.VehiculoInfo = "N/A";
                    ViewBag.FechaServicio = "N/A";
                }

                var comentario = new Comentario
                {
                    UsuarioId = usuarioId,
                    ServicioRealizadoId = servicioRealizadoId,
                    FechaCreacion = DateTime.Now
                };

                return View(comentario);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al preparar el formulario de comentario.";
                return RedirectToAction("Historial", "ServicioRealizado");
            }
        }

        // POST: Comentarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "3")]




        public async Task<IActionResult> Create([Bind("ServicioRealizadoId,Contenido,Calificacion")] Comentario comentario)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                // Verificar si el servicio existe y pertenece al usuario
                var servicio = await _context.ServicioRealizado
                    .Include(s => s.Servicio)
                    .Include(s => s.Vehiculo)
                    .FirstOrDefaultAsync(s => s.id == comentario.ServicioRealizadoId && s.UsuarioId == usuarioId);

                if (servicio == null)
                {
                    TempData["ErrorMessage"] = "El servicio solicitado no existe o no tiene permisos para comentarlo.";
                    return RedirectToAction("Historial", "ServicioRealizado");
                }

                // Verificar si ya existe un comentario
                var comentarioExistente = await _context.Comentarios
                    .AnyAsync(c => c.ServicioRealizadoId == comentario.ServicioRealizadoId && c.Estado);

                if (comentarioExistente)
                {
                    TempData["ErrorMessage"] = "Ya existe un comentario para este servicio.";
                    return RedirectToAction(nameof(MisComentarios));
                }

                // Validar el contenido y calificación
                if (string.IsNullOrWhiteSpace(comentario.Contenido) || comentario.Contenido.Length < 10)
                {
                    TempData["ErrorMessage"] = "El comentario debe tener al menos 10 caracteres.";
                    return RedirectToAction(nameof(Create), new { servicioRealizadoId = comentario.ServicioRealizadoId });
                }

                if (comentario.Calificacion < 1 || comentario.Calificacion > 5)
                {
                    TempData["ErrorMessage"] = "La calificación debe estar entre 1 y 5 estrellas.";
                    return RedirectToAction(nameof(Create), new { servicioRealizadoId = comentario.ServicioRealizadoId });
                }

                // Configurar el comentario
                comentario.UsuarioId = usuarioId;
                comentario.Estado = true;
                comentario.FechaCreacion = DateTime.Now;

                // Guardar el comentario
                _context.Add(comentario);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Comentario agregado exitosamente.";
                return RedirectToAction(nameof(MisComentarios));
            }
            catch (Exception ex)
            {
                // Agregar logging aquí si es necesario
                TempData["ErrorMessage"] = "Ocurrió un error al crear el comentario. Por favor, intente nuevamente.";
                return RedirectToAction(nameof(Create), new { servicioRealizadoId = comentario.ServicioRealizadoId });
            }
        }

















        // GET: Comentarios/Edit/5
        [Authorize(Roles = "3")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var comentario = await _context.Comentarios
                .Include(c => c.ServicioRealizado)
                    .ThenInclude(s => s.Servicio)
                .Include(c => c.ServicioRealizado)
                    .ThenInclude(s => s.Vehiculo)
                .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == usuarioId && c.Estado);

            if (comentario == null)
            {
                TempData["ErrorMessage"] = "Comentario no encontrado.";
                return RedirectToAction(nameof(MisComentarios));
            }

            return View(comentario);
        }

        // POST: Comentarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "3")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ServicioRealizadoId,Contenido,Calificacion")] Comentario comentario)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                // Obtener el comentario existente con todas sus relaciones
                var comentarioExistente = await _context.Comentarios
                    .Include(c => c.ServicioRealizado)
                        .ThenInclude(s => s.Servicio)
                    .Include(c => c.ServicioRealizado)
                        .ThenInclude(s => s.Vehiculo)
                    .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == usuarioId && c.Estado);

                if (comentarioExistente == null)
                {
                    TempData["ErrorMessage"] = "Comentario no encontrado.";
                    return RedirectToAction(nameof(MisComentarios));
                }

                // Validar el contenido
                if (string.IsNullOrWhiteSpace(comentario.Contenido) || comentario.Contenido.Length < 10)
                {
                    ModelState.AddModelError("Contenido", "El comentario debe tener al menos 10 caracteres.");
                    return View(comentarioExistente);
                }

                // Validar la calificación
                if (comentario.Calificacion < 1 || comentario.Calificacion > 5)
                {
                    ModelState.AddModelError("Calificacion", "La calificación debe estar entre 1 y 5 estrellas.");
                    return View(comentarioExistente);
                }

                // Actualizar solo los campos permitidos
                comentarioExistente.Contenido = comentario.Contenido;
                comentarioExistente.Calificacion = comentario.Calificacion;
                comentarioExistente.FechaModificacion = DateTime.Now;

                // Marcar solo la entidad como modificada
                _context.Entry(comentarioExistente).State = EntityState.Modified;

                // Guardar los cambios
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Comentario actualizado exitosamente.";
                return RedirectToAction(nameof(MisComentarios));
            }
            catch (Exception ex)
            {
                // Log the error here if needed
                TempData["ErrorMessage"] = "Ocurrió un error al actualizar el comentario.";

                // Recargar el comentario con sus relaciones para mostrar la vista nuevamente
                var comentarioParaRetornar = await _context.Comentarios
                    .Include(c => c.ServicioRealizado)
                        .ThenInclude(s => s.Servicio)
                    .Include(c => c.ServicioRealizado)
                        .ThenInclude(s => s.Vehiculo)
                    .FirstOrDefaultAsync(c => c.Id == id);

                return View(comentarioParaRetornar);
            }
        }












        // POST: Comentarios/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "3")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var comentario = await _context.Comentarios
                    .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == usuarioId && c.Estado);

                if (comentario == null)
                {
                    TempData["ErrorMessage"] = "Comentario no encontrado.";
                    return RedirectToAction(nameof(MisComentarios));
                }

                comentario.Estado = false;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Comentario eliminado exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar el comentario.";
            }

            return RedirectToAction(nameof(MisComentarios));
        }

        private bool ComentarioExists(int id)
        {
            return _context.Comentarios.Any(e => e.Id == id);
        }
    }
}