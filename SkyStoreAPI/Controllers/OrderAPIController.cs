using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SkyStoreAPI.Models;
using SkyStoreAPI.Models.DTO;
using SkyStoreAPI.Repository.IRepository;
using SkyStoreAPI.Untility;
using System.Net;

namespace SkyStoreAPI.Controllers
{
    [Route("api/OrderAPI")]
    [ApiController]
    public class OrderAPIController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        protected APIResponse _response;
        private readonly UserManager<ApplicationUser> _userManager;
        public OrderAPIController(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _response = new APIResponse();
        }
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<APIResponse>> GetAllOrder()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("User is unauthorized");
                _response.StatusCode = HttpStatusCode.Unauthorized;
                return Unauthorized(_response);
            }
            try
            {
                IEnumerable<OrderHeader> orders = await _unitOfWork.OrderHeader.GetAllAsync();

                bool isAdmin = await _userManager.IsInRoleAsync(user, SD.Role_Admin);
                bool isEmployee = await _userManager.IsInRoleAsync(user, SD.Role_Employee);

                if (isAdmin || isEmployee)
                {
                    _response.Result = _mapper.Map<IEnumerable<OrderHeaderDTO>>(orders);
                }
                else
                {
                    var order = await _unitOfWork.OrderHeader.GetAsync(u => u.ApplicationUserId == user.Id);
                    _response.Result = _mapper.Map<IEnumerable<OrderHeaderDTO>>(order);
                }
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
        }
        [HttpGet("{id:int}", Name = "GetOrder")]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public async Task<ActionResult<APIResponse>> GetOrder(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("User is unauthorized");
                _response.StatusCode = HttpStatusCode.Unauthorized;
                return Unauthorized(_response);
            }
            try
            {
                if (id == 0)
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Id is not valid");
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                OrderHeader order = await _unitOfWork.OrderHeader.GetAsync(u => u.Id == id);

                if (order == null)
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Not found");
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                order.OrderDetail = await _unitOfWork.OrderDetail.GetAllAsync(u => u.OrderHeaderId == order.Id, includeProperties: "MenuItem");
                _response.Result = _mapper.Map<OrderHeaderDTO>(order);

                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return _response;
            }
        }
    }
}
