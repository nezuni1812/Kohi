﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.ViewModels
{
    public class Sales
    {
        public string Product { get; set; } = "";

        public double SalesRate { get; set; }
    }
    public class ChartViewModel
    {
        public List<Sales> Data { get; set; }

        public ChartViewModel()
        {
            Data = new List<Sales>()
        {
            new Sales(){Product = "iPad", SalesRate = 25},
            new Sales(){Product = "iPhone", SalesRate = 35},
            new Sales(){Product = "MacBook", SalesRate = 15},
            new Sales(){Product = "Mac", SalesRate = 5},
            new Sales(){Product = "Others", SalesRate = 10},
        };
        }
    }
}
