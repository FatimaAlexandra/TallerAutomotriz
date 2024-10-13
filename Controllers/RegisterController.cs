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
