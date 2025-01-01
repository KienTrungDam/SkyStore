using SkyStoreAPI.Models;

namespace SkyStoreAPI.Repository.IRepository
{
    public interface IApplicationUserRepository : IRepository<ApplicationUser>
    {
        Task<ApplicationUser> UpdateAsync(ApplicationUser applicationUser);
    }
}
