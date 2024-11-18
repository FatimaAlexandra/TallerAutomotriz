using amazon.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace amazon.Models
{


    

        public class Comentario
        {
            [Key]
            public int Id { get; set; }

            [Required(ErrorMessage = "El texto del comentario es obligatorio.")]
            [StringLength(500, ErrorMessage = "El texto no puede exceder los 500 caracteres.")]
            public string Texto { get; set; }

            [Required(ErrorMessage = "La calificación es obligatoria.")]
            [Range(0, 5, ErrorMessage = "La calificación debe estar entre 0 y 5.")]
            public int Calificacion { get; set; }

            [Required]
            public int UsuarioId { get; set; } // Clave foránea explícita para Usuario.

            [ForeignKey("UsuarioId")]
            public Usuario Usuario { get; set; } // Relación con Usuario.

            [Required]
            public int ServicioId { get; set; } // Clave foránea explícita para Servicio.

            [ForeignKey("ServicioId")]
            public Servicio Servicio { get; set; } // Relación con Servicio.
        


    }
}
    




