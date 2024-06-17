using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using amazon.Models;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

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
                .Include(sr => sr.Servicio)
                .Include(sr => sr.Usuario)
                .ToList();

            return View(serviciosRealizados);
        }

        // GET: ServicioRealizado/Create
        public IActionResult Create()
        {
            ViewBag.ServicioId = new SelectList(_context.Servicios, "Id", "Nombre");
            return View();
        }

        


        private bool ServicioRealizadoExists(int id)
        {
            return _context.ServicioRealizado.Any(sr => sr.Id == id);
        }
    }
}
