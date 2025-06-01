using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    [Authorize]
    public class TareasController : Controller
    {
        private readonly DbamazonContext _context;

        public TareasController(DbamazonContext context)
        {
            _context = context;
        }

        // GET: Tareas - Lista todas las tareas (solo administradores)
        [Authorize(Roles = "1")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var tareas = await _context.Tareas
                    .Include(t => t.UsuarioCreador)
                    .Include(t => t.MecanicoAsignado)
                    .Include(t => t.Vehiculo)
                    .OrderByDescending(t => t.FechaCreacion)
                    .ToListAsync();

                return View(tareas);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar las tareas: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Tareas/MisTareas - Lista las tareas asignadas al mecánico actual
        [Authorize(Roles = "2")]
        public async Task<IActionResult> MisTareas()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var tareas = await _context.Tareas
                    .Include(t => t.UsuarioCreador)
                    .Include(t => t.Vehiculo)
                    .Where(t => t.MecanicoAsignadoId == userId)
                    .OrderByDescending(t => t.FechaCreacion)
                    .ToListAsync();

                return View(tareas);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar sus tareas: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Tareas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tarea = await _context.Tareas
                .Include(t => t.UsuarioCreador)
                .Include(t => t.MecanicoAsignado)
                .Include(t => t.Vehiculo)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tarea == null)
            {
                return NotFound();
            }

            // Verificar si el usuario puede ver esta tarea (administrador o mecánico asignado)
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = int.Parse(User.FindFirstValue(ClaimTypes.Role));

            if (userRole != 1 && tarea.MecanicoAsignadoId != userId)
            {
                return Forbid();
            }

            return View(tarea);
        }

        // GET: Tareas/Create
        [Authorize(Roles = "1")]
        public IActionResult Create()
        {
            try
            {
                // Cargar lista de mecánicos (usuarios con rol = 2)
                ViewBag.Mecanicos = new SelectList(_context.Usuarios.Where(u => u.Rol == 2).ToList(), "Id", "Nombre");

                // Cargar lista de vehículos
                ViewBag.Vehiculos = new SelectList(_context.Vehiculos
                    .Select(v => new { v.Id, Info = $"{v.Marca} {v.Modelo} ({v.Placa})" })
                    .ToList(), "Id", "Info");

                // Estados predefinidos para tareas
                ViewBag.Estados = new SelectList(new[] { "Pendiente", "En progreso", "Completada", "Cancelada" });

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el formulario: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Tareas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> Create(Tarea tarea)
        {
            try
            {
                // Asignar el usuario creador (administrador actual)
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                tarea.UsuarioCreadorId = userId;
                tarea.FechaCreacion = DateTime.Now;

                // Limpiar errores de validación específicos
                ModelState.Remove("UsuarioCreador");
                ModelState.Remove("MecanicoAsignado");
                ModelState.Remove("Vehiculo");

                // Validación manual si es necesaria
                if (string.IsNullOrEmpty(tarea.Titulo))
                {
                    ModelState.AddModelError("Titulo", "El título es obligatorio");
                }

                if (ModelState.IsValid)
                {
                    // Información de debugging
                    System.Diagnostics.Debug.WriteLine($"Guardando tarea: {tarea.Titulo}, MecanicoId: {tarea.MecanicoAsignadoId}, VehiculoId: {tarea.VehiculoId}");

                    _context.Add(tarea);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Tarea creada exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Para depuración: Ver qué errores específicos hay en el ModelState
                    var errores = string.Join("; ", ModelState.Values
                        .SelectMany(x => x.Errors)
                        .Select(x => x.ErrorMessage));

                    TempData["ErrorMessage"] = "Error de validación: " + errores;
                    System.Diagnostics.Debug.WriteLine("Errores de validación: " + errores);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al crear la tarea: " + ex.Message;
                System.Diagnostics.Debug.WriteLine("Error completo: " + ex.ToString());
            }

            // Si llegamos aquí, hubo un error - recargar datos para la vista
            ViewBag.Mecanicos = new SelectList(_context.Usuarios.Where(u => u.Rol == 2).ToList(), "Id", "Nombre", tarea.MecanicoAsignadoId);
            ViewBag.Vehiculos = new SelectList(_context.Vehiculos
                .Select(v => new { v.Id, Info = $"{v.Marca} {v.Modelo} ({v.Placa})" })
                .ToList(), "Id", "Info", tarea.VehiculoId);
            ViewBag.Estados = new SelectList(new[] { "Pendiente", "En progreso", "Completada", "Cancelada" }, tarea.Estado);

            return View(tarea);
        }

        // GET: Tareas/Edit/5
        [Authorize(Roles = "1")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tarea = await _context.Tareas.FindAsync(id);
            if (tarea == null)
            {
                return NotFound();
            }

            ViewBag.Mecanicos = new SelectList(_context.Usuarios.Where(u => u.Rol == 2).ToList(), "Id", "Nombre", tarea.MecanicoAsignadoId);
            ViewBag.Vehiculos = new SelectList(_context.Vehiculos
                .Select(v => new { v.Id, Info = $"{v.Marca} {v.Modelo} ({v.Placa})" })
                .ToList(), "Id", "Info", tarea.VehiculoId);
            ViewBag.Estados = new SelectList(new[] { "Pendiente", "En progreso", "Completada", "Cancelada" }, tarea.Estado);

            return View(tarea);
        }

        // POST: Tareas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titulo,Descripcion,Estado,FechaVencimiento,MecanicoAsignadoId,VehiculoId")] Tarea tarea)
        {
            if (id != tarea.Id)
            {
                return NotFound();
            }

            try
            {
                // Obtener la tarea original para mantener algunos datos
                var tareaOriginal = await _context.Tareas.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
                if (tareaOriginal == null)
                {
                    return NotFound();
                }

                // Mantener datos que no deben cambiar
                tarea.UsuarioCreadorId = tareaOriginal.UsuarioCreadorId;
                tarea.FechaCreacion = tareaOriginal.FechaCreacion;

                // Limpiar errores de validación específicos
                ModelState.Remove("UsuarioCreador");
                ModelState.Remove("MecanicoAsignado");
                ModelState.Remove("Vehiculo");

                if (ModelState.IsValid)
                {
                    _context.Update(tarea);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Tarea actualizada exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    var errores = string.Join("; ", ModelState.Values
                        .SelectMany(x => x.Errors)
                        .Select(x => x.ErrorMessage));

                    TempData["ErrorMessage"] = "Error de validación: " + errores;
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TareaExists(tarea.Id))
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
                TempData["ErrorMessage"] = "Error al actualizar la tarea: " + ex.Message;
            }

            // Si llegamos aquí, hubo un error
            ViewBag.Mecanicos = new SelectList(_context.Usuarios.Where(u => u.Rol == 2).ToList(), "Id", "Nombre", tarea.MecanicoAsignadoId);
            ViewBag.Vehiculos = new SelectList(_context.Vehiculos
                .Select(v => new { v.Id, Info = $"{v.Marca} {v.Modelo} ({v.Placa})" })
                .ToList(), "Id", "Info", tarea.VehiculoId);
            ViewBag.Estados = new SelectList(new[] { "Pendiente", "En progreso", "Completada", "Cancelada" }, tarea.Estado);

            return View(tarea);
        }

        // GET: Tareas/ActualizarEstado/5
        [Authorize(Roles = "1,2")]
        public async Task<IActionResult> ActualizarEstado(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tarea = await _context.Tareas
                .Include(t => t.MecanicoAsignado)
                .Include(t => t.Vehiculo)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tarea == null)
            {
                return NotFound();
            }

            // Verificar permisos: administrador o mecánico asignado
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = int.Parse(User.FindFirstValue(ClaimTypes.Role));

            if (userRole != 1 && tarea.MecanicoAsignadoId != userId)
            {
                return Forbid();
            }

            ViewBag.Estados = new SelectList(new[] { "Pendiente", "En progreso", "Completada", "Cancelada" }, tarea.Estado);

            return View(tarea);
        }

        // POST: Tareas/ActualizarEstado/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "1,2")]
        public async Task<IActionResult> ActualizarEstado(int id, string Estado)
        {
            try
            {
                var tarea = await _context.Tareas.FindAsync(id);
                if (tarea == null)
                {
                    return NotFound();
                }

                // Verificar permisos: administrador o mecánico asignado
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var userRole = int.Parse(User.FindFirstValue(ClaimTypes.Role));

                if (userRole != 1 && tarea.MecanicoAsignadoId != userId)
                {
                    return Forbid();
                }

                tarea.Estado = Estado;
                _context.Update(tarea);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Estado de la tarea actualizado exitosamente.";

                // Redirigir según el rol
                if (userRole == 1)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return RedirectToAction(nameof(MisTareas));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al actualizar el estado: " + ex.Message;
                return RedirectToAction(nameof(ActualizarEstado), new { id });
            }
        }

        // GET: Tareas/Delete/5
        [Authorize(Roles = "1")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tarea = await _context.Tareas
                .Include(t => t.UsuarioCreador)
                .Include(t => t.MecanicoAsignado)
                .Include(t => t.Vehiculo)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tarea == null)
            {
                return NotFound();
            }

            return View(tarea);
        }

        // POST: Tareas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var tarea = await _context.Tareas.FindAsync(id);
                if (tarea == null)
                {
                    return NotFound();
                }

                _context.Tareas.Remove(tarea);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Tarea eliminada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar la tarea: " + ex.Message;
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        public IActionResult GenerarReporteTareas()
        {
            try
            {
                var tareas = _context.Tareas
                    .Include(t => t.UsuarioCreador)
                    .Include(t => t.MecanicoAsignado)
                    .Include(t => t.Vehiculo)
                    .OrderByDescending(t => t.FechaCreacion)
                    .ToList();

                // Generamos el PDF usando QuestPDF
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
                                column.Item().Text($"Reporte de Tareas - {DateTime.Now:dd/MM/yyyy}").FontSize(12);
                            });
                        });

                        // Content
                        page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                        {
                            // Información general
                            column.Item().Text("INFORMACIÓN GENERAL").Bold().FontSize(14);
                            column.Item().Text($"Total de tareas: {tareas.Count}").FontSize(12);
                            column.Item().Text($"Tareas pendientes: {tareas.Count(t => t.Estado == "Pendiente")}").FontSize(12);
                            column.Item().Text($"Tareas en progreso: {tareas.Count(t => t.Estado == "En progreso")}").FontSize(12);
                            column.Item().Text($"Tareas completadas: {tareas.Count(t => t.Estado == "Completada")}").FontSize(12);
                            column.Item().Text($"Tareas canceladas: {tareas.Count(t => t.Estado == "Cancelada")}").FontSize(12);
                            column.Item().PaddingBottom(1, Unit.Centimetre);

                            // Tabla de tareas
                            column.Item().Table(table =>
                            {
                                // Definición de columnas
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3); // Título
                                    columns.RelativeColumn(1); // Estado
                                    columns.RelativeColumn(2); // Mecánico
                                    columns.RelativeColumn(2); // Vehículo
                                    columns.RelativeColumn(2); // Fecha vencimiento
                                });

                                // Encabezado de tabla
                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Medium).Text("Título").Bold();
                                    header.Cell().Background(Colors.Grey.Medium).Text("Estado").Bold();
                                    header.Cell().Background(Colors.Grey.Medium).Text("Mecánico").Bold();
                                    header.Cell().Background(Colors.Grey.Medium).Text("Vehículo").Bold();
                                    header.Cell().Background(Colors.Grey.Medium).Text("Vencimiento").Bold();
                                });

                                // Filas de la tabla
                                foreach (var tarea in tareas)
                                {
                                    var bgColor = Colors.White;

                                    switch (tarea.Estado)
                                    {
                                        case "Pendiente":
                                            bgColor = Colors.Yellow.Lighten3;
                                            break;
                                        case "En progreso":
                                            bgColor = Colors.Blue.Lighten3;
                                            break;
                                        case "Completada":
                                            bgColor = Colors.Green.Lighten3;
                                            break;
                                        case "Cancelada":
                                            bgColor = Colors.Red.Lighten3;
                                            break;
                                    }

                                    // Título
                                    table.Cell().Background(bgColor).Text(tarea.Titulo);

                                    // Estado
                                    table.Cell().Background(bgColor).Text(tarea.Estado);

                                    // Mecánico
                                    table.Cell().Background(bgColor).Text(tarea.MecanicoAsignado?.Nombre ?? "Sin asignar");

                                    // Vehículo
                                    string vehiculoInfo = "N/A";
                                    if (tarea.Vehiculo != null)
                                    {
                                        vehiculoInfo = $"{tarea.Vehiculo.Marca} {tarea.Vehiculo.Modelo} ({tarea.Vehiculo.Placa})";
                                    }
                                    table.Cell().Background(bgColor).Text(vehiculoInfo);

                                    // Fecha vencimiento
                                    string fechaVencimiento = "Sin fecha";
                                    if (tarea.FechaVencimiento.HasValue)
                                    {
                                        fechaVencimiento = tarea.FechaVencimiento.Value.ToString("dd/MM/yyyy");
                                    }
                                    table.Cell().Background(bgColor).Text(fechaVencimiento);
                                }
                            });

                            // Sección para tareas pendientes y en progreso
                            var tareasActivas = tareas.Where(t => t.Estado == "Pendiente" || t.Estado == "En progreso").ToList();
                            if (tareasActivas.Any())
                            {
                                column.Item().PaddingTop(1, Unit.Centimetre);
                                column.Item().Text("TAREAS ACTIVAS (Pendientes y En Progreso)").Bold().FontSize(14);
                                column.Item().PaddingBottom(0.5f, Unit.Centimetre);

                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3); // Título
                                        columns.RelativeColumn(1); // Estado
                                        columns.RelativeColumn(2); // Mecánico
                                        columns.RelativeColumn(2); // Vehículo
                                        columns.RelativeColumn(2); // Fecha vencimiento
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Blue.Medium).Text("Título").Bold();
                                        header.Cell().Background(Colors.Blue.Medium).Text("Estado").Bold();
                                        header.Cell().Background(Colors.Blue.Medium).Text("Mecánico").Bold();
                                        header.Cell().Background(Colors.Blue.Medium).Text("Vehículo").Bold();
                                        header.Cell().Background(Colors.Blue.Medium).Text("Vencimiento").Bold();
                                    });

                                    foreach (var tarea in tareasActivas)
                                    {
                                        var bgColor = tarea.Estado == "Pendiente" ? Colors.Yellow.Lighten3 : Colors.Blue.Lighten3;

                                        // Título
                                        table.Cell().Background(bgColor).Text(tarea.Titulo);

                                        // Estado
                                        table.Cell().Background(bgColor).Text(tarea.Estado);

                                        // Mecánico
                                        table.Cell().Background(bgColor).Text(tarea.MecanicoAsignado?.Nombre ?? "Sin asignar");

                                        // Vehículo
                                        string vehiculoInfo = "N/A";
                                        if (tarea.Vehiculo != null)
                                        {
                                            vehiculoInfo = $"{tarea.Vehiculo.Marca} {tarea.Vehiculo.Modelo} ({tarea.Vehiculo.Placa})";
                                        }
                                        table.Cell().Background(bgColor).Text(vehiculoInfo);

                                        // Fecha vencimiento
                                        string fechaVencimiento = "Sin fecha";
                                        if (tarea.FechaVencimiento.HasValue)
                                        {
                                            fechaVencimiento = tarea.FechaVencimiento.Value.ToString("dd/MM/yyyy");
                                        }
                                        table.Cell().Background(bgColor).Text(fechaVencimiento);
                                    }
                                });
                            }

                            // Sección para tareas por mecánico
                            column.Item().PaddingTop(1, Unit.Centimetre);
                            column.Item().Text("DISTRIBUCIÓN DE TAREAS POR MECÁNICO").Bold().FontSize(14);
                            column.Item().PaddingBottom(0.5f, Unit.Centimetre);

                            // Agrupar tareas por mecánico
                            var tareasPorMecanico = tareas
                                .Where(t => t.MecanicoAsignado != null)
                                .GroupBy(t => t.MecanicoAsignado.Nombre)
                                .Select(g => new
                                {
                                    Mecanico = g.Key,
                                    TotalTareas = g.Count(),
                                    Pendientes = g.Count(t => t.Estado == "Pendiente"),
                                    EnProgreso = g.Count(t => t.Estado == "En progreso"),
                                    Completadas = g.Count(t => t.Estado == "Completada"),
                                    Canceladas = g.Count(t => t.Estado == "Cancelada")
                                })
                                .ToList();

                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3); // Mecánico
                                    columns.RelativeColumn(1); // Total
                                    columns.RelativeColumn(1); // Pendientes
                                    columns.RelativeColumn(1); // En progreso
                                    columns.RelativeColumn(1); // Completadas
                                    columns.RelativeColumn(1); // Canceladas
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Medium).Text("Mecánico").Bold();
                                    header.Cell().Background(Colors.Grey.Medium).Text("Total").Bold();
                                    header.Cell().Background(Colors.Grey.Medium).Text("Pendientes").Bold();
                                    header.Cell().Background(Colors.Grey.Medium).Text("En Progreso").Bold();
                                    header.Cell().Background(Colors.Grey.Medium).Text("Completadas").Bold();
                                    header.Cell().Background(Colors.Grey.Medium).Text("Canceladas").Bold();
                                });

                                foreach (var registro in tareasPorMecanico)
                                {
                                    table.Cell().Text(registro.Mecanico);
                                    table.Cell().Text(registro.TotalTareas.ToString());
                                    table.Cell().Text(registro.Pendientes.ToString());
                                    table.Cell().Text(registro.EnProgreso.ToString());
                                    table.Cell().Text(registro.Completadas.ToString());
                                    table.Cell().Text(registro.Canceladas.ToString());
                                }
                            });
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

                return File(pdfBytes, "application/pdf", "ReporteTareas.pdf");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al generar el reporte: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private bool TareaExists(int id)
        {
            return _context.Tareas.Any(e => e.Id == id);
        }
    }
}