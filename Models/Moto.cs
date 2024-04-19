using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rent_Motorcycle.Models
{
    public class Moto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Ano { get; set; }

        [Required]
        public string Modelo { get; set; }

        [Required]
        [MaxLength(7)]
        public string Placa { get; set; }
    }
}
