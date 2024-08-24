﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockInformationSystem.Core
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int StockQuantity { get; set; }
        public decimal Price { get; set; }
    }
}
