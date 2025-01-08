using SkyStoreAPI.Models;

namespace SkyStoreAPI.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        Task<OrderHeader> UpdateAsync(OrderHeader entity);
        public bool IsValidStatus(string currentStatus, string newStatus);
    }
}
