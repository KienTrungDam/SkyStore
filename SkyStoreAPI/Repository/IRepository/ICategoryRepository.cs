using SkyStoreAPI.Models;

namespace SkyStoreAPI.Repository.IRepository
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<Category> UpdateAsync(Category entity);
    }
}
