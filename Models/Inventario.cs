using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace amazon.Models
{
    public class Inventario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El producto es requerido")]
        public int ProductoId { get; set; }

        [Required(ErrorMessage = "La cantidad es requerida")]
        [Range(0, int.MaxValue, ErrorMessage = "La cantidad debe ser un valor positivo")]
        public int Cantidad { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha de Actualización")]
        public DateTime FechaActualizacion { get; set; } = DateTime.Now;

        // Propiedad de navegación
        [ForeignKey("ProductoId")]
        public virtual Producto Producto { get; set; }
    }
}