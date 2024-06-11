using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using amazon.Models;

namespace amazon.Controllers
{
    public class ServicioController : Controller
    {
        private readonly DbamazonContext _context;

        public ServicioController(DbamazonContext context)
        {
            _context = context;
        }


        // GET: Servicio
        public IActionResult Index()
        {
            var servicios = _context.Servicios.ToList();
            return View(servicios);
        }

        // GET: Servicio/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var servicio = _context.Servicios
                .FirstOrDefault(m => m.Id == id);

            if (servicio == null)
            {
                return NotFound();
            }

            return View(servicio);
        }

        // Other CRUD actions like Create, Edit, Delete can be added here
    }
}
