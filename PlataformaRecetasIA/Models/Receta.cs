// Models/Receta.cs
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace PlataformaRecetasIA.Models
{
    [XmlRoot("Receta")]
    public class Receta
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public List<Ingrediente> Ingredientes { get; set; } = new List<Ingrediente>();
        public int Porciones { get; set; }
    }
}