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
                if (id <= 0)
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

                order.OrderDetails = await _unitOfWork.OrderDetail.GetAllAsync(u => u.OrderHeaderId == order.Id, includeProperties: "Product");
                _response.Result = _mapper.Map<OrderHeaderDTO>(order);

                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.ErrorMessages.Add(ex.Message);
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return _response;
            }
        }
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<APIResponse>> CreateOrder()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("User is unauthorized");
                    _response.StatusCode = HttpStatusCode.Unauthorized;
                    return Unauthorized(_response);
                }
                ShoppingCart cart = await _unitOfWork.ShoppingCart.GetAsync(u => u.UserId == user.Id);
                cart.ShoppingCartItems = await _unitOfWork.ShoppingCartItem.GetAllAsync(u => u.ShoppingCartId == cart.Id, includeProperties: "Product");
                if (cart == null || cart.ShoppingCartItems.Count() == 0)
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("ShoppingCart is null");
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                //create orderheader
                OrderHeader orderHeader = new OrderHeader()
                {
                    ApplicationUserId = user.Id,
                    OrderDate = DateTime.Now,
                    OrderDetails = new List<OrderDetail>(),
                    ItemsTotal = cart.ItemsTotal,
                    OrderTotal = cart.CartTotal,
                    OrderStatus = SD.Order_Pending,
                    PaymentStatus = SD.PaymentStatusPending,
                    StripePaymentIntentId = null

                };
                await _unitOfWork.OrderHeader.CreateAsync(orderHeader);
                //create orderdetail
                foreach (var shoppingCartItem in cart.ShoppingCartItems)
                {
                    var orderDetail = new OrderDetail()
                    {
                        OrderHeaderId = orderHeader.Id,
                        ProductId = shoppingCartItem.Product.Id,
                        Quantity = shoppingCartItem.Quantity,
                        Price = shoppingCartItem.Product.Price,
                        //  MenuItem = null
                    };
                    await _unitOfWork.OrderDetail.CreateAsync(orderDetail);
                }
                orderHeader.OrderDetails = await _unitOfWork.OrderDetail.GetAllAsync(u => u.OrderHeaderId == orderHeader.Id, includeProperties: "Product");
                //delete ShoppingCart
                await _unitOfWork.ShoppingCart.RemoveAsync(cart);

                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = _mapper.Map<OrderHeaderDTO>(orderHeader);
                return CreatedAtRoute("GetOrder", new { id = orderHeader.Id }, _response);
            }
            catch (Exception ex)
            {
                _response.ErrorMessages.Add(ex.Message);
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }

            

        }
        [HttpPut]
        [Authorize]
        public async Task<ActionResult<APIResponse>> UpdateStatusOrder(int id, string status)
        {
            OrderHeader orderHeader = await _unitOfWork.OrderHeader.GetAsync(u => u.Id == id);
            if (orderHeader == null)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Id OrderHeader is not valid");
                _response.StatusCode = HttpStatusCode.Unauthorized;
                return BadRequest(_response);
            }
            if(_unitOfWork.OrderHeader.IsValidStatus(status) == false)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Status for update is not valid");
                _response.StatusCode = HttpStatusCode.Unauthorized;
                return BadRequest(_response);
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("User is unauthorized");
                _response.StatusCode = HttpStatusCode.Unauthorized;
                return Unauthorized(_response);
            }
            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
            if(role == SD.Role_Employee || role == SD.Role_Admin)
            {
                if (_unitOfWork.OrderHeader.IsValidStatusTransition(orderHeader.OrderStatus, status))
                {
                    if(status == SD.Order_Processing && orderHeader.PaymentStatus == SD.PaymentStatusApproved) {
                    orderHeader.OrderStatus = status;
                }
                else
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add("Some thing went wrong");
                    return BadRequest(_response);
                }
            }
            else
            {
                OrderHeader orderHeaderCustomer = await _unitOfWork.OrderHeader.GetAsync(u => u.ApplicationUserId == user.Id && u.Id == id);
                if (orderHeaderCustomer.OrderStatus != SD.Order_Cancelled || orderHeaderCustomer.OrderStatus != SD.Order_Completed)
                {
                    if(status == SD.Order_Cancelled)
                    {
                        orderHeaderCustomer.OrderStatus = status;
                        orderHeaderCustomer.PaymentStatus = SD.PaymentStatusRejected;
                        await _unitOfWork.OrderHeader.UpdateAsync(orderHeaderCustomer);
                        _response.Result = _mapper.Map<OrderHeaderDTO>(orderHeaderCustomer);
                        _response.StatusCode = HttpStatusCode.NoContent;
                        return Ok(_response);
                    }
                    else
                    {
                        _response.IsSuccess = false;
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.ErrorMessages.Add("You cannot update status");
                        return BadRequest(_response);
                    }
                    
                }
                else if(orderHeaderCustomer.OrderStatus == SD.Order_Cancelled || orderHeaderCustomer.OrderStatus == SD.Order_Completed)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add("You cannot update a completed or cancelled order.");
                    return BadRequest(_response);
                }
            }
            await _unitOfWork.OrderHeader.UpdateAsync(orderHeader);
            _response.Result = _mapper.Map<OrderHeaderDTO>(orderHeader);   
            _response.StatusCode = HttpStatusCode.NoContent;
            return Ok(_response);
        }
        [HttpDelete]
        [Authorize]
        public async Task<ActionResult<APIResponse>> DeleteOrder(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("User is unauthorized");
                _response.StatusCode = HttpStatusCode.Unauthorized;
                return Unauthorized(_response);
            }
            OrderHeader orderHeader = await _unitOfWork.OrderHeader.GetAsync(u => u.Id == id);
            if (orderHeader == null)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Id Order is not valid");
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
            await _unitOfWork.OrderHeader.RemoveAsync(orderHeader);
            _response.StatusCode = HttpStatusCode.NoContent;
            return Ok(_response);
        }
    }
}
