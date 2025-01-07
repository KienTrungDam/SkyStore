using SkyStoreAPI.Data;
using SkyStoreAPI.Models;
using SkyStoreAPI.Repository.IRepository;

namespace SkyStoreAPI.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private readonly ApplicationDbContext _db;
        public ShoppingCartRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public async Task<ShoppingCart> UpdateAsync(ShoppingCart entity)
        {
            entity.CartTotal = entity.ShoppingCartItems.Sum(u => u.Product.Price * u.Quantity);
            entity.ItemsTotal = entity.ShoppingCartItems.Count();
            _db.ShoppingCarts.Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }
    }
}
