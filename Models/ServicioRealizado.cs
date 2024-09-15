using System;

namespace amazon.Models
{
    public class ServicioRealizado
    {
        public int id { get; set; }
        public int ServicioId { get; set; }
        public int UsuarioId { get; set; }
        public decimal Precio { get; set; }
        public String Fecha { get; set; }
        public int Estado { get; set; } = 1; //Valor por defecto
        public int? VehiculoId { get; set; }

        // Navigation properties
        public Servicio Servicio { get; set; }
        public Usuario Usuario { get; set; }
        public Vehiculo Vehiculo { get; set; }
    }
}
