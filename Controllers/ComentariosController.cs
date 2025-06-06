using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using amazon.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Collections.Generic;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace amazon.Controllers
{
    // Clases auxiliares para transferencia de datos
    public class MecanicoEstadistica
    {
        public int MecanicoId { get; set; }
        public string Nombre { get; set; }
        public double PromedioCalificacion { get; set; }
        public int TotalServicios { get; set; }
    }

    public class ServicioEstadistica
    {
        public int ServicioId { get; set; }
        public string Nombre { get; set; }
        public double PromedioCalificacion { get; set; }
        public int TotalComentarios { get; set; }
    }

    [Authorize]
    public class ComentariosController : Controller
    {
        private readonly DbamazonContext _context;

        public ComentariosController(DbamazonContext context)
        {
            _context = context;
        }

        // GET: Comentarios
        [Authorize(Roles = "1,2,3")]
        public IActionResult Index()
        {
            try
            {
                // Cargar comentarios
                List<Comentario> comentarios = _context.Comentarios
                    .Where(c => c.Estado)
                    .Include(c => c.Usuario)
                    .Include(c => c.ServicioRealizado)
                        .ThenInclude(s => s.Servicio)
                    .Include(c => c.ServicioRealizado)
                        .ThenInclude(s => s.Vehiculo)
                    .OrderByDescending(c => c.FechaCreacion)
                    .ToList();

                // Estadísticas básicas
                ViewBag.PromedioCalificacion = comentarios.Any() ? comentarios.Average(c => c.Calificacion) : 0;
                ViewBag.TotalComentarios = comentarios.Count;
                ViewBag.DistribucionCalificaciones = new Dictionary<int, int>
                {
                    { 5, comentarios.Count(c => c.Calificacion == 5) },
                    { 4, comentarios.Count(c => c.Calificacion == 4) },
                    { 3, comentarios.Count(c => c.Calificacion == 3) },
                    { 2, comentarios.Count(c => c.Calificacion == 2) },
                    { 1, comentarios.Count(c => c.Calificacion == 1) }
                };

                // Para administradores, calcular estadísticas adicionales
                if (User.IsInRole("1"))
                {
                    // Servicios mejor calificados
                    List<ServicioEstadistica> serviciosMejorCalificados = new List<ServicioEstadistica>();

                    // Mecánicos mejor calificados
                    List<MecanicoEstadistica> mecanicosMejorCalificados = new List<MecanicoEstadistica>();

                    // Procesar servicios
                    var serviciosConComentarios = comentarios
                        .Where(c => c.ServicioRealizado?.Servicio != null)
                        .GroupBy(c => new {
                            ServicioId = c.ServicioRealizado.ServicioId,
                            Nombre = c.ServicioRealizado.Servicio.Nombre
                        });

                    foreach (var grupo in serviciosConComentarios)
                    {
                        serviciosMejorCalificados.Add(new ServicioEstadistica
                        {
                            ServicioId = grupo.Key.ServicioId,
                            Nombre = grupo.Key.Nombre,
                            PromedioCalificacion = grupo.Average(c => c.Calificacion),
                            TotalComentarios = grupo.Count()
                        });
                    }

                    serviciosMejorCalificados = serviciosMejorCalificados
                        .Where(s => s.TotalComentarios > 0)
                        .OrderByDescending(s => s.PromedioCalificacion)
                        .Take(5)
                        .ToList();

                    ViewBag.ServiciosMejorCalificados = serviciosMejorCalificados;

                    // Procesar mecánicos
                    var mecanicos = _context.Usuarios.Where(u => u.Rol == 2).ToList();

                    foreach (var mecanico in mecanicos)
                    {
                        // Servicios realizados por este mecánico que tienen comentarios
                        var serviciosMecanico = comentarios
                            .Where(c => c.ServicioRealizado?.UsuarioId == mecanico.Id)
                            .ToList();

                        if (serviciosMecanico.Any())
                        {
                            mecanicosMejorCalificados.Add(new MecanicoEstadistica
                            {
                                MecanicoId = mecanico.Id,
                                Nombre = mecanico.Nombre,
                                PromedioCalificacion = serviciosMecanico.Average(c => c.Calificacion),
                                TotalServicios = serviciosMecanico.Count
                            });
                        }
                    }

                    mecanicosMejorCalificados = mecanicosMejorCalificados
                        .OrderByDescending(m => m.PromedioCalificacion)
                        .Take(5)
                        .ToList();

                    ViewBag.MecanicosMejorCalificados = mecanicosMejorCalificados;
                }

                return View(comentarios);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error al cargar los comentarios: " + ex.Message;
                return View(new List<Comentario>());
            }
        }

        // GET: Comentarios/MisComentarios
        [Authorize(Roles = "3")]
        public IActionResult MisComentarios()
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var comentarios = _context.Comentarios
                    .Where(c => c.UsuarioId == usuarioId && c.Estado)
                    .Include(c => c.ServicioRealizado)
                        .ThenInclude(s => s.Servicio)
                    .Include(c => c.ServicioRealizado)
                        .ThenInclude(s => s.Vehiculo)
                    .OrderByDescending(c => c.FechaCreacion)
                    .ToList();

                return View(comentarios);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error al cargar sus comentarios: " + ex.Message;
                return View(new List<Comentario>());
            }
        }

        // GET: Comentarios/Create
        [Authorize(Roles = "3")]
        public async Task<IActionResult> Create(int servicioRealizadoId)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                // Verificar servicio y permisos
                var servicio = await _context.ServicioRealizado
                    .Include(s => s.Servicio)
                    .Include(s => s.Vehiculo)
                    .FirstOrDefaultAsync(s => s.id == servicioRealizadoId);

                if (servicio == null)
                {
                    TempData["ErrorMessage"] = "El servicio solicitado no existe.";
                    return RedirectToAction("Historial", "ServicioRealizado");
                }

                if (servicio.UsuarioId != usuarioId)
                {
                    TempData["ErrorMessage"] = "No tienes permisos para comentar este servicio.";
                    return RedirectToAction("Historial", "ServicioRealizado");
                }

                if (servicio.Estado != 2) // No completado
                {
                    TempData["ErrorMessage"] = "Solo puedes comentar servicios completados.";
                    return RedirectToAction("Historial", "ServicioRealizado");
                }

                // Verificar si ya existe comentario
                var comentarioExistente = await _context.Comentarios
                    .AnyAsync(c => c.ServicioRealizadoId == servicioRealizadoId && c.Estado);

                if (comentarioExistente)
                {
                    TempData["ErrorMessage"] = "Ya existe un comentario para este servicio.";
                    return RedirectToAction(nameof(MisComentarios));
                }

                // Preparar datos para la vista
                ViewBag.ServicioNombre = servicio.Servicio?.Nombre ?? "N/A";
                ViewBag.VehiculoInfo = servicio.Vehiculo != null ?
                    $"{servicio.Vehiculo.Marca} {servicio.Vehiculo.Modelo} ({servicio.Vehiculo.Placa})" : "N/A";
                ViewBag.FechaServicio = servicio.Fecha;

                // Obtener información del mecánico
                var mecanico = await _context.Usuarios.FirstOrDefaultAsync(u => u.Rol == 2);
                ViewBag.MecanicoNombre = mecanico?.Nombre ?? "No especificado";

                return View(new Comentario
                {
                    UsuarioId = usuarioId,
                    ServicioRealizadoId = servicioRealizadoId,
                    FechaCreacion = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al preparar el formulario: " + ex.Message;
                return RedirectToAction("Historial", "ServicioRealizado");
            }
        }

        // POST: Comentarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "3")]
        public async Task<IActionResult> Create([Bind("ServicioRealizadoId,Contenido,Calificacion,AspectosDestacados")] Comentario comentario)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                // Validar servicio
                var servicio = await _context.ServicioRealizado
                    .FirstOrDefaultAsync(s => s.id == comentario.ServicioRealizadoId);

                if (servicio == null || servicio.UsuarioId != usuarioId || servicio.Estado != 2)
                {
                    TempData["ErrorMessage"] = "El servicio no existe, no te pertenece o no está completado.";
                    return RedirectToAction("Historial", "ServicioRealizado");
                }

                // Verificar si ya existe comentario
                if (await _context.Comentarios.AnyAsync(c => c.ServicioRealizadoId == comentario.ServicioRealizadoId && c.Estado))
                {
                    TempData["ErrorMessage"] = "Ya existe un comentario para este servicio.";
                    return RedirectToAction(nameof(MisComentarios));
                }

                // Validar contenido
                if (string.IsNullOrWhiteSpace(comentario.Contenido) || comentario.Contenido.Length < 10)
                {
                    TempData["ErrorMessage"] = "El comentario debe tener al menos 10 caracteres.";
                    return RedirectToAction(nameof(Create), new { servicioRealizadoId = comentario.ServicioRealizadoId });
                }

                // Validar calificación
                if (comentario.Calificacion < 1 || comentario.Calificacion > 5)
                {
                    TempData["ErrorMessage"] = "La calificación debe estar entre 1 y 5 estrellas.";
                    return RedirectToAction(nameof(Create), new { servicioRealizadoId = comentario.ServicioRealizadoId });
                }

                // Configurar comentario
                comentario.UsuarioId = usuarioId;
                comentario.Estado = true;
                comentario.FechaCreacion = DateTime.Now;

                // Asignar aspectos destacados automáticos si no se proporcionaron
                if (string.IsNullOrWhiteSpace(comentario.AspectosDestacados))
                {
                    comentario.AspectosDestacados = comentario.Calificacion switch
                    {
                        5 => "Excelente servicio, Atención al cliente, Profesionalismo",
                        4 => "Buen servicio, Profesionalismo",
                        3 => "Servicio aceptable",
                        _ => null
                    };
                }

                // Guardar el comentario
                _context.Add(comentario);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Comentario agregado exitosamente.";
                return RedirectToAction(nameof(MisComentarios));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al crear el comentario: " + ex.Message;
                return RedirectToAction(nameof(Create), new { servicioRealizadoId = comentario.ServicioRealizadoId });
            }
        }

        // GET: Comentarios/Edit/5
        [Authorize(Roles = "1,3")]  // Modificado para permitir acceso a administradores
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = int.Parse(User.FindFirstValue(ClaimTypes.Role));

            // Consulta base para obtener el comentario
            var query = _context.Comentarios
                .Include(c => c.ServicioRealizado)
                    .ThenInclude(s => s.Servicio)
                .Include(c => c.ServicioRealizado)
                    .ThenInclude(s => s.Vehiculo)
                .Where(c => c.Id == id && c.Estado);

            // Si es administrador, puede editar cualquier comentario
            // Si es cliente, solo puede editar sus propios comentarios
            if (userRole != 1)
            {
                query = query.Where(c => c.UsuarioId == usuarioId);
            }

            var comentario = await query.FirstOrDefaultAsync();

            if (comentario == null)
            {
                TempData["ErrorMessage"] = "Comentario no encontrado.";
                return RedirectToAction(userRole == 1 ? nameof(Index) : nameof(MisComentarios));
            }

            // Información adicional
            var mecanico = await _context.Usuarios.FirstOrDefaultAsync(u => u.Rol == 2);
            ViewBag.MecanicoNombre = mecanico?.Nombre ?? "No especificado";

            return View(comentario);
        }

        // POST: Comentarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "1,3")]  // Modificado para permitir acceso a administradores
        public async Task<IActionResult> Edit(int id, [Bind("Id,ServicioRealizadoId,Contenido,Calificacion,AspectosDestacados")] Comentario comentario)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var userRole = int.Parse(User.FindFirstValue(ClaimTypes.Role));

                // Verificar existencia del comentario
                var comentarioExistente = await _context.Comentarios
                    .FirstOrDefaultAsync(c => c.Id == id && c.Estado);

                if (comentarioExistente == null)
                {
                    TempData["ErrorMessage"] = "Comentario no encontrado.";
                    return RedirectToAction(userRole == 1 ? nameof(Index) : nameof(MisComentarios));
                }

                // Si no es administrador, verificar que el comentario pertenezca al usuario
                if (userRole != 1 && comentarioExistente.UsuarioId != usuarioId)
                {
                    TempData["ErrorMessage"] = "No tienes permisos para editar este comentario.";
                    return RedirectToAction(nameof(MisComentarios));
                }

                // Validaciones
                if (string.IsNullOrWhiteSpace(comentario.Contenido) || comentario.Contenido.Length < 10)
                {
                    ModelState.AddModelError("Contenido", "El comentario debe tener al menos 10 caracteres.");
                    return View(await CargarComentarioParaVista(id));
                }

                if (comentario.Calificacion < 1 || comentario.Calificacion > 5)
                {
                    ModelState.AddModelError("Calificacion", "La calificación debe estar entre 1 y 5 estrellas.");
                    return View(await CargarComentarioParaVista(id));
                }

                // Actualizar campos
                comentarioExistente.Contenido = comentario.Contenido;
                comentarioExistente.Calificacion = comentario.Calificacion;
                comentarioExistente.AspectosDestacados = comentario.AspectosDestacados;
                comentarioExistente.FechaModificacion = DateTime.Now;

                _context.Update(comentarioExistente);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Comentario actualizado exitosamente.";
                return RedirectToAction(userRole == 1 ? nameof(Index) : nameof(MisComentarios));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al actualizar el comentario: " + ex.Message;
                return View(await CargarComentarioParaVista(id));
            }
        }

        // Método auxiliar para cargar comentario con sus relaciones
        private async Task<Comentario> CargarComentarioParaVista(int id)
        {
            try
            {
                return await _context.Comentarios
                    .Include(c => c.ServicioRealizado)
                        .ThenInclude(s => s.Servicio)
                    .Include(c => c.ServicioRealizado)
                        .ThenInclude(s => s.Vehiculo)
                    .FirstOrDefaultAsync(c => c.Id == id) ?? new Comentario();
            }
            catch
            {
                return new Comentario();
            }
        }

        // POST: Comentarios/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "1,3")]  // Modificado para permitir acceso a administradores
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var userRole = int.Parse(User.FindFirstValue(ClaimTypes.Role));

                // Consulta base para obtener el comentario
                var query = _context.Comentarios.Where(c => c.Id == id && c.Estado);

                // Si no es administrador, solo puede eliminar sus propios comentarios
                if (userRole != 1)
                {
                    query = query.Where(c => c.UsuarioId == usuarioId);
                }

                var comentario = await query.FirstOrDefaultAsync();

                if (comentario == null)
                {
                    TempData["ErrorMessage"] = "Comentario no encontrado.";
                    return RedirectToAction(userRole == 1 ? nameof(Index) : nameof(MisComentarios));
                }

                // Eliminación lógica (cambio de estado)
                comentario.Estado = false;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Comentario eliminado exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar el comentario: " + ex.Message;
            }

            // Redirigir según el rol
            var userRoleForRedirect = int.Parse(User.FindFirstValue(ClaimTypes.Role));
            return RedirectToAction(userRoleForRedirect == 1 ? nameof(Index) : nameof(MisComentarios));
        }

        // GET: Comentarios/ServiciosDestacados
        [AllowAnonymous]
        public async Task<IActionResult> ServiciosDestacados()
        {
            try
            {
                // Cargar datos necesarios
                var comentarios = await _context.Comentarios
                    .Where(c => c.Estado)
                    .ToListAsync();

                var serviciosRealizados = await _context.ServicioRealizado
                    .Where(sr => sr.Estado == 2)
                    .Include(sr => sr.Servicio)
                    .ToListAsync();

                // Filtrar para asegurar que solo usamos entidades completas
                var serviciosConComentarios = new List<ServicioEstadistica>();

                // Agrupar comentarios por servicio
                var comentariosAgrupados = comentarios
                    .Where(c => c.ServicioRealizado != null)
                    .GroupBy(c => c.ServicioRealizadoId);

                // Procesar cada grupo para calcular estadísticas
                foreach (var grupo in comentariosAgrupados)
                {
                    var servicioRealizado = serviciosRealizados
                        .FirstOrDefault(sr => sr.id == grupo.Key && sr.Servicio != null);

                    if (servicioRealizado != null)
                    {
                        serviciosConComentarios.Add(new ServicioEstadistica
                        {
                            ServicioId = servicioRealizado.ServicioId,
                            Nombre = servicioRealizado.Servicio.Nombre,
                            PromedioCalificacion = grupo.Average(c => c.Calificacion),
                            TotalComentarios = grupo.Count()
                        });
                    }
                }

                // Ordenar y filtrar los resultados
                var serviciosDestacados = serviciosConComentarios
                    .Where(s => s.TotalComentarios >= 1)
                    .OrderByDescending(s => s.PromedioCalificacion)
                    .Take(5)
                    .ToList();

                return View(serviciosDestacados);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar los servicios destacados: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Comentarios/GenerarReporteComentarios
        [Authorize(Roles = "1")]
        public IActionResult GenerarReporteComentarios()
        {
            try
            {
                // Cargar datos necesarios
                var comentarios = _context.Comentarios
                    .Where(c => c.Estado)
                    .Include(c => c.Usuario)
                    .Include(c => c.ServicioRealizado)
                        .ThenInclude(s => s.Servicio)
                    .Include(c => c.ServicioRealizado)
                        .ThenInclude(s => s.Vehiculo)
                    .OrderByDescending(c => c.FechaCreacion)
                    .ToList();

                // Filtrar comentarios válidos
                var comentariosValidos = comentarios
                    .Where(c => c.ServicioRealizado != null && c.ServicioRealizado.Servicio != null)
                    .ToList();

                // Estadísticas generales
                double promedioCalificacion = comentariosValidos.Any() ? comentariosValidos.Average(c => c.Calificacion) : 0;
                var distribucionCalificaciones = new Dictionary<int, int>
                {
                    { 5, comentariosValidos.Count(c => c.Calificacion == 5) },
                    { 4, comentariosValidos.Count(c => c.Calificacion == 4) },
                    { 3, comentariosValidos.Count(c => c.Calificacion == 3) },
                    { 2, comentariosValidos.Count(c => c.Calificacion == 2) },
                    { 1, comentariosValidos.Count(c => c.Calificacion == 1) }
                };

                // Preparar datos para el reporte en el mismo estilo que Inventario
                // Servicios mejor calificados
                List<ServicioEstadistica> serviciosMejorCalificados = new List<ServicioEstadistica>();

                // Agrupar comentarios por servicio
                var serviciosAgrupados = comentariosValidos
                    .GroupBy(c => new {
                        ServicioId = c.ServicioRealizado.ServicioId,
                        Nombre = c.ServicioRealizado.Servicio.Nombre
                    });

                foreach (var grupo in serviciosAgrupados)
                {
                    serviciosMejorCalificados.Add(new ServicioEstadistica
                    {
                        ServicioId = grupo.Key.ServicioId,
                        Nombre = grupo.Key.Nombre,
                        PromedioCalificacion = grupo.Average(c => c.Calificacion),
                        TotalComentarios = grupo.Count()
                    });
                }

                serviciosMejorCalificados = serviciosMejorCalificados
                    .OrderByDescending(s => s.PromedioCalificacion)
                    .Take(10)
                    .ToList();

                // Mecánicos mejor calificados
                List<MecanicoEstadistica> mecanicosMejorCalificados = new List<MecanicoEstadistica>();

                var mecanicos = _context.Usuarios
                    .Where(u => u.Rol == 2)
                    .ToList();

                foreach (var mecanico in mecanicos)
                {
                    var serviciosMecanico = comentariosValidos
                        .Where(c => c.ServicioRealizado.UsuarioId == mecanico.Id)
                        .ToList();

                    if (serviciosMecanico.Any())
                    {
                        mecanicosMejorCalificados.Add(new MecanicoEstadistica
                        {
                            MecanicoId = mecanico.Id,
                            Nombre = mecanico.Nombre,
                            PromedioCalificacion = serviciosMecanico.Average(c => c.Calificacion),
                            TotalServicios = serviciosMecanico.Count
                        });
                    }
                }

                mecanicosMejorCalificados = mecanicosMejorCalificados
                    .OrderByDescending(m => m.PromedioCalificacion)
                    .Take(10)
                    .ToList();

                // Últimos comentarios
                var ultimosComentarios = comentariosValidos.Take(5).ToList();

                // Configuración QuestPDF
                QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;

                // Generar PDF al estilo de Inventario
                var pdfBytes = Document.Create(container =>
                {
                container.Page(page =>
                {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                // Header
                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(column =>
                    {
                        column.Item().Text("Marland Auto Servicio").FontSize(20).Bold();
                        column.Item().Text($"Reporte de Comentarios - {DateTime.Now:dd/MM/yyyy}").FontSize(12);
                    });
                });

                // Content
                page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                {
                // Información general
                column.Item().Text("INFORMACIÓN GENERAL").Bold().FontSize(14);
                column.Item().Text($"Total de comentarios: {comentariosValidos.Count}").FontSize(12);
                column.Item().Text($"Calificación promedio: {promedioCalificacion:F1} de 5 estrellas").FontSize(12);
                column.Item().PaddingBottom(1, Unit.Centimetre);

                // Distribución de calificaciones
                column.Item().Text("DISTRIBUCIÓN DE CALIFICACIONES").Bold().FontSize(14);
                column.Item().PaddingBottom(0.5f, Unit.Centimetre);

                column.Item().Table(table =>
                {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn(2);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Medium).Element(c => c.Text("Calificación").Bold());
                    header.Cell().Background(Colors.Grey.Medium).Element(c => c.Text("Distribución").Bold());
                    header.Cell().Background(Colors.Grey.Medium).Element(c => c.Text("Cantidad").Bold());
                    header.Cell().Background(Colors.Grey.Medium).Element(c => c.Text("Porcentaje").Bold());
                });

                // Filas para cada calificación
                for (int i = 5; i >= 1; i--)
                {
                    var cantidad = distribucionCalificaciones[i];
                    var porcentaje = comentariosValidos.Count > 0 ? (double)cantidad / comentariosValidos.Count * 100 : 0;
                    var color = i switch
                    {
                        5 => Colors.Green.Medium,
                        4 => Colors.Blue.Medium,
                        3 => Colors.Yellow.Medium,
                        2 => Colors.Orange.Medium,
                        _ => Colors.Red.Medium
                    };

                    table.Cell().Element(c => c.Text($"{i} estrellas"));

                    // Barra gráfica de distribución
                    table.Cell().Element(cell =>
                    {
                        var width = (float)Math.Max(porcentaje, 1);
                        cell.Row(row =>
                        {
                            row.RelativeItem(width).Background(color).MinHeight(15);
                            row.RelativeItem((float)(100 - width)).MinHeight(15);
                        });
                    });

                    table.Cell().Element(c => c.Text(cantidad.ToString()));

                    table.Cell().Element(c => c.Text($"{porcentaje:F1}%"));
                    }
                });

                    column.Item().PaddingBottom(1, Unit.Centimetre);

                    // Servicios mejor calificados
                    if (serviciosMejorCalificados.Any())
                    {
                        column.Item().Text("SERVICIOS MEJOR CALIFICADOS").Bold().FontSize(14);
                        column.Item().PaddingBottom(0.5f, Unit.Centimetre);

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Medium).Element(c => c.Text("Servicio").Bold());
                                header.Cell().Background(Colors.Grey.Medium).Element(c => c.Text("Comentarios").Bold());
                                header.Cell().Background(Colors.Grey.Medium).Element(c => c.Text("Calificación").Bold());
                            });


                            foreach (var servicio in serviciosMejorCalificados)
                            {
                                table.Cell().Element(c => c.Text(servicio.Nombre));
                                table.Cell().Element(c => c.Text(servicio.TotalComentarios.ToString()));
                                table.Cell().Element(c => c.Text($"{servicio.PromedioCalificacion:F1} de 5.0"));
                            }
                        });

                        column.Item().PaddingBottom(1, Unit.Centimetre);
                    }

                    // Mecánicos mejor calificados
                    if (mecanicosMejorCalificados.Any())
                    {
                        column.Item().Text("MECÁNICOS MEJOR CALIFICADOS").Bold().FontSize(14);
                        column.Item().PaddingBottom(0.5f, Unit.Centimetre);

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Medium).Element(c => c.Text("Mecánico").Bold());
                                header.Cell().Background(Colors.Grey.Medium).Element(c => c.Text("Servicios").Bold());
                                header.Cell().Background(Colors.Grey.Medium).Element(c => c.Text("Calificación").Bold());
                            });

                            foreach (var mecanico in mecanicosMejorCalificados)
                            {
                                table.Cell().Element(c => c.Text(mecanico.Nombre));
                                table.Cell().Element(c => c.Text(mecanico.TotalServicios.ToString()));
                                table.Cell().Element(c => c.Text($"{mecanico.PromedioCalificacion:F1} de 5.0"));
                            }
                        });

                        column.Item().PaddingBottom(1, Unit.Centimetre);
                    }

                    // Últimos comentarios
                    if (ultimosComentarios.Any())
                    {
                        column.Item().Text("ÚLTIMOS COMENTARIOS").Bold().FontSize(14);
                        column.Item().PaddingBottom(0.5f, Unit.Centimetre);

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Medium).Element(c => c.Text("Cliente").Bold());
                                header.Cell().Background(Colors.Grey.Medium).Element(c => c.Text("Servicio").Bold());
                                header.Cell().Background(Colors.Grey.Medium).Element(c => c.Text("Calificación").Bold());
                                header.Cell().Background(Colors.Grey.Medium).Element(c => c.Text("Fecha").Bold());
                            });

                            foreach (var comentario in ultimosComentarios)
                            {
                                table.Cell().Element(c => c.Text(comentario.Usuario?.Nombre ?? "Desconocido"));
                                table.Cell().Element(c => c.Text(comentario.ServicioRealizado?.Servicio?.Nombre ?? "N/A"));
                                table.Cell().Element(c => c.Text($"{comentario.Calificacion}/5"));
                                table.Cell().Element(c => c.Text(comentario.FechaCreacion.ToString("dd/MM/yyyy")));
                            }
                        });
                    }
                });

                    // Footer
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Página ");
                        text.CurrentPageNumber();
                        text.Span(" de ");
                        text.TotalPages();
                    });
                });
                }).GeneratePdf();

                return File(pdfBytes, "application/pdf", $"ReporteComentarios_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al generar el reporte: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Comentarios/Dashboard - Para administradores
        [Authorize(Roles = "1")]
        public IActionResult Dashboard()
        {
            try
            {
                // Cargar datos
                var comentarios = _context.Comentarios
                    .Where(c => c.Estado)
                    .Include(c => c.Usuario)
                    .Include(c => c.ServicioRealizado)
                        .ThenInclude(s => s.Servicio)
                    .ToList()
                    .Where(c => c.ServicioRealizado?.Servicio != null) // Filtrar nulos
                    .ToList();

                // Estadísticas básicas
                ViewBag.TotalComentarios = comentarios.Count;
                ViewBag.PromedioCalificacion = comentarios.Any() ? comentarios.Average(c => c.Calificacion) : 0;
                ViewBag.ComentariosUltimaSemana = comentarios.Count(c => c.FechaCreacion >= DateTime.Now.AddDays(-7));

                // Distribución de calificaciones
                ViewBag.DistribucionCalificaciones = new Dictionary<int, int>
                {
                    { 5, comentarios.Count(c => c.Calificacion == 5) },
                    { 4, comentarios.Count(c => c.Calificacion == 4) },
                    { 3, comentarios.Count(c => c.Calificacion == 3) },
                    { 2, comentarios.Count(c => c.Calificacion == 2) },
                    { 1, comentarios.Count(c => c.Calificacion == 1) }
                };

                // Tendencia mensual
                var ultimosSeisMeses = Enumerable.Range(0, 6)
                    .Select(i => DateTime.Now.AddMonths(-i))
                    .Select(date => new
                    {
                        Mes = date.ToString("MMM yyyy"),
                        Fecha = date,
                        Total = comentarios.Count(c => c.FechaCreacion.Month == date.Month &&
                                                    c.FechaCreacion.Year == date.Year)
                    })
                    .Reverse()
                    .ToList();

                ViewBag.TendenciaMensual = ultimosSeisMeses;

                // Servicios mejor calificados
                var serviciosRealizados = _context.ServicioRealizado
                    .Where(sr => sr.Estado == 2)
                    .Include(sr => sr.Servicio)
                    .ToList()
                    .Where(sr => sr.Servicio != null)
                    .ToList();

                List<ServicioEstadistica> serviciosMejorCalificados = new List<ServicioEstadistica>();

                // Agrupar por servicio
                var serviciosConComentarios = comentarios
                    .GroupBy(c => new {
                        ServicioId = c.ServicioRealizado.ServicioId,
                        Nombre = c.ServicioRealizado.Servicio.Nombre
                    });

                foreach (var grupo in serviciosConComentarios)
                {
                    serviciosMejorCalificados.Add(new ServicioEstadistica
                    {
                        ServicioId = grupo.Key.ServicioId,
                        Nombre = grupo.Key.Nombre,
                        PromedioCalificacion = grupo.Average(c => c.Calificacion),
                        TotalComentarios = grupo.Count()
                    });
                }

                serviciosMejorCalificados = serviciosMejorCalificados
                    .Where(s => s.TotalComentarios >= 1)
                    .OrderByDescending(s => s.PromedioCalificacion)
                    .Take(5)
                    .ToList();

                ViewBag.ServiciosMejorCalificados = serviciosMejorCalificados;

                // Comentarios recientes
                ViewBag.ComentariosRecientes = comentarios
                    .OrderByDescending(c => c.FechaCreacion)
                    .Take(5)
                    .ToList();

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el dashboard: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Comentarios/Estadisticas
        [Authorize(Roles = "1,2")]
        public IActionResult Estadisticas()
        {
            try
            {
                // Cargar datos necesarios
                var comentarios = _context.Comentarios
                    .Where(c => c.Estado)
                    .ToList();

                var serviciosRealizados = _context.ServicioRealizado
                    .Where(sr => sr.Estado == 2)
                    .Include(sr => sr.Servicio)
                    .ToList()
                    .Where(sr => sr.Servicio != null)
                    .ToList();

                // Comentarios válidos
                var comentariosValidos = comentarios
                    .Where(c => serviciosRealizados.Any(sr => sr.id == c.ServicioRealizadoId))
                    .ToList();

                // Estadísticas por servicio
                var estadisticasPorServicio = new List<dynamic>();

                // Agrupar por servicio
                var serviciosAgrupados = from sr in serviciosRealizados
                                         join c in comentariosValidos on sr.id equals c.ServicioRealizadoId
                                         group c by sr.Servicio.Nombre into g
                                         select new
                                         {
                                             Servicio = g.Key,
                                             PromedioCalificacion = g.Average(c => c.Calificacion),
                                             TotalComentarios = g.Count(),
                                             Comentarios5Estrellas = g.Count(c => c.Calificacion == 5),
                                             Comentarios4Estrellas = g.Count(c => c.Calificacion == 4),
                                             Comentarios3Estrellas = g.Count(c => c.Calificacion == 3),
                                             Comentarios2Estrellas = g.Count(c => c.Calificacion == 2),
                                             Comentarios1Estrella = g.Count(c => c.Calificacion == 1)
                                         };

                estadisticasPorServicio = serviciosAgrupados
                    .OrderByDescending(x => x.PromedioCalificacion)
                    .ToList<dynamic>();

                ViewBag.EstadisticasPorServicio = estadisticasPorServicio;

                // Aspectos destacados más comunes
                var aspectosDestacados = comentariosValidos
                    .Where(c => !string.IsNullOrWhiteSpace(c.AspectosDestacados))
                    .SelectMany(c => c.AspectosDestacados.Split(',').Select(a => a.Trim()))
                    .GroupBy(a => a)
                    .Select(g => new { Aspecto = g.Key, Cantidad = g.Count() })
                    .OrderByDescending(x => x.Cantidad)
                    .Take(10)
                    .ToList();

                ViewBag.AspectosDestacados = aspectosDestacados;

                // Estadísticas por tiempo
                var ultimoAno = DateTime.Now.AddYears(-1);
                var comentariosPorMes = comentariosValidos
                    .Where(c => c.FechaCreacion >= ultimoAno)
                    .GroupBy(c => new { c.FechaCreacion.Year, c.FechaCreacion.Month })
                    .Select(g => new {
                        Fecha = new DateTime(g.Key.Year, g.Key.Month, 1),
                        Total = g.Count(),
                        PromedioCalificacion = g.Average(c => c.Calificacion)
                    })
                    .OrderBy(x => x.Fecha)
                    .ToList();

                ViewBag.ComentariosPorMes = comentariosPorMes;

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar las estadísticas: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // Método para verificar si un comentario existe
        private bool ComentarioExists(int id)
        {
            return _context?.Comentarios?.Any(e => e.Id == id) ?? false;
        }
    }
}
