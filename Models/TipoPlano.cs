using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rent_Motorcycle.Models
{
    public class TipoPlano
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; }

        [Required] 
        public int Dias { get; set; }

        [Required]
        public decimal Custo { get; set; }
    }
}
