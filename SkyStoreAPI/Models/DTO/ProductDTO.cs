using System.ComponentModel.DataAnnotations.Schema;

namespace SkyStoreAPI.Models.DTO
{
    public class ProductDTO
    {
        public int Id { get; set; }
        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public string? Description { get; set; }
        public string ImageUrl { get; set; }
    }
}
