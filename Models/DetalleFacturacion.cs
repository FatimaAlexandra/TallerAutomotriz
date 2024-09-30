using amazon.Models;
using System.ComponentModel.DataAnnotations;

namespace TuNamespace.Models
{
    public class DetalleFacturacion
    {
        public int Id { get; set; }

        [Required]
        public int FacturacionId { get; set; }

        [Required]
        public int ServicioRealizadoId { get; set; }

        [Required]
        public decimal Subtotal { get; set; }

        public virtual Facturacion Facturacion { get; set; }  // Relación con la tabla de Facturación
        public virtual ServicioRealizado ServicioRealizado { get; set; }  // Relación con el Servicio Realizado
    }
}
