using System.Linq;
using Microsoft.AspNetCore.Mvc;
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

        // GET: Servicio/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Servicio/Create
        [HttpPost]
        public IActionResult Create(Servicio servicio)
        {
            if (ModelState.IsValid)
            {
                _context.Servicios.Add(servicio);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(servicio);
        }

        // GET: Servicio/Edit/5
        public IActionResult Edit(int id)
        {
            var servicio = _context.Servicios.Find(id);
            if (servicio == null)
            {
                return NotFound();
            }
            return View(servicio);
        }

        // POST: Servicio/Edit/5
        [HttpPost]
        public IActionResult Edit(Servicio servicio)
        {
            if (ModelState.IsValid)
            {
                _context.Servicios.Update(servicio);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(servicio);
        }

        // GET: Servicio/Delete/5
        public IActionResult Delete(int id)
        {
            var servicio = _context.Servicios.Find(id);
            if (servicio == null)
            {
                return NotFound();
            }
            return View(servicio);
        }

        // POST: Servicio/DeleteConfirmed/5
        [HttpPost, ActionName("DeleteConfirmed")]
        public IActionResult DeleteConfirmed(int id)
        {
            var servicio = _context.Servicios.FirstOrDefault(m => m.Id == id);
            if (servicio == null)
            {
                return NotFound();
            }

            _context.Servicios.Remove(servicio);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}

