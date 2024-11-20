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
        public virtual DbSet<DetalleFacturacion> DetalleFacturacion { get; set; }
        public virtual DbSet<Comentario> Comentarios { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("usuarios");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Nombre)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("nombre");
                entity.Property(e => e.Usuario1)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("usuario");
                entity.Property(e => e.Rol)
                    .HasColumnName("rol");
                entity.Property(e => e.Clave)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("clave");
                entity.Property(e => e.Telefono)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);
                entity.Property(e => e.Correo)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);
                entity.Property(e => e.Dui)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Servicio>(entity =>
            {
                entity.ToTable("Servicios");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Nombre)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);
                entity.Property(e => e.Descripcion)
                    .HasMaxLength(255)
                    .IsUnicode(false);
                entity.Property(e => e.Categoria)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ServicioRealizado>(entity =>
            {
                entity.ToTable("ServicioRealizado");

                entity.Property(e => e.id).HasColumnName("id");
                entity.Property(e => e.Precio)
                    .HasColumnType("decimal(18, 2)");
                entity.Property(e => e.Fecha)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.Estado)
                    .HasDefaultValue(1);

                entity.HasOne(d => d.Servicio)
                    .WithMany(p => p.ServiciosRealizados)
                    .HasForeignKey(d => d.ServicioId);

                entity.HasOne(d => d.Usuario)
                    .WithMany()
                    .HasForeignKey(d => d.UsuarioId);

                entity.HasOne(d => d.Vehiculo)
                    .WithMany()
                    .HasForeignKey(d => d.VehiculoId);
            });

            modelBuilder.Entity<Producto>(entity =>
            {
                entity.ToTable("productos");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Nombre)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);
                entity.Property(e => e.Descripcion)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);
                entity.Property(e => e.Tipo)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);
                entity.Property(e => e.Precio)
                    .HasColumnType("decimal(18, 2)");
            });

            modelBuilder.Entity<Vehiculo>(entity =>
            {
                entity.ToTable("Vehiculos");

                entity.Property(e => e.Marca)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.Modelo)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.Placa)
                    .IsRequired()
                    .HasMaxLength(20);
                entity.Property(e => e.Descripcion)
                    .HasMaxLength(255);

                entity.HasOne(d => d.Usuario)
                    .WithMany()
                    .HasForeignKey(d => d.UsuarioId);
            });

            modelBuilder.Entity<Facturacion>(entity =>
            {
                entity.ToTable("Facturacion");

                entity.Property(e => e.FechaFacturacion)
                    .HasColumnType("date");
                entity.Property(e => e.MontoTotal)
                    .HasColumnType("decimal(18, 2)");
                entity.Property(e => e.MetodoPago)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.EstadoFactura)
                    .HasMaxLength(20)
                    .HasDefaultValue("Pendiente");

                entity.HasOne(d => d.Usuario)
                    .WithMany()
                    .HasForeignKey(d => d.UsuarioId);
            });

            modelBuilder.Entity<DetalleFacturacion>(entity =>
            {
                entity.ToTable("DetalleFacturacion");

                entity.Property(e => e.PrecioUnitario)
                    .HasColumnType("decimal(18, 2)");
                entity.Property(e => e.Subtotal)
                    .HasColumnType("decimal(18, 2)");

                entity.HasOne(d => d.Facturacion)
                    .WithMany(p => p.DetalleFacturacion)
                    .HasForeignKey(d => d.FacturaId);

                entity.HasOne(d => d.ServicioRealizado)
                    .WithMany()
                    .HasForeignKey(d => d.ServicioRealizadoId);
            });

            modelBuilder.Entity<Comentario>(entity =>
            {
                entity.ToTable("Comentarios");

                entity.Property(e => e.Contenido)
                    .IsRequired()
                    .HasMaxLength(1000)
                    .IsUnicode(true);

                entity.Property(e => e.Calificacion)
                    .IsRequired();

                entity.Property(e => e.FechaCreacion)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.FechaModificacion)
                    .HasColumnType("datetime");

                entity.Property(e => e.Estado)
                    .HasDefaultValue(true);

                // Configuración de las relaciones
                entity.HasOne(d => d.Usuario)
                    .WithMany()
                    .HasForeignKey(d => d.UsuarioId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Comentarios_Usuarios");

                entity.HasOne(d => d.ServicioRealizado)
                    .WithMany()
                    .HasForeignKey(d => d.ServicioRealizadoId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Comentarios_ServicioRealizado");

                // Índices
                entity.HasIndex(e => e.UsuarioId)
                    .HasDatabaseName("IX_Comentarios_UsuarioId");

                entity.HasIndex(e => e.ServicioRealizadoId)
                    .HasDatabaseName("IX_Comentarios_ServicioRealizadoId");
            });
        }
    }
}