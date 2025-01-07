using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SkyStoreAPI.Models;
using SkyStoreAPI.Models.DTO;
using SkyStoreAPI.Repository.IRepository;
using System.Collections;
using System.Net;
using System.Security.Claims;

namespace SkyStoreAPI.Controllers
{
    [Route("api/ShoppingCart")]
    [ApiController]
    public class ShoppingCartAPIController : ControllerBase
    {
        protected APIResponse _response;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ShoppingCartAPIController(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _mapper = mapper;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _response = new APIResponse();
        }
        [HttpGet]
        public async Task<ActionResult<APIResponse>> GetAllShoppingCart()
        {
            /*var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;*/
            IEnumerable<ShoppingCart> shoppingCarts = await _unitOfWork.ShoppingCart.GetAllAsync();
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = _mapper.Map<IEnumerable<ShoppingCartDTO>>(shoppingCarts);
            return Ok(_response);
        }
        [HttpGet("{Userid}", Name = "GetShoppingCart")]
        public async Task<ActionResult<APIResponse>> GetShoppingCart(string Userid)
        {
            var user = await _userManager.FindByIdAsync(Userid);
            if (user == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Invalid User ID");
                return BadRequest(_response);
            }
            /*var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;*/
            ShoppingCart shoppingCart = await _unitOfWork.ShoppingCart.GetAsync(u => u.UserId == Userid);
            if (shoppingCart == null)
            {
                //chua co ShoppingCart
                shoppingCart = new ShoppingCart
                {
                    UserId = Userid,
                    CartTotal = 0,
                    ItemsTotal = 0,
                    ShoppingCartItems = new List<ShoppingCartItem>()
                };
                await _unitOfWork.ShoppingCart.CreateAsync(shoppingCart);
                _response.Result = _mapper.Map<ShoppingCartDTO>(shoppingCart);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            else
            {
                shoppingCart.ShoppingCartItems = await _unitOfWork.ShoppingCartItem.GetAllAsync(u => u.ShoppingCartId == shoppingCart.Id, includeProperties: "Product");
                if (shoppingCart.ShoppingCartItems != null && shoppingCart.ShoppingCartItems.Count() > 0)
                {
                    await _unitOfWork.ShoppingCart.UpdateAsync(shoppingCart);
                }
                _response.Result = _mapper.Map<ShoppingCartDTO>(shoppingCart);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }

        }
        [HttpPost]
        //[Authorize(Roles = SD.Role_Admin)]
        public async Task<ActionResult<APIResponse>> CreateShoppingCart(string userId, int productId, int quantity)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Invalid User ID");
                return BadRequest(_response);
            }
            Product product = await _unitOfWork.Product.GetAsync(u => u.Id == productId);
            if (product == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Product item is not valid");
                return BadRequest(_response);
            }
            ShoppingCart shoppingCart = await _unitOfWork.ShoppingCart.GetAsync(u => u.UserId == userId);
            if (shoppingCart == null)
            {
                //chua co ShoppingCart
                ShoppingCart newShoppingCart = new ShoppingCart
                {
                    UserId = userId,
                    CartTotal = 0,
                    ItemsTotal = 0,
                    ShoppingCartItems = new List<ShoppingCartItem>()
                };
                await _unitOfWork.ShoppingCart.CreateAsync(newShoppingCart);
                ShoppingCartItem item = new ShoppingCartItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    ShoppingCartId = newShoppingCart.Id
                };
                await _unitOfWork.ShoppingCartItem.CreateAsync(item);
                newShoppingCart.ShoppingCartItems = await _unitOfWork.ShoppingCartItem.GetAllAsync(u => u.ShoppingCartId == newShoppingCart.Id, includeProperties: "Product");
                await _unitOfWork.ShoppingCart.UpdateAsync(newShoppingCart);

                _response.Result = _mapper.Map<ShoppingCartDTO>(newShoppingCart);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            else
            {
                //ShoppingCart da ton tai
                ShoppingCartItem cartItem = await _unitOfWork.ShoppingCartItem.GetAsync(u => u.ProductId == productId && u.ShoppingCartId == shoppingCart.Id, includeProperties: "Product");
                if (cartItem == null)
                {
                    //ShoppingCartItem chua ton tai
                    ShoppingCartItem item = new ShoppingCartItem
                    {
                        ProductId = productId,
                        Quantity = quantity,
                        ShoppingCartId = shoppingCart.Id
                    };
                    await _unitOfWork.ShoppingCartItem.CreateAsync(item);
                    shoppingCart.ShoppingCartItems = await _unitOfWork.ShoppingCartItem.GetAllAsync(u => u.ShoppingCartId == shoppingCart.Id, includeProperties: "Product");
                    await _unitOfWork.ShoppingCart.UpdateAsync(shoppingCart);

                    _response.Result = _mapper.Map<ShoppingCartDTO>(shoppingCart);
                    _response.StatusCode = HttpStatusCode.OK;
                    return Ok(_response);
                }
                else
                {
                    //ShoppingCartItem da ton tai
                    cartItem.Quantity = cartItem.Quantity + quantity;
                    await _unitOfWork.ShoppingCartItem.UpdateAsync(cartItem);
                    shoppingCart.ShoppingCartItems = await _unitOfWork.ShoppingCartItem.GetAllAsync(u => u.ShoppingCartId == shoppingCart.Id, includeProperties: "Product");
                    await _unitOfWork.ShoppingCart.UpdateAsync(shoppingCart);

                    _response.Result = _mapper.Map<ShoppingCartDTO>(shoppingCart);
                    _response.StatusCode = HttpStatusCode.OK;
                    return Ok(_response);
                }

            }
        }
        [HttpPut]
        //[Authorize(Roles = SD.Role_Admin)]
        public async Task<ActionResult<APIResponse>> UpdateShoppingCart(string userId, int productId, int quantity)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Invalid User ID");
                return BadRequest(_response);
            }
            Product product = await _unitOfWork.Product.GetAsync(u => u.Id == productId);
            if (product == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Product item is not valid");
                return BadRequest(_response);
            }
            ShoppingCart shoppingCart = await _unitOfWork.ShoppingCart.GetAsync(u => u.UserId == userId);
            if (shoppingCart == null)
            {
                //chua co ShoppingCart
                ShoppingCart newShoppingCart = new ShoppingCart
                {
                    UserId = userId,
                    CartTotal = 0,
                    ItemsTotal = 0,
                    ShoppingCartItems = new List<ShoppingCartItem>()
                };
                await _unitOfWork.ShoppingCart.CreateAsync(newShoppingCart);
                ShoppingCartItem item = new ShoppingCartItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    ShoppingCartId = newShoppingCart.Id
                };
                await _unitOfWork.ShoppingCartItem.CreateAsync(item);
                newShoppingCart.ShoppingCartItems = await _unitOfWork.ShoppingCartItem.GetAllAsync(u => u.ShoppingCartId == newShoppingCart.Id, includeProperties: "Product");
                await _unitOfWork.ShoppingCart.UpdateAsync(newShoppingCart);

                _response.Result = _mapper.Map<ShoppingCartDTO>(newShoppingCart);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            else
            {
                //ShoppingCart da ton tai
                ShoppingCartItem cartItem = await _unitOfWork.ShoppingCartItem.GetAsync(u => u.ProductId == productId && u.ShoppingCartId == shoppingCart.Id, includeProperties: "Product");
                if (cartItem == null)
                {
                    //ShoppingCartItem chua ton tai
                    ShoppingCartItem item = new ShoppingCartItem
                    {
                        ProductId = productId,
                        Quantity = quantity,
                        ShoppingCartId = shoppingCart.Id
                    };
                    await _unitOfWork.ShoppingCartItem.CreateAsync(item);
                    shoppingCart.ShoppingCartItems = await _unitOfWork.ShoppingCartItem.GetAllAsync(u => u.ShoppingCartId == shoppingCart.Id, includeProperties: "Product");
                    await _unitOfWork.ShoppingCart.UpdateAsync(shoppingCart);

                    _response.Result = _mapper.Map<ShoppingCartDTO>(shoppingCart);
                    _response.StatusCode = HttpStatusCode.OK;
                    return Ok(_response);
                }
                else
                {
                    //ShoppingCartItem da ton tai
                    cartItem.Quantity = quantity;
                    await _unitOfWork.ShoppingCartItem.UpdateAsync(cartItem);
                    shoppingCart.ShoppingCartItems = await _unitOfWork.ShoppingCartItem.GetAllAsync(u => u.ShoppingCartId == shoppingCart.Id, includeProperties: "Product");
                    await _unitOfWork.ShoppingCart.UpdateAsync(shoppingCart);

                    _response.Result = _mapper.Map<ShoppingCartDTO>(shoppingCart);
                    _response.StatusCode = HttpStatusCode.OK;
                    return Ok(_response);
                }
            }
        }
        [HttpDelete]
        public async Task<ActionResult<APIResponse>> DeleteShoppingCartItem(string userId, int shoppingCartItemId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Invalid User ID");
                return BadRequest(_response);
            }
            ShoppingCart shoppingCart = await _unitOfWork.ShoppingCart.GetAsync(u => u.UserId == userId);
            if (shoppingCart == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("ShoppingCart does not exist");
                return BadRequest(_response);
            }
            ShoppingCartItem item = await _unitOfWork.ShoppingCartItem.GetAsync(u => u.ShoppingCartId == shoppingCart.Id && u.Id == shoppingCartItemId);
            if (item == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("item does not exist");
                return BadRequest(_response);
            }
            else
            {
                await _unitOfWork.ShoppingCartItem.RemoveAsync(item);
                //update lai ShoppingCart
                shoppingCart.ShoppingCartItems = await _unitOfWork.ShoppingCartItem.GetAllAsync(u => u.ShoppingCartId == shoppingCart.Id, includeProperties: "Product");
                await _unitOfWork.ShoppingCart.UpdateAsync(shoppingCart);

                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = _mapper.Map<ShoppingCartDTO>(shoppingCart);
                return Ok(_response);
            }
        }
    }
}
