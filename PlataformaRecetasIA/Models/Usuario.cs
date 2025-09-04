using System.ComponentModel.DataAnnotations;

namespace PlataformaRecetasIA.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        public string PasswordHash { get; set; } // Almacenaremos la contraseña hasheada
    }
}