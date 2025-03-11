using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Models
{
    internal class Product
    {
        [Key]
        public int Id { get; set; }  // Primary Key

        [Required]
        public required string Name { get; set; }

        // Category is optional so we use nullable int and nullable Category
        [ForeignKey("Category")]
        public int? CategoryId { get; set; }  // Foreign Key to Category

        public Category? Category { get; set; }  // Navigation Property
    }
}
