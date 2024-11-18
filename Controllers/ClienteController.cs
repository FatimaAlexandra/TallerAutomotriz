using Microsoft.AspNetCore.Mvc;
using amazon.Models;
using System.Linq;

namespace amazon.Controllers
{
    public class ClienteController : Controller
    {
        private readonly DbamazonContext _context;

        public ClienteController(DbamazonContext context)
        {
            _context = context;
        }

        // Acción para listar los clientes con rol = 3
        public IActionResult Index()
        {
            var clientes = _context.Usuarios.Where(u => u.Rol == 3).ToList();
            return View(clientes);
        }

        // GET: Cliente/Details/{id} - Devuelve los detalles del cliente en formato JSON
        public IActionResult Details(int id)
        {
            var cliente = _context.Usuarios.Where(u => u.Rol == 3 && u.Id == id).FirstOrDefault();
            if (cliente == null)
            {
                return NotFound();
            }

            // Retorna los datos del cliente como JSON
            return Json(new
            {
                nombre = cliente.Nombre,
                telefono = cliente.Telefono,
                correo = cliente.Correo,
                dui = cliente.Dui
            });
        }
    }

}