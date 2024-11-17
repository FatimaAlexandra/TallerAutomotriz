using System;
using System.Collections.Generic;

namespace amazon.Models
{

    public enum Rol
    {
        Administrador = 1,
        Mecanico = 2,
        Cliente = 3
    }
    public partial class Usuario
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = null!;

        public string Usuario1 { get; set; } = null!;

        public int Rol { get; set; } = 3; 

        public string Clave { get; set; } = null!;

        public string Telefono { get; set; } = null!;

        public string Correo { get; set; } = null!;

        public string Dui { get; set; } = null!;   
    }

}
