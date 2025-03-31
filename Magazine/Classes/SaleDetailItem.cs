using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magazine.Classes
{
    public class SaleDetailItem
    {
        public int PositionNumber { get; set; }
        public string ProductName { get; set; }
        public string Specifications { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public decimal Price { get; set; }
        public decimal Total => Quantity * Price;
        public decimal ManualDiscount { get; set; }
        public decimal AutoDiscount { get; set; }
    }
}
