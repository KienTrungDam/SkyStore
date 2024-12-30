using System.ComponentModel.DataAnnotations;

namespace SkyStoreAPI.Models.DTO
{
    public class CategoryCreateDTO
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
