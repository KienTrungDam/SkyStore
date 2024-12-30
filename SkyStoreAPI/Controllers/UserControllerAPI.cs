using Azure;
using Microsoft.AspNetCore.Mvc;
using SkyStoreAPI.Models.DTO;
using SkyStoreAPI.Models;
using System.Net;
using AutoMapper;
using SkyStoreAPI.Repository.IRepository;
using Microsoft.AspNetCore.Identity;

namespace SkyStoreAPI.Controllers
{
    [Route("api/UserAPI")]
    [ApiController]
    public class UserControllerAPI : ControllerBase
    {
        private readonly APIResponse _response;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserControllerAPI(IMapper mapper, IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _response = new ();
            _userManager = userManager;
        }
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Login([FromBody]LoginRequestDTO loginRequestDTO)
        {
            LoginResponseDTO loginResponseDTO = await _unitOfWork.User.LoginAsync(loginRequestDTO);
            try
            {
                if(loginResponseDTO == null)
                {
                    throw new Exception("UserName or Password is incorrect");
                }
                _response.Result = loginResponseDTO;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add(ex.Message);
                return BadRequest(_response);
            }
        }
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Register([FromBody] RegisterRequestDTO registerRequestDTO)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(registerRequestDTO.UserName);
                //bool valid = _unitOfWork.User.UniqueUserName(registerRequestDTO.UserName);   
                if (user == null)
                {
                    UserDTO userDTO = await _unitOfWork.User.RegisterAsync(registerRequestDTO);
                    if (userDTO == null)
                    {
                        throw new Exception("Error while registering");
                    }
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.Result = userDTO;
                    return Ok(_response);
                }
                else
                {
                    throw new Exception("UserName is already exist");
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add(ex.Message);
                return BadRequest(_response);
            }
        }
    }
}
