using System;
using System.Collections.Generic;
using Api_Usuario.Models;
using Microsoft.EntityFrameworkCore;

namespace Api_Usuario.Data;

public partial class UsuarioContext : DbContext
{
    public UsuarioContext(DbContextOptions<UsuarioContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("adminDrug");

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuario__3213E83F5DF604D1");

            entity.ToTable("Usuario");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClaveUsuario)
                .IsUnicode(false)
                .HasColumnName("clave_usuario");
            entity.Property(e => e.Region)
                .IsUnicode(false)
                .HasColumnName("region");
            entity.Property(e => e.Uid).IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
