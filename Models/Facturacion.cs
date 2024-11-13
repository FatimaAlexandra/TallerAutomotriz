using amazon.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TuNamespace.Models
{
    public class Facturacion
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El usuario es requerido")]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "La fecha de facturación es requerida")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Facturación")]
        public DateTime FechaFacturacion { get; set; }

        [Required(ErrorMessage = "El monto total es requerido")]
        [DataType(DataType.Currency)]
        [Display(Name = "Monto Total")]
        public decimal MontoTotal { get; set; }

        [Required(ErrorMessage = "El método de pago es requerido")]
        [Display(Name = "Método de Pago")]
        public string MetodoPago { get; set; }

        [Display(Name = "Estado de la Factura")]
        public string EstadoFactura { get; set; } = "Pendiente";

        public virtual Usuario Usuario { get; set; }
        public virtual ICollection<DetalleFacturacion> DetalleFacturacion { get; set; }
    }
}