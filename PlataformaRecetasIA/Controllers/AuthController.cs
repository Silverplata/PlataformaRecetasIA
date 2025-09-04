using Microsoft.AspNetCore.Mvc;
using PlataformaRecetasIA.Models;
using PlataformaRecetasIA.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;

namespace PlataformaRecetasIA.Controllers
{
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public AuthController(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
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
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Username == user.Username);
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(user.Password, usuario.PasswordHash))
            {
                ViewBag.Error = "Credenciales inválidas";
                return View();
            }

            var token = GenerateJwtToken(usuario);
            Response.Cookies.Append("JwtToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(1)
            });

            return RedirectToAction("Index", "Recetas");
        }

        // GET: /Auth/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Auth/Register
        [HttpPost]
        public IActionResult Register(UserLogin user)
        {
            if (_context.Usuarios.Any(u => u.Username == user.Username))
            {
                ViewBag.Error = "El nombre de usuario ya existe.";
                return View();
            }

            var usuario = new Usuario
            {
                Username = user.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password)
            };

            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        // GET: /Auth/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("JwtToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            });
            return RedirectToAction("Login");
        }

        // GET: /Auth/CheckAuth
        [HttpGet]
        public IActionResult CheckAuth()
        {
            return Json(new { authenticated = User.Identity.IsAuthenticated });
        }

        private string GenerateJwtToken(Usuario usuario)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, usuario.Username),
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}