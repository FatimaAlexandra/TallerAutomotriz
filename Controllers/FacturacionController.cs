using amazon.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TuNamespace.Models;

namespace amazon.Controllers
{
    public class FacturacionController : Controller
    {
        private readonly DbamazonContext _context;

        public FacturacionController(DbamazonContext context)
        {
            _context = context;
        }

        // GET: Facturacion
        public IActionResult Index()
        {
            var facturaciones = _context.Facturacion
                .Include(f => f.Usuario) // Incluir la relación con Usuario
                .ToList(); 

            return View(facturaciones);
        }

  
        public IActionResult Details(int id)
        {
            var factura = _context.Facturacion
                                  .Include(f => f.Usuario)  // Incluye el usuario relacionado
                                  .FirstOrDefault(f => f.Id == id);

            if (factura == null)
            {
                return NotFound();
            }

            return PartialView("_FacturaDetailsPartial", factura);  // Devolver la vista parcial con los detalles de la factura
        }






        public IActionResult Create()
        {
            // Obtener los usuarios con rol = 3 (Cliente)
            var usuarios = _context.Usuarios
                .Where(u => u.Rol == 3) // Filtra usuarios con Rol = 3
                .ToList();

            ViewData["Usuarios"] = usuarios; // Almacena los usuarios en ViewData
            return View();
        }


        // Acción para buscar clientes por DUI
        [HttpGet]
        public JsonResult BuscarClientePorDui(string dui)
        {
            var cliente = _context.Usuarios.FirstOrDefault(u => u.Dui == dui && u.Rol == 3); // Filtrar solo clientes por DUI

            if (cliente != null)
            {
                return Json(new { id = cliente.Id, nombre = cliente.Nombre });
            }
            return Json(null);
        }

        // Acción para obtener los servicios realizados del día actual para un cliente específico
        [HttpGet]
        public IActionResult ServiciosDelDia(int usuarioId)
        {
            var serviciosDelDia = _context.ServicioRealizado
                .Include(s => s.Servicio)
                .Include(s => s.Vehiculo)
                .Where(s => s.UsuarioId == usuarioId && s.Fecha == DateTime.Now.ToString("yyyy-MM-dd") && s.Estado == 1)
                .ToList();

            return PartialView("_ServiciosRealizadosPartial", serviciosDelDia);
        }

        // Acción para crear la factura
        [HttpPost]
        public IActionResult Create(Facturacion facturacion, List<int> serviciosSeleccionados)
        {
            if (ModelState.IsValid)
            {
                // Guardar la factura
                _context.Facturacion.Add(facturacion);
                _context.SaveChanges();

                // Asignar los servicios a la factura y cambiar el estado a completado
                foreach (var servicioId in serviciosSeleccionados)
                {
                    var servicio = _context.ServicioRealizado.Find(servicioId);
                    if (servicio != null)
                    {
                        // Crear el detalle de la factura
                        var detalle = new DetalleFacturacion
                        {
                            FacturaId = facturacion.Id,
                            ServicioRealizadoId = servicio.id,
                            Subtotal = servicio.Precio
                        };
                        _context.DetalleFacturacion.Add(detalle);

                        // Cambiar el estado del servicio a Completado
                        servicio.Estado = 2; // Estado Completado
                        _context.Update(servicio);
                    }
                }

                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(facturacion);
        }







        private bool FacturacionExists(int id)
        {
            return _context.Facturacion.Any(e => e.Id == id);
        }
    }
}
