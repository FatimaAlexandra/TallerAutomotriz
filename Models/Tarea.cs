using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace amazon.Models
{
    public class Tarea
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(100, ErrorMessage = "El título no puede exceder los 100 caracteres")]
        [Display(Name = "Título")]
        public string Titulo { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El estado es requerido")]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Pendiente";

        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha de Vencimiento")]
        public DateTime? FechaVencimiento { get; set; }

        [Required(ErrorMessage = "El usuario creador es requerido")]
        [Display(Name = "Creado por")]
        public int UsuarioCreadorId { get; set; }

        [Display(Name = "Mecánico Asignado")]
        public int? MecanicoAsignadoId { get; set; }

        [Display(Name = "Vehículo")]
        public int? VehiculoId { get; set; }

        // Propiedades de navegación
        [ForeignKey("UsuarioCreadorId")]
        [Display(Name = "Creado por")]
        public virtual Usuario UsuarioCreador { get; set; }

        [ForeignKey("MecanicoAsignadoId")]
        [Display(Name = "Mecánico Asignado")]
        public virtual Usuario MecanicoAsignado { get; set; }

        [ForeignKey("VehiculoId")]
        [Display(Name = "Vehículo")]
        public virtual Vehiculo Vehiculo { get; set; }
    }
}