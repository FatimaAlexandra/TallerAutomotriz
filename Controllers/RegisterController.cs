using Microsoft.AspNetCore.Mvc;
using amazon.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace amazon.Controllers
{
    public class RegisterController : Controller
    {
        private readonly DbamazonContext _context;

        public RegisterController(DbamazonContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("id,Nombre,Usuario1,Rol,Clave,Telefono,Correo,Dui")] Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                _context.Add(usuario);
                await _context.SaveChangesAsync();
                // Autenticar al usuario después del registro
                await AuthenticateUser(usuario.Usuario1);
                return RedirectToAction("Index", "Home");
            }
            return View("~/Views/Registro/Index.cshtml", usuario);
        }
        [HttpGet]
        public IActionResult ObtenerServicios()
        {
            var servicios = _context.Servicios
                .Select(s => new { s.Id, s.Nombre })
                .ToList();
            return Json(servicios);
        }

        [HttpGet]
        public IActionResult ObtenerMarcas()
        {
            var marcas = _context.Vehiculos
                .Select(v => v.Marca)
                .Distinct()
                .ToList();
            return Json(marcas);
        }



        [HttpGet]
        public IActionResult ModelosPorMarca(string marca)
        {
            try
            {
                var modelos = _context.Vehiculos
                    .Where(v => v.Marca == marca)
                    .GroupBy(v => v.Modelo)
                    .Select(g => new
                    {
                        Modelo = g.Key,
                        Cantidad = g.Count()
                    })
                    .ToList();

                return Json(modelos);
            }
            catch (Exception ex)
            {
                // Log error si es necesario
                Console.WriteLine($"Error en ModelosPorMarca: {ex.Message}");
                return BadRequest("Error al obtener los modelos por marca.");
            }
        }




        private async Task AuthenticateUser(string userName)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userName)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                IsPersistent = true,
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );
        }
    }
}
