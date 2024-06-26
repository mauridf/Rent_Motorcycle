﻿using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Rent_Motorcycle.Models
{
    public class Locacao
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Column(TypeName = "Date")]
        public DateTime DataInicio { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Column(TypeName = "Date")]
        public DateTime DataTermino { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Column(TypeName = "Date")]
        public DateTime DataPrevistaTermino { get; set; }

        [Required]
        public int TipoPlanoId { get; set; }

        [ForeignKey("TipoPlanoId")]
        [JsonIgnore]
        public TipoPlano? TipoPlano { get; set; }

        [Required]
        public int MotoId { get; set; }

        [ForeignKey("MotoId")]
        [JsonIgnore]
        public Moto? Moto { get; set; }

        [Required]
        public int EntregadorId { get; set; }

        [ForeignKey("EntregadorId")]
        [JsonIgnore]
        public Entregador? Entregador { get; set; }

        [Required]
        public decimal ValorLocacao { get; set; }
    }
}