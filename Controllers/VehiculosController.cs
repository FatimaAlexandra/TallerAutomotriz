﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using amazon.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace amazon.Controllers
{
    public class VehiculosController : Controller
    {
        private readonly DbamazonContext _context;

        public VehiculosController(DbamazonContext context)
        {
            _context = context;
        }

        // GET: Vehiculos
        // GET: Vehiculos
        // GET: Vehiculos
        // GET: Vehiculos
        public async Task<IActionResult> Index()
        {
            // Obtener el ID del usuario autenticado y el rol desde los Claims
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value); // Obtiene el ID del usuario
            var rol = int.Parse(User.FindFirst(ClaimTypes.Role).Value); // Obtiene el rol del usuario

            List<Vehiculo> vehiculos;

            // Si el rol es 1 o 2, el usuario puede ver todos los vehículos
            if (rol == 1 || rol == 2)
            {
                vehiculos = await _context.Vehiculos
                    .Include(v => v.Usuario) // Incluye la relación con el usuario
                    .ToListAsync(); // Muestra todos los vehículos
            }
            else
            {
                // Si el rol no es 1 o 2 (por ejemplo, rol 3), muestra solo los vehículos del usuario autenticado
                vehiculos = await _context.Vehiculos
                    .Include(v => v.Usuario)
                    .Where(v => v.UsuarioId == usuarioId) // Filtrar por el ID del usuario autenticado
                    .ToListAsync();
            }

            return View(vehiculos);
        }

        //metodo para editar

        // GET: Vehiculos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null)
            {
                return NotFound();
            }

            ViewBag.Usuarios = new SelectList(_context.Usuarios, "Id", "Nombre", vehiculo.UsuarioId);

            return View(vehiculo);
        }

        // POST: Vehiculos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UsuarioId,Marca,Modelo,Año,Placa,Descripcion")] Vehiculo vehiculo)
        {
            if (id != vehiculo.Id)
            {
                return NotFound();
            }
            Console.WriteLine("Hasta este punto funciona");
            if (ModelState.IsValid)
            {
                Console.WriteLine("Entramos al if de modelo valido");

                try
                {
                    Console.WriteLine("Hasta este punto funciona 2");
                    _context.Update(vehiculo);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Vehículo actualizado correctamente.";
                    Console.WriteLine("Hasta este punto funciona 3");

                    return RedirectToAction(nameof(Index));
                }

                catch (DbUpdateConcurrencyException)
                {
                    if (!VehiculoExists(vehiculo.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            Console.WriteLine("En teoria aqui editamos ya");

            if (!ModelState.IsValid)
            {
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                }
            }

            Console.WriteLine(vehiculo.UsuarioId);

            Console.WriteLine("El modelo no es valido");

            ViewBag.Usuarios = new SelectList(_context.Usuarios, "Id", "Nombre", vehiculo.UsuarioId);
            return View(vehiculo);
        }

        private bool VehiculoExists(int id)
        {
            return _context.Vehiculos.Any(e => e.Id == id);
        }



        // GET: Vehiculos/Create
        public IActionResult Create()
        {
            ViewBag.Usuarios = new SelectList(_context.Usuarios, "Id", "Nombre");
            return View();
        }


        [HttpPost]
public async Task<IActionResult> Create([Bind("Marca,Modelo,Año,Placa,Descripcion,UsuarioId")] Vehiculo vehiculo)
{
    // Verificar si la placa ya está registrada
    var existePlaca = await _context.Vehiculos.AnyAsync(v => v.Placa == vehiculo.Placa);
    
    if (existePlaca)
    {
        // Si la placa ya existe, agregar un error al ModelState
        ModelState.AddModelError("Placa", "La placa ingresada ya está registrada.");
    }

    else
    {
        try
        {
            _context.Add(vehiculo);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error: {ex.Message}");
        }
    }

    // Repoblar el dropdown si hay un error
    ViewBag.Usuarios = new SelectList(_context.Usuarios, "Id", "Nombre", vehiculo.UsuarioId);
    return View(vehiculo);
}




        // GET: Vehiculos/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehiculo = _context.Vehiculos
                .FirstOrDefault(v => v.Id == id);

            if (vehiculo == null)
            {
                return NotFound();
            }

            return View(vehiculo);
        }

        // POST: Vehiculos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null)
            {
                return NotFound();
            }

            _context.Vehiculos.Remove(vehiculo);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
