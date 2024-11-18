using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using amazon.Models;

namespace amazon.Controllers
{
    public class ComentariosController : Controller
    {
        private readonly DbamazonContext _context;

        public ComentariosController(DbamazonContext context)
        {
            _context = context;
        }

        // GET: Comentarios
        public async Task<IActionResult> Index()
        {
            var dbamazonContext = _context.Comentarios.Include(c => c.Servicio).Include(c => c.Usuario);
            return View(await dbamazonContext.ToListAsync());
        }

        // GET: Comentarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Comentarios == null)
            {
                return NotFound();
            }

            var comentario = await _context.Comentarios
                .Include(c => c.Servicio)
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comentario == null)
            {
                return NotFound();
            }

            return View(comentario);
        }

        // GET: Comentarios/Create
        public IActionResult Create()

        {

            ModelState.Remove("Servicio");
            ModelState.Remove("Usuario");
            ViewData["ServicioId"] = new SelectList(_context.Servicios, "Id", "Nombre");
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "Nombre");
            return View();
        }

        // POST: Comentarios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Texto,Calificacion,UsuarioId,ServicioId")] Comentario comentario)
        {

            comentario.Servicio = _context.Servicios.First(s => s.Id == comentario.ServicioId);
            comentario.Usuario = _context.Usuarios.First(u => u.Id == comentario.ServicioId);
            ModelState.Remove("Servicio");
            ModelState.Remove("Usuario");

            if (ModelState.IsValid)
            {
                _context.Add(comentario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ServicioId"] = new SelectList(_context.Servicios, "Id", "Nombre", comentario.ServicioId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "Nombre", comentario.UsuarioId);
            return View(comentario);
        }

        // GET: Comentarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Comentarios == null)
            {
                return NotFound();
            }

            var comentario = await _context.Comentarios.FindAsync(id);
            if (comentario == null)
            {
                return NotFound();
            }
            ViewData["ServicioId"] = new SelectList(_context.Servicios, "Id", "Categoria", comentario.ServicioId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "Id", comentario.UsuarioId);
            return View(comentario);
        }

        // POST: Comentarios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Texto,Calificacion,UsuarioId,ServicioId")] Comentario comentario)
        {
            if (id != comentario.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(comentario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ComentarioExists(comentario.Id))
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
            ViewData["ServicioId"] = new SelectList(_context.Servicios, "Id", "Categoria", comentario.ServicioId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "Id", comentario.UsuarioId);
            return View(comentario);
        }

        // GET: Comentarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Comentarios == null)
            {
                return NotFound();
            }

            var comentario = await _context.Comentarios
                .Include(c => c.Servicio)
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comentario == null)
            {
                return NotFound();
            }

            return View(comentario);
        }

        // POST: Comentarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Comentarios == null)
            {
                return Problem("Entity set 'DbamazonContext.Comentarios'  is null.");
            }
            var comentario = await _context.Comentarios.FindAsync(id);
            if (comentario != null)
            {
                _context.Comentarios.Remove(comentario);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ComentarioExists(int id)
        {
          return (_context.Comentarios?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
