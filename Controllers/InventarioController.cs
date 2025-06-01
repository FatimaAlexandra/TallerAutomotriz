using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using amazon.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace amazon.Controllers
{
    [Authorize(Roles = "1,2")] // Solo administradores y mecánicos pueden acceder
    public class InventarioController : Controller
    {
        private readonly DbamazonContext _context;

        public InventarioController(DbamazonContext context)
        {
            _context = context;
        }

        // GET: Inventario
        public async Task<IActionResult> Index()
        {
            try
            {
                // Asegúrate de que esta consulta está trayendo resultados
                var inventario = await _context.Inventario
                    .Include(i => i.Producto)
                    .OrderBy(i => i.Producto.Nombre)
                    .ToListAsync();

                // Para depuración, añade esto temporalmente
                ViewBag.InventarioCount = inventario.Count;
                ViewBag.DebugMessage = "Se cargaron " + inventario.Count + " registros";

                return View(inventario);
            }
            catch (Exception ex)
            {
                // Registra el error para depuración
                ViewBag.ErrorMessage = "Error al cargar el inventario: " + ex.Message;
                return View(new List<Inventario>());
            }
        }

        // GET: Inventario/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventario = await _context.Inventario
                .Include(i => i.Producto)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inventario == null)
            {
                return NotFound();
            }

            return View(inventario);
        }

        // GET: Inventario/ActualizarStock/5
        public async Task<IActionResult> ActualizarStock(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventario = await _context.Inventario
                .Include(i => i.Producto)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inventario == null)
            {
                return NotFound();
            }

            ViewBag.ProductoNombre = inventario.Producto?.Nombre;
            ViewBag.StockActual = inventario.Cantidad;

            return View(inventario);
        }

        // POST: Inventario/ActualizarStock/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarStock(int id, [Bind("Id,ProductoId,Cantidad")] Inventario inventario)
        {
            if (id != inventario.Id)
            {
                return NotFound();
            }

            try
            {
                var inventarioActual = await _context.Inventario.FindAsync(id);
                if (inventarioActual == null)
                {
                    return NotFound();
                }

                // Actualizar cantidad y fecha
                inventarioActual.Cantidad = inventario.Cantidad;
                inventarioActual.FechaActualizacion = DateTime.Now;

                _context.Update(inventarioActual);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Stock actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al actualizar el stock: " + ex.Message;
                return RedirectToAction(nameof(ActualizarStock), new { id });
            }
        }

        // GET: Inventario/AjustarStock/5
        public async Task<IActionResult> AjustarStock(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventario = await _context.Inventario
                .Include(i => i.Producto)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inventario == null)
            {
                return NotFound();
            }

            ViewBag.ProductoNombre = inventario.Producto?.Nombre;
            ViewBag.StockActual = inventario.Cantidad;
            ViewBag.ProductoId = inventario.ProductoId;
            ViewBag.InventarioId = inventario.Id;

            return View();
        }

        // POST: Inventario/AjustarStock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AjustarStock(int inventarioId, int cantidad, string tipoAjuste)
        {
            try
            {
                var inventario = await _context.Inventario.FindAsync(inventarioId);
                if (inventario == null)
                {
                    return NotFound();
                }

                // Actualizar cantidad según el tipo de ajuste
                if (tipoAjuste == "agregar")
                {
                    inventario.Cantidad += cantidad;
                }
                else if (tipoAjuste == "restar")
                {
                    if (inventario.Cantidad < cantidad)
                    {
                        TempData["ErrorMessage"] = "No hay suficiente stock para restar esa cantidad.";
                        return RedirectToAction(nameof(AjustarStock), new { id = inventarioId });
                    }
                    inventario.Cantidad -= cantidad;
                }

                inventario.FechaActualizacion = DateTime.Now;

                _context.Update(inventario);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Stock ajustado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al ajustar el stock: " + ex.Message;
                return RedirectToAction(nameof(AjustarStock), new { id = inventarioId });
            }
        }

        // GET: Inventario/BajoStock
        public async Task<IActionResult> BajoStock()
        {
            try
            {
                // Por defecto, consideramos "bajo stock" cuando hay menos de 10 unidades
                var inventarioBajo = await _context.Inventario
                    .Include(i => i.Producto)
                    .Where(i => i.Cantidad < 10)
                    .OrderBy(i => i.Cantidad)
                    .ToListAsync();

                return View(inventarioBajo);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el inventario con bajo stock: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }








        // GET: Inventario/GenerarReporteInventario
        public IActionResult GenerarReporteInventario()
        {
            try
            {
                var inventario = _context.Inventario
                    .Include(i => i.Producto)
                    .OrderBy(i => i.Producto.Nombre)
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
                                column.Item().Text($"Reporte de Inventario - {DateTime.Now:dd/MM/yyyy}").FontSize(12);
                            });
                        });

                        // Content
                        page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                        {
                            // Información general
                            column.Item().Text("INFORMACIÓN GENERAL").Bold().FontSize(14);
                            column.Item().Text($"Total de productos en inventario: {inventario.Count}").FontSize(12);
                            column.Item().Text($"Cantidad total de unidades: {inventario.Sum(i => i.Cantidad)}").FontSize(12);
                            column.Item().Text($"Productos con bajo stock (menos de 10 unidades): {inventario.Count(i => i.Cantidad < 10)}").FontSize(12);
                            column.Item().PaddingBottom(1, Unit.Centimetre);

                            // Tabla de inventario
                            column.Item().Table(table =>
                            {
                                // Definición de columnas
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                // Encabezado de tabla
                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Medium).Text("Producto").Bold();
                                    header.Cell().Background(Colors.Grey.Medium).Text("Tipo").Bold();
                                    header.Cell().Background(Colors.Grey.Medium).Text("Cantidad").Bold();
                                    header.Cell().Background(Colors.Grey.Medium).Text("Última Actualización").Bold();
                                });

                                // Filas de la tabla
                                foreach (var item in inventario)
                                {
                                    // La clave es crear un nuevo contexto para cada fila
                                    table.Cell().Text(item.Producto.Nombre);
                                    table.Cell().Text(item.Producto.Tipo);
                                    table.Cell().Text(item.Cantidad.ToString());
                                    table.Cell().Text(item.FechaActualizacion.ToString("dd/MM/yyyy"));
                                }
                            });

                            // Productos con bajo stock - en una nueva sección
                            var productosBajoStock = inventario.Where(i => i.Cantidad < 10).ToList();
                            if (productosBajoStock.Any())
                            {
                                column.Item().PaddingTop(1, Unit.Centimetre);
                                column.Item().Text("PRODUCTOS CON BAJO STOCK").Bold().FontSize(14);
                                column.Item().PaddingBottom(0.5f, Unit.Centimetre);

                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn();
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Red.Medium).Text("Producto").Bold();
                                        header.Cell().Background(Colors.Red.Medium).Text("Tipo").Bold();
                                        header.Cell().Background(Colors.Red.Medium).Text("Cantidad").Bold();
                                    });

                                    foreach (var item in productosBajoStock)
                                    {
                                        table.Cell().Text(item.Producto.Nombre);
                                        table.Cell().Text(item.Producto.Tipo);
                                        table.Cell().Text(item.Cantidad.ToString());
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

                return File(pdfBytes, "application/pdf", "ReporteInventario.pdf");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al generar el reporte: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

    }
    }