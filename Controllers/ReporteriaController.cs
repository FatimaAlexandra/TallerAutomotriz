using Microsoft.AspNetCore.Mvc;
using System.Linq;
using amazon.Models;

namespace amazon.Controllers
{
    public class ReporteriaController : Controller
    {
        private readonly DbamazonContext _context;

        public ReporteriaController(DbamazonContext context)
        {
            _context = context;
        }

        // Vista principal de reportería
        public IActionResult Index()
        {
            return View();
        }

        // API para obtener ingresos totales por servicio
        [HttpGet]
        public IActionResult IngresosPorServicio()
        {
            var ingresos = _context.ServicioRealizado
                .Where(s => s.Servicio != null) // Evitar posibles nulos
                .GroupBy(s => s.Servicio.Nombre)
                .Select(g => new
                {
                    Servicio = g.Key,
                    TotalIngresos = g.Sum(s => s.Precio)
                }).ToList();

            return Json(ingresos);
        }



        // API para obtener la cantidad de vehículos por marca y modelo
        [HttpGet]
        public IActionResult VehiculosPorMarcaModelo()
        {
            var vehiculos = _context.Vehiculos
                .GroupBy(v => new { v.Marca, v.Modelo })
                .Select(g => new
                {
                    Marca = g.Key.Marca,
                    Modelo = g.Key.Modelo,
                    Cantidad = g.Count()
                }).ToList();

            return Json(vehiculos);
        }


        // API para resumen de servicios realizados
        [HttpGet]
        public IActionResult ServiciosRealizadosResumen()
        {
            var totalServicios = _context.ServicioRealizado.Count();
            return Json(new { TotalServicios = totalServicios });
        }

        // API para obtener los ingresos totales de facturación
        [HttpGet]
        public IActionResult TotalFacturacion()
        {
            var totalFacturacion = _context.Facturacion.Sum(f => (decimal?)f.MontoTotal) ?? 0;
            return Json(new { TotalFacturacion = totalFacturacion });
        }
        [HttpGet]
        public IActionResult TotalServiciosRealizados()
        {
            var totalServicios = _context.ServicioRealizado.Count();
            return Json(new { TotalServicios = totalServicios });
        }

        [HttpGet]
        public IActionResult TotalVehiculosAtendidos()
        {
            var totalVehiculos = _context.ServicioRealizado
                .Where(sr => sr.VehiculoId != null)
                .Select(sr => sr.VehiculoId)
                .Distinct()
                .Count();
            return Json(new { TotalVehiculos = totalVehiculos });
        }
        [HttpGet]
        [HttpGet]
        public IActionResult ObtenerServicios()
        {
            try
            {
                var servicios = _context.Servicios
                    .Select(s => new
                    {
                        Id = s.Id,
                        Nombre = s.Nombre
                    })
                    .ToList();

                return Json(servicios);
            }
            catch (Exception ex)
            {
                // Registra el error en el log si es necesario
                Console.WriteLine($"Error en ObtenerServicios: {ex.Message}");
                return BadRequest("Error al obtener los servicios.");
            }
        }


        [HttpGet]
        public IActionResult ObtenerMarcas()
        {
            var marcas = _context.Vehiculos
                .Select(v => v.Marca)
                .Distinct()
                .ToList();
            return Json(marcas);
        }

        [HttpGet]
        public IActionResult ModelosPorMarca(string marca)
        {
            if (string.IsNullOrEmpty(marca))
            {
                return BadRequest("La marca no puede ser nula o vacía.");
            }

            var modelos = _context.Vehiculos
                .Where(v => v.Marca == marca)
                .GroupBy(v => v.Modelo)
                .Select(g => new
                {
                    Modelo = g.Key,
                    Cantidad = g.Count()
                })
                .ToList();

            return Json(modelos);
        }







    }
}
