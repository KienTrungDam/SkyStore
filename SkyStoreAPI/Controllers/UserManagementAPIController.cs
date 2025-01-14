using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SkyStoreAPI.Models.DTO;
using SkyStoreAPI.Models;
using SkyStoreAPI.Repository.IRepository;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using SkyStoreAPI.Untility;

namespace SkyStoreAPI.Controllers
{
    [Route("api/UserManagement")]
    [ApiController]
    public class UserManagementAPIController : ControllerBase
    {
        protected APIResponse _response;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UserManagementAPIController(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> uuserManager, RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _response = new();
            _userManager = uuserManager;
            _roleManager = roleManager;
        }
        [HttpGet]
        [Authorize(Roles = SD.Role_Admin)]
        //[ResponseCache(Duration = 30)]//khi thuc hien get villas giong nhau trong 30s cac hanh dong tiep theo se lay du lieu tu lan 1 vaf ko can goi api 
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse>> GetUsers()
        {
            IEnumerable<ApplicationUser> users = await _userManager.Users.ToListAsync();
            foreach (var user in users)
            {
                user.Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
            }
            _response.Result = _mapper.Map<List<UserDTO>>(users);
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }
        [HttpGet("UserName/{UserName}", Name = "GetUserByUserName")]
        public async Task<ActionResult<APIResponse>> GetUserByUserName(string UserName)
        {
            try
            {
                if (UserName == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    return BadRequest(_response);
                }
                ApplicationUser users = await _unitOfWork.ApplicationUser.GetAsync(u => u.UserName.ToLower() == UserName.ToLower());
                if (users == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    return NotFound(_response);
                }
                users.Role = (await _userManager.GetRolesAsync(users)).FirstOrDefault();
                _response.Result = _mapper.Map<UserDTO>(users);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
                _response.StatusCode = HttpStatusCode.BadRequest;
            }
            return _response;
        }
        [HttpGet("Id/{Id}", Name = "GetUserById")]

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse>> GetUserById(string Id)
        {
            try
            {
                if (Id == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    return BadRequest(_response);
                }
                ApplicationUser users = await _unitOfWork.ApplicationUser.GetAsync(u => u.Id.ToLower() == Id.ToLower());
                if (users == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    return NotFound(_response);
                }
                users.Role = (await _userManager.GetRolesAsync(users)).FirstOrDefault();
                _response.Result = _mapper.Map<UserDTO>(users);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
                _response.StatusCode = HttpStatusCode.BadRequest;
            }
            return _response;
        }
        [HttpDelete("{id}", Name = "DeleteUser")]
        [Authorize(Roles = SD.Role_Admin)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse>> DeleteUser(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest();
                }
                var user = await _unitOfWork.ApplicationUser.GetAsync(u => u.Id == id);
                if (user == null)
                {
                    return NotFound();
                }
                await _unitOfWork.ApplicationUser.RemoveAsync(user);
                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };

            }
            return _response;
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<ActionResult<APIResponse>> CreateUser([FromForm] RegisterRequestDTO requestDTO)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(requestDTO.UserName);

                if (user != null)
                {
                    throw new Exception("UserName exists");
                }

                var userDto = await _unitOfWork.User.RegisterAsync(requestDTO);

                if (userDto == null)
                {
                    throw new Exception("Error while registering");
                }
                _response.Result = userDto;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.Message);
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
        }
        [HttpPut("Role/{id}", Name = "UpdateRoleToUser")]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<ActionResult<APIResponse>> UpdateRoleToUser(string id, string role)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                var rolecurrent = await _userManager.GetRolesAsync(user);
                if (user == null)
                {
                    throw new Exception("User does not exists");
                }
                if(role != rolecurrent.FirstOrDefault())
                {
                    bool isValid = await _roleManager.RoleExistsAsync(role);
                    if (isValid)
                    {
                        await _userManager.RemoveFromRolesAsync(user, rolecurrent);
                        await _userManager.AddToRoleAsync(user, role);
                    }
                    else
                    {
                        throw new Exception("Role is not valid");
                    }
                }
                var userDTO = _mapper.Map<UserDTO>(user);
                userDTO.Role = role;
                _response.Result = userDTO;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.Message);
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
        }
        [HttpPost("LockUnlock")]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<ActionResult<APIResponse>> LockUnlock(string id)
        {

            var objFromDb = await _userManager.FindByIdAsync(id);
            if (objFromDb == null)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Error while Locking/Unlocking");
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }

            if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
            {
                //user is currently locked and we need to unlock them
                objFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            }
            await _unitOfWork.SaveAsync();
            _response.StatusCode = HttpStatusCode.NoContent;
            return Ok(_response);
        }
    }
}
