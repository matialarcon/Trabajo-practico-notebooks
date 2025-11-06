using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace NotebookApp.Models;

public partial class NotebooksContext : IdentityDbContext
{
    public NotebooksContext()
    {
    }

    public NotebooksContext(DbContextOptions<NotebooksContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Equipo> Equipos { get; set; }

    public virtual DbSet<Prestamo> Prestamos { get; set; }

    public virtual DbSet<PrestamoDetalle> PrestamoDetalles { get; set; }

    public virtual DbSet<Profesor> Profesores { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //optionsBuilder.UseSqlite("Data Source=C:\\Users\\NoteBook\\OneDrive\\Documents\\notebooks.db;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Equipo>(entity =>
        {
            entity.Property(e => e.EquipoId).HasColumnName("equipoId");
            entity.Property(e => e.NumeroInventario).HasColumnName("numeroInventario");
        });

        modelBuilder.Entity<Prestamo>(entity =>
        {
            entity.Property(e => e.PrestamoId).HasColumnName("prestamoId");
            entity.Property(e => e.FechaEntrada).HasColumnName("fechaEntrada");
            entity.Property(e => e.FechaSalida).HasColumnName("fechaSalida");
            entity.Property(e => e.ProfesorId).HasColumnName("profesorId");

            entity.HasOne(d => d.Profesor).WithMany(p => p.Prestamos)
                .HasForeignKey(d => d.ProfesorId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<PrestamoDetalle>(entity =>
        {
            entity.ToTable("prestamoDetalle");

            entity.Property(e => e.PrestamoDetalleId).HasColumnName("prestamoDetalleId");
            entity.Property(e => e.EquipoId).HasColumnName("equipoId");
            entity.Property(e => e.PrestamoId).HasColumnName("prestamoId");

            entity.HasOne(d => d.Equipo).WithMany(p => p.PrestamoDetalles)
                .HasForeignKey(d => d.EquipoId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Prestamo).WithMany(p => p.PrestamoDetalles)
                .HasForeignKey(d => d.PrestamoId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Profesor>(entity =>
        {
            entity.HasKey(e => e.ProfesorId);

            entity.Property(e => e.ProfesorId).HasColumnName("profesorId");
            entity.Property(e => e.Dni).HasColumnName("DNI");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
