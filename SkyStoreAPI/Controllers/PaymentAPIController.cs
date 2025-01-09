using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkyStoreAPI.Data;
using SkyStoreAPI.Models;
using SkyStoreAPI.Repository.IRepository;
using SkyStoreAPI.Untility;
using Stripe;
using System.Net;

namespace SkyStoreAPI.Controllers
{
    [Route("api/PaymentAPI")]
    [ApiController]
    [Authorize]
    public class PaymentAPIController : Controller
    {
        protected APIResponse _response;
        private string _secretKey;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        public PaymentAPIController(IConfiguration configuration, ApplicationDbContext db, IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _secretKey = configuration.GetValue<string>("Stripe:SecretKey");
            _unitOfWork = unitOfWork;
            _db = db;
            _userManager = userManager;
            _response = new APIResponse();
        }
        [HttpPost]
        public async Task<ActionResult<APIResponse>> MakePayment(int orderId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Invalid User ID");
                return BadRequest(_response);
            }
            OrderHeader orderHeader = await _unitOfWork.OrderHeader.GetAsync(u => u.Id == orderId && u.ApplicationUserId == user.Id);

            //ShoppingCart shoppingCart = _db.ShoppingCarts.Include(u => u.CartItems).ThenInclude(u => u.MenuItem).FirstOrDefault(u => u.ApplicationUserId == userId);
            if (orderHeader == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }
            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Order is already paid");
                return BadRequest(_response);
            }
            #region Create Payment Intent
            StripeConfiguration.ApiKey = _secretKey;
            //shoppingCart.CartTotal = shoppingCart.CartItems.Sum(u => u.Quantity * u.MenuItem.Price);
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(orderHeader.OrderTotal * 100),
                Currency = "usd",
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
            };
            var service = new PaymentIntentService();
            PaymentIntent response = service.Create(options);
            orderHeader.StripePaymentIntentId = response.Id;
            orderHeader.PaymentStatus = SD.PaymentStatusApproved;
            #endregion
            await _unitOfWork.OrderHeader.UpdateAsync(orderHeader);
            _response.StatusCode = HttpStatusCode.NoContent;
            return Ok(_response);
        }
    }
}
