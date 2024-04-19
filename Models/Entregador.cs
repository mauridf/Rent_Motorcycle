using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rent_Motorcycle.Models
{
    public class Entregador
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; }

        [Required]
        [MaxLength(14)]
        public string CNPJ { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DataNascimento { get; set; }

        [Required]
        [MaxLength(20)]
        public string CNH { get; set; }

        [Required]
        [RegularExpression(@"^(A|B|AB)$")]
        public string TipoCNH { get; set; }

        [Required]
        [MaxLength]
        public byte[] ImagemCNH { get; set; }
    }
}
