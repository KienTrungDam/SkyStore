using System.ComponentModel.DataAnnotations;

namespace SkyStoreAPI.Models.DTO
{
    public class CategoryUpdateDTO
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        [Required]
        public string? Description { get; set; }
    }
}
