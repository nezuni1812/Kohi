﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Kohi.Models
{
    public class ProductModel : INotifyPropertyChanged
    {
        public int Id { get; set; }  // Primary Key
        public string? Name { get; set; }
        public int? CategoryId { get; set; }  // Foreign Key to Category
        public bool IsActive { get; set; } = true;
        public bool IsTopping { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public CategoryModel Category { get; set; }
        public List<ProductVariantModel> ProductVariants { get; set; } = new List<ProductVariantModel>(); // Matches Ref: product_variants.product_id > products.id

        public event PropertyChangedEventHandler? PropertyChanged;

    }
}
