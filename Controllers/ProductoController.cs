﻿// Path: Controllers/ProductoController.cs
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using amazon.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace amazon.Controllers
{
    public class ProductoController : Controller
    {
        private readonly DbamazonContext _context;

        public ProductoController(DbamazonContext context)
        {
            _context = context;
        }

        // GET: Producto
        public IActionResult Index()
        {
            var productos = _context.Productos.ToList();
            return View(productos);
        }

        // GET: Producto/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = _context.Productos
                .FirstOrDefault(m => m.Id == id);

            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // GET: Productos/Create
        [Authorize(Roles = "1")] // Solo administradores
        public IActionResult Create()
        {
            return View();
        }

        // POST: Productos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "1")] // Solo administradores
        public async Task<IActionResult> Create([Bind("Nombre,Descripcion,Tipo,Precio")] Producto producto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(producto);
                    await _context.SaveChangesAsync();

                    // Crear entrada en inventario con cantidad inicial 0
                    var inventario = new Inventario
                    {
                        ProductoId = producto.Id,
                        Cantidad = 0,
                        FechaActualizacion = DateTime.Now
                    };
                    _context.Inventario.Add(inventario);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Producto creado correctamente y añadido al inventario.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al crear el producto: " + ex.Message);
                }
            }
            return View(producto);
        }

        // GET: Producto/Edit/5
        [Authorize(Roles = "1")] // Solo administradores
        public IActionResult Edit(int id)
        {
            var producto = _context.Productos.Find(id);
            if (producto == null)
            {
                return NotFound();
            }
            return View(producto);
        }

        // POST: Producto/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "1")] // Solo administradores
        public IActionResult Edit(Producto producto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Entry(producto).State = EntityState.Modified;
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = "Producto actualizado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.Id))
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
                    ModelState.AddModelError("", "Error al actualizar el producto: " + ex.Message);
                }
            }
            return View(producto);
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }

        // GET: Producto/Delete/5
        [Authorize(Roles = "1")] // Solo administradores
        public IActionResult Delete(int id)
        {
            var producto = _context.Productos.Find(id);
            if (producto == null)
            {
                return NotFound();
            }
            return View(producto);
        }

        // POST: Producto/DeleteConfirmed/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "1")] // Solo administradores
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(id);
                if (producto == null)
                {
                    return NotFound();
                }

                // Verificar si hay inventario asociado
                var inventario = await _context.Inventario.FirstOrDefaultAsync(i => i.ProductoId == id);
                if (inventario != null)
                {
                    // Eliminar primero el inventario
                    _context.Inventario.Remove(inventario);
                }

                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Producto eliminado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar el producto: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}