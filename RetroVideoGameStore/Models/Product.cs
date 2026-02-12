using System.ComponentModel.DataAnnotations;

namespace RetroVideoGameStore.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        public required string Name { get; set; }
        [DisplayFormat(DataFormatString ="{0:c}")]
        public double Price { get; set; }
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        public string? Photo { get; set; }
        public string? Description { get; set; }


        // Reference the child model (1 category -> many products)
        public Category? Category { get; set; }

    }
}
