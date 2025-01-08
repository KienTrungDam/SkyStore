using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkyStoreAPI.Models.DTO
{
    public class OrderHeaderDTO
    {
        public int Id { get; set; }
        public string AppilcationId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public IEnumerable<OrderDetail> OrderDetail { get; set; }
        public int ItemsTotal { get; set; }
        public double OrderTotal { get; set; }
        public string? OrderStatus { get; set; }
    }
}
