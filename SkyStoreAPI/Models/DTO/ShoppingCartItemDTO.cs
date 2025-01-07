using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkyStoreAPI.Models
{
    public class ShoppingCartItemDTO
    {
        public int Id { get; set; }

        public int ShoppingCartId { get; set; }

        public int ProductId { get; set; }
        public int Quantity { get; set; }

    }
}
