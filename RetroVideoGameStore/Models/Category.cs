using System.ComponentModel.DataAnnotations;
namespace RetroVideoGameStore.Models
{


    public class Category
    {
        public int Id { get; set; }

        [Required]
        public required string Name { get; set; }
        //add commen       
    }
}
