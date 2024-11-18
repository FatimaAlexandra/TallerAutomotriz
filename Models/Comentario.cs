using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace amazon.Models
{
    public class Comentario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El usuario es requerido")]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "El servicio es requerido")]
        public int ServicioRealizadoId { get; set; }

        [Required(ErrorMessage = "El contenido es requerido")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "El comentario debe tener entre 10 y 1000 caracteres")]
        public string Contenido { get; set; }

        [Required(ErrorMessage = "La calificación es requerida")]
        [Range(1, 5, ErrorMessage = "La calificación debe estar entre 1 y 5")]
        public int Calificacion { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime? FechaModificacion { get; set; }

        public bool Estado { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuario { get; set; }

        [ForeignKey("ServicioRealizadoId")]
        public virtual ServicioRealizado ServicioRealizado { get; set; }
    }
}