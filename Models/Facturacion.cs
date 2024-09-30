using amazon.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TuNamespace.Models
{
    public class Facturacion
    {
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime FechaFacturacion { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal MontoTotal { get; set; }

        [Required]
        public string MetodoPago { get; set; }

        public string EstadoFactura { get; set; } = "Pendiente";

        public virtual Usuario Usuario { get; set; }  // Relación con la tabla de Usuarios
        public virtual ICollection<DetalleFacturacion> DetalleFacturacion { get; set; }  // Detalles de la factura
    }
}
