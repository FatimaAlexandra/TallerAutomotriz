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

        public int Rol { get; set; } = 3; // Rol por defecto igual a 3 para registrarse como cliente

        public string Clave { get; set; } = null!;
    }

}
