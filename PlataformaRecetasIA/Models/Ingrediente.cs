// Models/Ingrediente.cs
using System.ComponentModel.DataAnnotations;

namespace PlataformaRecetasIA.Models
{
    public class Ingrediente
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Nombre { get; set; }
        public double Cantidad { get; set; }
        public string Unidad { get; set; } // ej. "gramos"
        public int RecetaId { get; set; } // FK
        public Receta Receta { get; set; } // Navegación
    }
}