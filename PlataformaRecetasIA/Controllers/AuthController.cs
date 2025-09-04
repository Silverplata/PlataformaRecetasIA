// Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using PlataformaRecetasIA.Models;

namespace PlataformaRecetasIA.Controllers
{
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        public IActionResult Login(UserLogin user)
        {
            // Validación simple (sin JWT)
            if (user.Username == "admin" && user.Password == "password")
            {
                // Simular login exitoso redirigiendo a /Recetas
                return RedirectToAction("Index", "Recetas");
            }
            ViewBag.Error = "Credenciales inválidas";
            return View();
        }
    }
}