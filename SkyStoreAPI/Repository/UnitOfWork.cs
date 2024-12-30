using SkyStoreAPI.Data;
using SkyStoreAPI.Repository.IRepository;

namespace SkyStoreAPI.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;
        public ICategoryRepository Category { get; }
        public IProductRepository Product { get; }
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Category = new CategoryRepository(_db);
            Product = new ProductRepository(_db);
        }
    }
}
