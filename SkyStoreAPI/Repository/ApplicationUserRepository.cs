using SkyStoreAPI.Data;
using SkyStoreAPI.Models;
using SkyStoreAPI.Repository.IRepository;

namespace SkyStoreAPI.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private readonly ApplicationDbContext _db;
        public ApplicationUserRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public async Task<ApplicationUser> UpdateAsync(ApplicationUser applicationUser)
        {
            _db.ApplicationUsers.Update(applicationUser);
            await _db.SaveChangesAsync();
            return applicationUser;
        }
    }
}
