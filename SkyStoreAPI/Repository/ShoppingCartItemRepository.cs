using SkyStoreAPI.Data;
using SkyStoreAPI.Models;
using SkyStoreAPI.Repository.IRepository;

namespace SkyStoreAPI.Repository
{
    public class ShoppingCartItemRepository : Repository<ShoppingCartItem>, IShoppingCartItemRepository
    {
        private readonly ApplicationDbContext _db;
        public ShoppingCartItemRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public async Task<ShoppingCartItem> UpdateAsync(ShoppingCartItem entity)
        {
            _db.ShoppingCartItems.Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }
    }
}
