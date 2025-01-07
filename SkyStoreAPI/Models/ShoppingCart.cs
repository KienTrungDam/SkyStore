using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkyStoreAPI.Models
{
    public class ShoppingCart
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("ApplicationUser")]
        [Required]
        public string UserId { get; set; }
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }
        //public string StripePaymentIntentId { get; set; }
        public IEnumerable<ShoppingCartItem> ShoppingCartItems { get; set; }
        //[NotMapped]
        public double CartTotal { get; set; }
        //[NotMapped]
        public int ItemsTotal { get; set; }
    }
}
