using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SkyStoreAPI.Data;
using SkyStoreAPI.Models;
using SkyStoreAPI.Models.DTO;
using SkyStoreAPI.Repository.IRepository;
using SkyStoreAPI.Untility;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SkyStoreAPI.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private string _secretKey;
        public UserRepository(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IMapper mapper, IConfiguration configuration)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _secretKey = configuration.GetValue<string>("ApiSettings:SecretKey");
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO loginRequestDTO)
        {
            var user = await _userManager.FindByNameAsync(loginRequestDTO.UserName);
            bool ValidPassword = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);
            if (!ValidPassword || user == null)
            {
                return null;
            }
            var role = await _userManager.GetRolesAsync(user);
            var token = GeneratedJWTToken(user, role.FirstOrDefault());
            return new LoginResponseDTO
            {
                Token = token,
                user = _mapper.Map<UserDTO>(user),
            };
            
        }

        public async Task<UserDTO> RegisterAsync(RegisterRequestDTO registerRequestDTO)
        {
            ApplicationUser user = new ApplicationUser
            {
                UserName = registerRequestDTO.UserName,
                Email = registerRequestDTO.Email,
                Name = registerRequestDTO.Name,
                State = registerRequestDTO.State,
                StreetAddress = registerRequestDTO.StreetAddress,
                NormalizedEmail = registerRequestDTO.UserName.ToUpper(),
                City = registerRequestDTO.City,
                
            };
            try
            {
                var create = await _userManager.CreateAsync(user, registerRequestDTO.Password);
                if(create.Succeeded)
                {
                    //Tao role truoc khi chua co role nao chi chay 1 lan
                    /*if (!_roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
                    {
                        await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                        await _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee));
                        await _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer));

                    }*/
                    if (_roleManager.RoleExistsAsync(registerRequestDTO.Role).GetAwaiter().GetResult())
                    {
                        await _userManager.AddToRoleAsync(user, registerRequestDTO.Role);
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, SD.Role_Customer);
                    }
                    var getuser = _db.ApplicationUsers.FirstOrDefault(u => u.UserName == registerRequestDTO.UserName);
                    return _mapper.Map<UserDTO>(getuser);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {

            }
            return new UserDTO();
        }

        public bool UniqueUserName(string userName)
        {
            var user = _db.ApplicationUsers.FirstOrDefault(u => u.UserName == userName);
            if (user == null)
            {
                return true;
            }
            return false;
        }
        private string GeneratedJWTToken(ApplicationUser user, string role)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey); //key de ma hoa token - chuyen sang kieu byte
            //trong token co role va id cua user
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new Claim[]
               {
                   new Claim(ClaimTypes.Name, user.Name), 
                   new Claim(ClaimTypes.Role, role),
                   new Claim(ClaimTypes.NameIdentifier, user.Id),
                   new Claim(ClaimTypes.Email, user.Email)
               }),
                Expires = DateTime.UtcNow.AddDays(3),
                //xac dinh key và thuat toan ma hoa
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature) //ma hoa token
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }
}
