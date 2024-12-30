using SkyStoreAPI.Models;

namespace SkyStoreAPI.Repository.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<Product> UpdateAsync(Product entity);
    }
}
