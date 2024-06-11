using System;
using System.Collections.Generic;

namespace amazon.Models
{
    public partial class Servicio
    {
        public Servicio()
        {
            ServiciosRealizados = new HashSet<ServicioRealizado>();
        }

        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Categoria { get; set; }

        public virtual ICollection<ServicioRealizado> ServiciosRealizados { get; set; }
    }
}
