using System.ComponentModel.DataAnnotations;

namespace SkyStoreAPI.Models.DTO
{
    public class CategoryDTO
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
