﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Models
{
    internal class Category
    {
        [Key]
        public int Id { get; set; }  // Primary Key

        [Required]
        public string Name { get; set; }
    }
}
