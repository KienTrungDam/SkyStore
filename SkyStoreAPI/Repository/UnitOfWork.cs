using AutoMapper;
using Microsoft.AspNetCore.Identity;
using SkyStoreAPI.Data;
using SkyStoreAPI.Models;
using SkyStoreAPI.Repository.IRepository;

namespace SkyStoreAPI.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        public ICategoryRepository Category { get; }
        public IProductRepository Product { get; }
        public IUserRepository User { get; }  
        public IApplicationUserRepository ApplicationUser { get; }
        public IShoppingCartRepository ShoppingCart { get; }
        public IShoppingCartItemRepository ShoppingCartItem { get; }
        public UnitOfWork(ApplicationDbContext db, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, IConfiguration configuration, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
            _roleManager = roleManager;
            _userManager = userManager;
            _configuration = configuration;

            Category = new CategoryRepository(_db);
            Product = new ProductRepository(_db);
            User = new UserRepository(_db, _userManager, _roleManager, _mapper, _configuration);
            ApplicationUser = new ApplicationUserRepository(_db);   
            ShoppingCart = new ShoppingCartRepository(_db);
            ShoppingCartItem = new ShoppingCartItemRepository(_db);
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
