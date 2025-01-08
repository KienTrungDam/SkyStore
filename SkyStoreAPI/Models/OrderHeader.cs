using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using SkyStoreAPI.Untility;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkyStoreAPI.Models
{
    public class OrderHeader
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("ApplicationUser")]
        public string ApplicationUserId { get; set; }
        [ValidateNever]
        public ApplicationUser User { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public IEnumerable<OrderDetail> OrderDetails { get; set; }
        public int ItemsTotal { get; set; }
        public double OrderTotal { get; set; }
        public string? OrderStatus { get; set; }

    }
}
