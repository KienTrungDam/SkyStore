using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkyStoreAPI.Models
{
    public class ShoppingCartItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("ShoppingCart")]
        public int ShoppingCartId { get; set; }
        [ValidateNever]
        public ShoppingCart ShoppingCart { get; set; }

        [Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        [ValidateNever]
        public Product Product { get; set; }
        [Required]
        public int Quantity { get; set; }

    }
}
