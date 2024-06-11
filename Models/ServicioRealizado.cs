using System;

namespace amazon.Models
{
    public class ServicioRealizado
    {
        public int Id { get; set; }
        public int ServicioId { get; set; } // Cambiado de IdServicio a ServicioId
        public int UsuarioId { get; set; }
        public decimal Precio { get; set; }
        public DateTime Fecha { get; set; }

        // Propiedades de navegación
        public Servicio Servicio { get; set; }
        public Usuario Usuario { get; set; }
    }
}
