using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PruebaPlaywright
{
    public class Product
    {
        public decimal Price { get; init; }
        public Product(decimal price)
        {
            Price = price;
        }

        public override string ToString()
        {

            return Price+"";
    
        }
    }
}
