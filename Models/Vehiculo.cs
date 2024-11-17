namespace amazon.Models
{
    public class Vehiculo
    {
        public int Id { get; set; }
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public int Año { get; set; }
        public string Placa { get; set; }
        public string Descripcion { get; set; } 
        public int UsuarioId { get; set; }

        public Usuario? Usuario { get; set; }

    }
}
