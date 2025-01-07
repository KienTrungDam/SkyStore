using SkyStoreAPI.Models;

namespace SkyStoreAPI.Repository.IRepository
{
    public interface IShoppingCartItemRepository : IRepository<ShoppingCartItem>
    {
        Task<ShoppingCartItem> UpdateAsync(ShoppingCartItem entity);
    }
}
