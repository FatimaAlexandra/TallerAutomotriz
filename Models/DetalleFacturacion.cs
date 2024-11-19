using amazon.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TuNamespace.Models
{
    public class DetalleFacturacion
    {
        public int Id { get; set; }

        [Required]
        public int FacturaId { get; set; }

        [Required]
        public int ServicioRealizadoId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        public virtual Facturacion Facturacion { get; set; }
        public virtual ServicioRealizado ServicioRealizado { get; set; }
    }
}

