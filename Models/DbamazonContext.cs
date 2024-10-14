using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TuNamespace.Models;

namespace amazon.Models
{
    public partial class DbamazonContext : DbContext
    {
        public DbamazonContext()
        {
        }

        public DbamazonContext(DbContextOptions<DbamazonContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Usuario> Usuarios { get; set; }
        public virtual DbSet<ServicioRealizado> ServicioRealizado { get; set; }
        public virtual DbSet<Servicio> Servicios { get; set; }

        public virtual DbSet<Producto> Productos { get; set; }

        public virtual DbSet<Vehiculo> Vehiculos { get; set; }

        public virtual DbSet<Facturacion> Facturacion { get; set; }
        public virtual DbSet<DetalleFacturacion> DetallesFacturacion { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__usuarios__3213E83FCF404963");

                entity.ToTable("usuarios");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Clave)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("clave");
                entity.Property(e => e.Nombre)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("nombre");
                entity.Property(e => e.Rol).HasColumnName("rol");
                entity.Property(e => e.Usuario1)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("usuario");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
