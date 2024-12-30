using Microsoft.AspNetCore.Identity.Data;
using SkyStoreAPI.Models;
using SkyStoreAPI.Models.DTO;

namespace SkyStoreAPI.Repository.IRepository
{
    public interface IUserRepository
    {
        bool UniqueUserName (string userName);
        Task<LoginResponseDTO> LoginAsync(LoginRequestDTO loginRequestDTO);
        Task<UserDTO> RegisterAsync(RegisterRequestDTO registerRequestDTO);
    }
}
