using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using amazon.Models;

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

        // GET: ServicioRealizado/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var servicioRealizado = _context.ServicioRealizado
                .Include(s => s.Servicio)
                .Include(s => s.Usuario)
                .FirstOrDefault(m => m.Id == id);

            if (servicioRealizado == null)
            {
                return NotFound();
            }

            return View(servicioRealizado);
        }

        // Other CRUD actions like Create, Edit, Delete can be added here

        private bool ServicioRealizadoExists(int id)
        {
            return _context.ServicioRealizado.Any(e => e.Id == id);
        }
    }
}
