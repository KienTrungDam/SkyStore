using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkyStoreAPI.Models.DTO
{
    public class ProductCreateDTO
    {
        [Required]
        public int CategoryId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public double Price { get; set; }
        public string Description { get; set; }

        public string ImageUrl { get; set; }
    }
}
