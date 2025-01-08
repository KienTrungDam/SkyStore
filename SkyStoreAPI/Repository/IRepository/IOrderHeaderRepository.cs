using SkyStoreAPI.Models;

namespace SkyStoreAPI.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        Task<OrderHeader> UpdateAsync(OrderHeader entity);
        public bool IsValidStatusTransition(string currentStatus, string newStatus);
        public bool IsValidStatus(string status);
    }
}
