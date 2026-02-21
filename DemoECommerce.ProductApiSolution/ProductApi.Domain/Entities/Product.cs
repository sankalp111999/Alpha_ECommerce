using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductApi.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string? Name { get; set; }      //string? means value null ho sakti hai (nullable).
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
