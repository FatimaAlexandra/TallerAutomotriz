﻿using amazon.Models;
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
                .Include(f => f.Usuario)
                .ToList();

            return View(facturaciones);
        }

        // GET: Facturacion/Details/5
        public IActionResult Details(int id)
        {
            var facturacion = _context.Facturacion
                .Include(f => f.Usuario)
                .Include(f => f.DetalleFacturacion)
                .ThenInclude(df => df.ServicioRealizado)
                .FirstOrDefault(m => m.Id == id);

            if (facturacion == null)
            {
                return NotFound();
            }

            return View(facturacion);
        }

        // GET: Facturacion/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Facturacion/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Facturacion facturacion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(facturacion);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(facturacion);
        }

        // GET: Facturacion/Edit/5
        public IActionResult Edit(int id)
        {
            var facturacion = _context.Facturacion.Find(id);
            if (facturacion == null)
            {
                return NotFound();
            }
            return View(facturacion);
        }

        // POST: Facturacion/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Facturacion facturacion)
        {
            if (id != facturacion.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(facturacion);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FacturacionExists(facturacion.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(facturacion);
        }

        // GET: Facturacion/Delete/5
        public IActionResult Delete(int id)
        {
            var facturacion = _context.Facturacion
                .Include(f => f.Usuario)
                .FirstOrDefault(m => m.Id == id);
            if (facturacion == null)
            {
                return NotFound();
            }

            return View(facturacion);
        }

        // POST: Facturacion/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var facturacion = _context.Facturacion.Find(id);
            if (facturacion != null)
            {
                _context.Facturacion.Remove(facturacion);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool FacturacionExists(int id)
        {
            return _context.Facturacion.Any(e => e.Id == id);
        }

        // GET: Facturacion/DownloadPdf/5
        public IActionResult DownloadPdf(int id)
        {
            var facturacion = _context.Facturacion
                .Include(f => f.Usuario)
                .Include(f => f.DetalleFacturacion)
                .FirstOrDefault(m => m.Id == id);

            if (facturacion == null)
            {
                return NotFound();
            }

            return View("DownloadPdf", facturacion);
        }
    }
}