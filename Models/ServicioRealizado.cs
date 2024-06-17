using System;

namespace amazon.Models
{
    public class ServicioRealizado
    {
        public int Id { get; set; }
        public int ServicioId { get; set; } 
        public int UsuarioId { get; set; }
        public decimal Precio { get; set; }
        public String Fecha { get; set; }
        public int Estado { get; set; } = 1; //Valor por defecto

        // Propiedades de navegación
        public Servicio Servicio { get; set; }
        public Usuario Usuario { get; set; }
    }
}
