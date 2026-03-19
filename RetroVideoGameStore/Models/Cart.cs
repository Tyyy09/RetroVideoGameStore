using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace RetroVideoGameStore.Models
{


    public class Cart
    {

        public int Id { get; set; }
        public int ProductId { get; set; }
        public string CustomerId { get; set; }
        public int Quantity { get; set; }
        public DateTime DateCreated { get; set; }
        public double Price { get; set; }
        // Reference to parent
        public Product Product { get; set; }
    }
}