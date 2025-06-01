    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    namespace amazon.Models
    {
        public partial class Servicio
        {
            public Servicio()
            {
                ServiciosRealizados = new HashSet<ServicioRealizado>();
            }

            public int Id { get; set; }
            [Required(ErrorMessage = "El nombre es obligatorio.")]
            public string Nombre { get; set; }

            [Required(ErrorMessage = "La descripción es obligatoria.")]
            public string Descripcion { get; set; }

            [Required(ErrorMessage = "La categoría es obligatoria.")]
            public string Categoria { get; set; }
            public virtual ICollection<ServicioRealizado> ServiciosRealizados { get; set; }
        }
    }
