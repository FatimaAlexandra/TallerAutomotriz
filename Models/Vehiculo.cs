using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace amazon.Models
{
    public class Vehiculo
    {
        public int Id { get; set; }
      
        public string Marca { get; set; }
   
        public string Modelo { get; set; }

        public int Año { get; set; }
        public string Placa { get; set; }
        public string Descripcion { get; set; } // Descripción opcional
        public int UsuarioId { get; set; }

        // Relación con otras tablas
        public Usuario Usuario { get; set; }

    }
}
