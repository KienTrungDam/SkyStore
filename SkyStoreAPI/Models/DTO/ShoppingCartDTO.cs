using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkyStoreAPI.Models
{
    public class ShoppingCartDTO
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        //public string StripePaymentIntentId { get; set; }
        public ICollection<ShoppingCartItemDTO> ShoppingCartItems { get; set; }
        public double CartTotal { get; set; }
        public int ItemsTotal { get; set; }
    }
}   
