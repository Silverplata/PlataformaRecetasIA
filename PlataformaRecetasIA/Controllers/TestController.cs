// Controllers/TestController.cs
using Microsoft.AspNetCore.Mvc;
using PlataformaRecetasIA.Data;

namespace PlataformaRecetasIA.Controllers
{
    public class TestController : Controller
    {
        private readonly AppDbContext _context;

        public TestController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var recetas = _context.Recetas.ToList();
            return Ok(recetas);
        }
    }
}