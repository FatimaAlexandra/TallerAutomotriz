using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using amazon.Models;
using TuNamespace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using amazon.Services;

namespace amazon.Controllers
{
    public class FacturacionController : Controller
    {
        //variables de solo lectura para la inyeccion de dependencia

        private readonly DbamazonContext _context;
        private readonly PdfService _pdfService;

        public FacturacionController(DbamazonContext context)
        {
            _context = context;
            _pdfService = new PdfService();
        }

        public async Task<IActionResult> Index()
        {
            var facturaciones = await _context.Facturacion
                .Include(f => f.Usuario)
                .Include(f => f.DetalleFacturacion)
                    .ThenInclude(d => d.ServicioRealizado)
                        .ThenInclude(s => s.Servicio)
                .OrderByDescending(f => f.Id)
                .ToListAsync();

            return View(facturaciones);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var factura = await _context.Facturacion
                    .Include(f => f.Usuario)
                    .Include(f => f.DetalleFacturacion)
                        .ThenInclude(d => d.ServicioRealizado)
                            .ThenInclude(s => s.Servicio)
                    .FirstOrDefaultAsync(f => f.Id == id);

                if (factura == null)
                {
                    return NotFound();
                }

                return PartialView("_FacturaDetailsPartial", factura);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error al cargar los detalles de la factura");
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            try
            {
                var usuarios = _context.Usuarios
                    .Where(u => u.Rol == 3)
                    .Select(u => new { u.Id, u.Nombre, u.Telefono, u.Correo })
                    .ToList();

                ViewBag.Usuarios = usuarios;
                return View(new Facturacion { FechaFacturacion = DateTime.Now });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar la página: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<JsonResult> BuscarClientePorDui(string dui)
        {
            try
            {
                var cliente = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Dui == dui && u.Rol == 3); //linq para buscar solos los usuarios del rol de cliente

                if (cliente != null)
                {
                    return Json(new
                    {
                        success = true,
                        id = cliente.Id,
                        nombre = cliente.Nombre,
                        telefono = cliente.Telefono,
                        correo = cliente.Correo
                    });
                }
                return Json(new { success = false, message = "Cliente no encontrado" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al buscar cliente: " + ex.Message });
            }
        }

        //aqui se busca los servicios del cliente
        [HttpGet]
        public async Task<JsonResult> GetServiciosDisponibles(int usuarioId)
        {
            try
            {
                var serviciosDisponibles = await _context.ServicioRealizado
                    .Include(s => s.Servicio)
                    .Include(s => s.Vehiculo)
                    .Where(s => s.UsuarioId == usuarioId && s.Estado == 1)
                    .Select(s => new
                    {
                        id = s.id,
                        servicioNombre = s.Servicio.Nombre,
                        vehiculo = new
                        {
                            marca = s.Vehiculo.Marca,
                            modelo = s.Vehiculo.Modelo,
                            placa = s.Vehiculo.Placa
                        },
                        precio = s.Precio,
                        fecha = s.Fecha
                    })
                    .ToListAsync();

                return Json(new { success = true, servicios = serviciosDisponibles });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al cargar servicios: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //aqui se crea la factura
        public async Task<IActionResult> Create(int UsuarioId, DateTime FechaFacturacion, string MetodoPago, decimal MontoTotal, string serviciosSeleccionados)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return Json(new { success = false, message = "Errores de validación: " + string.Join(", ", errors) });
                }

                if (string.IsNullOrEmpty(serviciosSeleccionados))
                {
                    return Json(new { success = false, message = "No hay servicios seleccionados" });
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var serviciosIds = serviciosSeleccionados.Split(',')
                        .Select(id => int.TryParse(id, out int servicioId) ? servicioId : -1)
                        .Where(id => id != -1)
                        .ToList();

                    var serviciosValidos = await _context.ServicioRealizado
                        .Where(s => serviciosIds.Contains(s.id) && s.Estado == 1)
                        .ToListAsync();

                    if (!serviciosValidos.Any())
                    {
                        return Json(new { success = false, message = "No se encontraron servicios válidos" });
                    }

                    var nuevaFactura = new Facturacion
                    {
                        UsuarioId = UsuarioId,
                        FechaFacturacion = FechaFacturacion,
                        MetodoPago = MetodoPago,
                        MontoTotal = serviciosValidos.Sum(s => s.Precio),
                        EstadoFactura = "Pendiente"
                    };

                    _context.Facturacion.Add(nuevaFactura);
                    await _context.SaveChangesAsync();

                    foreach (var servicio in serviciosValidos)
                    {
                        var detalle = new DetalleFacturacion
                        {
                            FacturaId = nuevaFactura.Id,
                            ServicioRealizadoId = servicio.id,
                            PrecioUnitario = servicio.Precio,
                            Subtotal = servicio.Precio
                        };
                        _context.DetalleFacturacion.Add(detalle);

                        servicio.Estado = 2;
                        _context.Update(servicio);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new
                    {
                        success = true,
                        message = $"Factura #{nuevaFactura.Id} generada exitosamente",
                        redirectUrl = Url.Action("Index", "Facturacion")
                    });
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al generar la factura: {ex.Message}" });
            }
        }
        //para editar la factura
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var facturacion = await _context.Facturacion
                .Include(f => f.Usuario)
                .Include(f => f.DetalleFacturacion)
                    .ThenInclude(d => d.ServicioRealizado)
                        .ThenInclude(s => s.Servicio)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (facturacion == null)
            {
                return NotFound();
            }

            ViewBag.MetodosPago = new List<string> { "Efectivo", "Tarjeta de Crédito", "Transferencia" };
            ViewBag.EstadosFactura = new List<string> { "Pendiente", "Pagado", "Cancelado" };
            ViewBag.Usuario = facturacion.Usuario;
            ViewBag.MontoTotal = facturacion.MontoTotal;

            return View(facturacion);
        }

        //para editar la factura
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UsuarioId,FechaFacturacion,MontoTotal,MetodoPago,EstadoFactura")] Facturacion facturacion)
        {
            if (id != facturacion.Id)
            {
                return NotFound();
            }

            try
            {
                var facturaExistente = await _context.Facturacion
                    .Include(f => f.DetalleFacturacion)
                    .FirstOrDefaultAsync(f => f.Id == id);

                if (facturaExistente == null)
                {
                    return NotFound();
                }

                facturaExistente.MetodoPago = facturacion.MetodoPago;
                facturaExistente.EstadoFactura = facturacion.EstadoFactura;
                facturaExistente.FechaFacturacion = facturacion.FechaFacturacion;

                _context.Update(facturaExistente);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Factura actualizada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al actualizar la factura: " + ex.Message);
            }

            ViewBag.MetodosPago = new List<string> { "Efectivo", "Tarjeta de Crédito", "Transferencia" };
            ViewBag.EstadosFactura = new List<string> { "Pendiente", "Pagado", "Cancelado" };
            var usuario = await _context.Usuarios.FindAsync(facturacion.UsuarioId);
            ViewBag.Usuario = usuario;

            return View(facturacion);
        }
        //para eliminar la factura
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var facturacion = await _context.Facturacion
                .Include(f => f.Usuario)
                .Include(f => f.DetalleFacturacion)
                    .ThenInclude(d => d.ServicioRealizado)
                        .ThenInclude(s => s.Servicio)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (facturacion == null)
            {
                return NotFound();
            }

            return View(facturacion);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var facturacion = await _context.Facturacion
                    .Include(f => f.DetalleFacturacion)
                    .FirstOrDefaultAsync(f => f.Id == id);

                if (facturacion == null)
                {
                    return NotFound();
                }

                foreach (var detalle in facturacion.DetalleFacturacion)
                {
                    var servicio = await _context.ServicioRealizado.FindAsync(detalle.ServicioRealizadoId);
                    if (servicio != null)
                    {
                        servicio.Estado = 1;
                        _context.Update(servicio);
                    }
                }

                _context.DetalleFacturacion.RemoveRange(facturacion.DetalleFacturacion);
                _context.Facturacion.Remove(facturacion);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "Factura eliminada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "Error al eliminar la factura: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
        //para descarga del pdf
        [HttpGet]
        public async Task<IActionResult> DownloadPdf(int id)
        {
            var factura = await _context.Facturacion
                .Include(f => f.Usuario)
                .Include(f => f.DetalleFacturacion)
                    .ThenInclude(d => d.ServicioRealizado)
                        .ThenInclude(s => s.Servicio)
                .FirstOrDefaultAsync(f => f.Id == id);
            if (factura == null)
                return NotFound();
            var pdfBytes = _pdfService.GenerateInvoicePdf(factura);
            return File(pdfBytes, "application/pdf", $"Factura-{factura.Id}.pdf");
        }


        //validacion de la existencia de la factura

        private bool FacturacionExists(int id)
        {
            return _context.Facturacion.Any(e => e.Id == id);
        }

        private void LogModelStateErrors()
        {
            var errors = string.Join("; ", ModelState.Values
                .SelectMany(x => x.Errors)
                .Select(x => x.ErrorMessage));
            System.Diagnostics.Debug.WriteLine($"ModelState Errors: {errors}");
        }
    }
}