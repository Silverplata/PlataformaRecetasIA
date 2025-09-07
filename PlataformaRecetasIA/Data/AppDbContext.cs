using Microsoft.EntityFrameworkCore;
using PlataformaRecetasIA.Models;

namespace PlataformaRecetasIA.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Receta> Recetas { get; set; }
        public DbSet<Ingrediente> Ingredientes { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Receta>()
                .HasMany(r => r.Ingredientes)
                .WithOne(i => i.Receta)
                .HasForeignKey(i => i.RecetaId);

            // Configuración para Usuarios
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Configurar valor por defecto para CanUseAI
            modelBuilder.Entity<Usuario>()
                .Property(u => u.CanUseAI)
                .HasDefaultValue(0);
        }
    }
}