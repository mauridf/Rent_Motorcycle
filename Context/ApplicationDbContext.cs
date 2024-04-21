using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Rent_Motorcycle.Models;
using System;

namespace Rent_Motorcycle.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Moto> Motos { get; set; }
        public DbSet<Entregador> Entregadores { get; set; }
        public DbSet<Locacao> Locacoes { get; set; }
        public DbSet<TipoPlano> TipoPlanos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Configura um padrão para todas as propriedades DateTime
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(new ValueConverter<DateTime, DateTime>(
                            v => v.ToUniversalTime(),
                            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)));
                    }
                }
            }

            modelBuilder.Entity<TipoPlano>()
                .ToTable("TiposPlanos");

            // Configuração específica do Postgre (opcional)
            modelBuilder.HasDefaultSchema("public"); // Define o esquema padrão para o Postgre

            // Configuração de índices únicos
            modelBuilder.Entity<Moto>()
                .HasIndex(m => m.Placa)
                .IsUnique();

            modelBuilder.Entity<Entregador>()
                .HasIndex(e => e.CNPJ)
                .IsUnique();

            modelBuilder.Entity<Entregador>()
                .HasIndex(e => e.CNH)
                .IsUnique();

            // Configuração de identificadores autoincrementais
            modelBuilder.Entity<Moto>()
                .Property(m => m.Id)
                .UseIdentityColumn(); // Define o identificador como autoincremento

            modelBuilder.Entity<Entregador>()
                .Property(e => e.Id)
                .UseIdentityColumn(); // Define o identificador como autoincremento

            modelBuilder.Entity<Locacao>()
                .Property(l => l.Id)
                .UseIdentityColumn(); // Define o identificador como autoincremento

            modelBuilder.Entity<TipoPlano>()
                .Property(t => t.Id)
                .UseIdentityColumn(); // Define o identificador como autoincremento

            // Relacionamentos entre as entidades
            modelBuilder.Entity<Locacao>()
                .HasOne(l => l.Moto)
                .WithMany()
                .HasForeignKey(l => l.MotoId)
                .OnDelete(DeleteBehavior.Restrict); // Impede a exclusão da moto se houver locações associadas

            modelBuilder.Entity<Locacao>()
                .HasOne(l => l.TipoPlano)
                .WithMany()
                .HasForeignKey(l => l.TipoPlanoId);

            // Popula a tabela TipoPlano com os tipos de plano e seus custos
            modelBuilder.Entity<TipoPlano>().HasData(
                new TipoPlano { Id = 1, Nome = "Plano 7 Dias", Dias = 7, Custo = 30 },
                new TipoPlano { Id = 2, Nome = "Plano 15 Dias", Dias = 15, Custo = 28 },
                new TipoPlano { Id = 3, Nome = "Plano 30 Dias", Dias = 30, Custo = 22 },
                new TipoPlano { Id = 4, Nome = "Plano 45 Dias", Dias = 45, Custo = 20 },
                new TipoPlano { Id = 5, Nome = "Plano 50 Dias", Dias = 50, Custo = 18 }
            );
        }
    }
}