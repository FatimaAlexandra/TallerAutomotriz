using Microsoft.AspNetCore.Mvc;
using amazon.Models;
using System.Linq;

namespace amazon.Controllers
{
    public class ClienteController : Controller
    {
        private readonly DbamazonContext _context;

        // Constructor que inyecta el contexto de la base de datos
        public ClienteController(DbamazonContext context)
        {
            _context = context;
        }

        // Acción para listar los clientes (usando el modelo Usuario)
        public IActionResult Index()
        {
            var usuarios = _context.Usuarios.ToList(); // Utiliza el modelo Usuarios
            return View(usuarios); // Envía la lista de usuarios (clientes) a la vista
        }

        // GET: Cliente/Create - Acción para mostrar la vista de creación de cliente
        public IActionResult Create()
        {
            return View();
        }

        // POST: Cliente/Create - Acción para procesar la creación de un nuevo cliente
        [HttpPost]
        public IActionResult Create(Usuario usuario) // Usa el modelo Usuario
        {
            if (ModelState.IsValid)
            {
                _context.Usuarios.Add(usuario); // Añade el nuevo usuario (cliente)
                _context.SaveChanges(); // Guarda los cambios en la base de datos
                return RedirectToAction(nameof(Index)); // Redirige al índice de clientes
            }
            return View(usuario); // Si hay errores, vuelve a mostrar la vista de creación con el modelo
        }

        // GET: Cliente/Edit/{id} - Acción para mostrar la vista de edición de un cliente
        public IActionResult Edit(int id)
        {
            var usuario = _context.Usuarios.Find(id); // Busca el cliente (usuario) por su ID
            if (usuario == null)
            {
                return NotFound(); // Si no se encuentra, devuelve un error 404
            }
            return View(usuario); // Muestra la vista de edición con el usuario (cliente) encontrado
        }

        // POST: Cliente/Edit/{id} - Acción para procesar la edición de un cliente
        [HttpPost]
        public IActionResult Edit(Usuario usuario) // Usa el modelo Usuario
        {
            if (ModelState.IsValid)
            {
                _context.Usuarios.Update(usuario); // Actualiza los datos del usuario (cliente)
                _context.SaveChanges(); // Guarda los cambios en la base de datos
                return RedirectToAction(nameof(Index)); // Redirige al índice de clientes
            }
            return View(usuario); // Si hay errores, vuelve a mostrar la vista de edición con el modelo
        }

        // GET: Cliente/Delete/{id} - Acción para mostrar la vista de eliminación de un cliente
        public IActionResult Delete(int id)
        {
            var usuario = _context.Usuarios.Find(id); // Busca el cliente (usuario) por su ID
            if (usuario == null)
            {
                return NotFound(); // Si no se encuentra, devuelve un error 404
            }
            return View(usuario); // Muestra la vista de eliminación con el usuario (cliente) encontrado
        }

        // POST: Cliente/Delete/{id} - Acción para procesar la eliminación de un cliente
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var usuario = _context.Usuarios.Find(id); // Busca el cliente (usuario) por su ID
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario); // Elimina el cliente (usuario)
                _context.SaveChanges(); // Guarda los cambios en la base de datos
            }
            return RedirectToAction(nameof(Index)); // Redirige al índice de clientes
        }
    }
}
