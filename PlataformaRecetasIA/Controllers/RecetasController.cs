using Microsoft.AspNetCore.Mvc;
using PlataformaRecetasIA.Data;
using PlataformaRecetasIA.Models;
using System.Xml.Serialization;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace PlataformaRecetasIA.Controllers
{
    public class RecetasController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<RecetasController> _logger;

        public RecetasController(AppDbContext context, IConfiguration configuration, ILogger<RecetasController> logger)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = new HttpClient();
            _logger = logger;
        }

        // GET: /Recetas/Index
        [HttpGet]
        [Authorize]
        public IActionResult Index()
        {
            var recetas = _context.Recetas.ToList();
            return View(recetas);
        }

        // GET: /Recetas/Details/5
        [Authorize]
        public IActionResult Details(int id)
        {
            var receta = _context.Recetas
                .Include(r => r.Ingredientes)
                .FirstOrDefault(r => r.Id == id);
            if (receta == null)
                return NotFound();
            return View(receta);
        }

        // GET: /Recetas/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Recetas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Create(Receta receta)
        {
            if (ModelState.IsValid)
            {
                _context.Recetas.Add(receta);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(receta);
        }

        // GET: /Recetas/Edit/5
        [Authorize]
        public IActionResult Edit(int id)
        {
            var receta = _context.Recetas.Find(id);
            if (receta == null)
                return NotFound();
            return View(receta);
        }

        // POST: /Recetas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Edit(int id, Receta receta)
        {
            if (id != receta.Id)
                return BadRequest("El ID de la receta no coincide.");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(receta);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Error al actualizar la receta en la base de datos.");
                }
            }
            return View(receta);
        }

        // GET: /Recetas/Delete/5
        [Authorize]
        public IActionResult Delete(int id)
        {
            var receta = _context.Recetas.Find(id);
            if (receta == null)
                return NotFound();
            return View(receta);
        }

        // POST: /Recetas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult DeleteConfirmed(int id)
        {
            var receta = _context.Recetas.Find(id);
            if (receta == null)
                return NotFound();
            try
            {
                _context.Recetas.Remove(receta);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Error al eliminar la receta de la base de datos.");
                return View(receta);
            }
        }

        // GET: /Recetas/Search
        [Authorize]
        public IActionResult Search()
        {
            return View();
        }

        // POST: /Recetas/Search
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Search(string searchTerm)
        {
            var recetas = _context.Recetas
                .Where(r => r.Nombre.Contains(searchTerm) || r.Descripcion.Contains(searchTerm))
                .ToList();
            return View("Index", recetas);
        }

        // GET: /Recetas/Portions
        [Authorize]
        public IActionResult Portions()
        {
            var recetas = _context.Recetas.ToList();
            return View(recetas);
        }

        // POST: /Recetas/Portions
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Portions(int recetaId, int nuevasPorciones)
        {
            var receta = _context.Recetas
                .Include(r => r.Ingredientes)
                .FirstOrDefault(r => r.Id == recetaId);
            if (receta == null)
                return NotFound();

            foreach (var ingrediente in receta.Ingredientes)
            {
                ingrediente.Cantidad *= (double)nuevasPorciones / receta.Porciones;
            }
            receta.Porciones = nuevasPorciones;
            _context.SaveChanges();
            return RedirectToAction("Details", new { id = recetaId });
        }

        // POST: /Recetas/ImportarXml
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult ImportarXml(IFormFile xmlFile)
        {
            if (xmlFile == null || xmlFile.Length == 0)
                return BadRequest("No se proporcionó un archivo XML");

            try
            {
                using var stream = xmlFile.OpenReadStream();
                var serializer = new XmlSerializer(typeof(List<Receta>), new XmlRootAttribute("Recetas"));
                var recetas = (List<Receta>)serializer.Deserialize(stream);
                _context.Recetas.AddRange(recetas);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return BadRequest("Error al procesar el archivo XML");
            }
        }

        // GET: /Recetas/Generar
        [Authorize]
        public IActionResult Generar()
        {
            return View();
        }

        // POST: /Recetas/Generar
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Generar([FromForm] string ingredientes)
        {
            _logger.LogInformation("Iniciando Generar con ingredientes: {Ingredientes}, User: {User}", ingredientes, User.Identity.Name);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Validación CSRF fallida.");
                return BadRequest(new { error = "Token CSRF inválido o ausente." });
            }

            if (string.IsNullOrWhiteSpace(ingredientes))
            {
                _logger.LogWarning("Ingredientes vacíos o nulos.");
                return BadRequest(new { error = "Debes proporcionar al menos un ingrediente." });
            }

            try
            {
                var ingredientesList = ingredientes.Split(',').Select(i => i.Trim()).ToList();
                var apiKey = _configuration["OpenAI:ApiKey"];
                _logger.LogInformation("Clave de API leída: {ApiKey}", apiKey != null ? "presente" : "nula");

                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogError("Clave de API de OpenAI no configurada.");
                    return BadRequest(new { error = "Clave de API de OpenAI no configurada." });
                }

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

                var requestBody = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                        new { role = "system", content = "Eres un chef experto que genera recetas detalladas." },
                        new { role = "user", content = $"Genera una receta usando estos ingredientes: {string.Join(", ", ingredientesList)}. Incluye nombre, descripción, pasos e ingredientes con cantidades. Formatea la respuesta en texto plano." }
                    },
                    max_tokens = 500,
                    temperature = 0.7
                };

                _logger.LogInformation("Enviando solicitud a OpenAI con cuerpo: {RequestBody}", JsonSerializer.Serialize(requestBody));
                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

                _logger.LogInformation("Respuesta de OpenAI: StatusCode={StatusCode}", response.StatusCode);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error en la API de OpenAI: {StatusCode} - {ErrorContent}", response.StatusCode, errorContent);
                    return BadRequest(new { error = $"Error en la API de OpenAI: {response.StatusCode} - {errorContent}" });
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Respuesta JSON de OpenAI: {ResponseJson}", responseJson);
                var result = JsonSerializer.Deserialize<OpenAIResponse>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result?.choices == null || result.choices.Count == 0 || string.IsNullOrEmpty(result.choices[0]?.message?.content))
                {
                    _logger.LogError("La respuesta de OpenAI está vacía o no es válida.");
                    return BadRequest(new { error = "La respuesta de OpenAI está vacía o no es válida." });
                }

                var recetaGenerada = result.choices[0].message.content;
                _logger.LogInformation("Receta generada: {Receta}", recetaGenerada);
                return Json(new { receta = recetaGenerada });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al procesar la respuesta de OpenAI.");
                return BadRequest(new { error = $"Error al procesar la respuesta de OpenAI: {ex.Message}" });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error en la solicitud a OpenAI.");
                return BadRequest(new { error = $"Error en la solicitud a OpenAI: {ex.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en Generar.");
                return BadRequest(new { error = $"Error inesperado: {ex.Message}" });
            }
        }
    }

    public class OpenAIResponse
    {
        public List<Choice> choices { get; set; }
    }

    public class Choice
    {
        public Message message { get; set; }
    }

    public class Message
    {
        public string content { get; set; }
    }
}