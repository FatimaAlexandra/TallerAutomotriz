using Microsoft.AspNetCore.Mvc;
using amazon.Models;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace amazon.Controllers
{
    public class AuthController : Controller
    {
        private readonly DbamazonContext _context;

        public AuthController(DbamazonContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(Usuario usuario)
        {
            var user = _context.Usuarios.FirstOrDefault(u => u.Usuario1 == usuario.Usuario1 && u.Clave == usuario.Clave);

            if (user != null)
            {
                List<Claim> claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Usuario1),
                    new Claim(ClaimTypes.Role, user.Rol.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                AuthenticationProperties properties = new AuthenticationProperties()
                {
                    AllowRefresh = true,
                    IsPersistent = false
                };

                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity), properties);
                return RedirectToAction("Index", "Home");
            }
            ViewData["ErrorMessage"] = "Usuario o contraseña incorrectos";
            return View(usuario);
        }

        public IActionResult Register()
        {
            return View("~/Views/Registro/Index.cshtml");
        }

        [HttpPost]
        public IActionResult Register(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                _context.Usuarios.Add(usuario);
                _context.SaveChanges();
                return RedirectToAction("Login");
            }

            return View("~/Views/Registro/Index.cshtml", usuario);
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }


    }
}
