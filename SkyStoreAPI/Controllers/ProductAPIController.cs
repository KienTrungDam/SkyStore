using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SkyStoreAPI.Models.DTO;
using SkyStoreAPI.Models;
using SkyStoreAPI.Repository.IRepository;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using SkyStoreAPI.Untility;

namespace SkyStoreAPI.Controllers
{
    [Route("api/ProductAPI")]
    [ApiController]
    public class ProductAPIController : ControllerBase
    {
        protected APIResponse _response;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public ProductAPIController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _response = new APIResponse();
        }
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<APIResponse>> GetProducts()
        {
            IEnumerable<Product> products = await _unitOfWork.Product.GetAllAsync(includeProperties: "Category");
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = _mapper.Map<IEnumerable<ProductDTO>>(products);
            return Ok(_response);
        }
        [HttpGet("{id:int}", Name = "GetProduct")]
        [Authorize(Roles = SD.Role_Customer)]
        public async Task<ActionResult<APIResponse>> GetProduct(int id)
        {
            if (id == 0)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Invalid Product ID");
                return BadRequest(_response);
            }
            Product product = await _unitOfWork.Product.GetAsync(u => u.Id == id);
            if (product == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Not Found");
                return NotFound(_response);
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = _mapper.Map<ProductDTO>(product);
            return Ok(_response);
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<ActionResult<APIResponse>> CreateProduct([FromForm] ProductCreateDTO productCreateDTO)
        {
            var temp = await _unitOfWork.Product.GetAsync(u => u.Name == productCreateDTO.Name);
            if (temp != null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Name is already exist");
                return BadRequest(_response);
            }
            if (productCreateDTO == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Not Found");
                return BadRequest(_response);
            }
            var product = _mapper.Map<Product>(productCreateDTO);
            await _unitOfWork.Product.CreateAsync(product);
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = _mapper.Map<ProductDTO>(product);

            return CreatedAtRoute("GetProduct", new { id = product.Id }, _response);
        }
        [HttpPut("{id:int}", Name = "UpdateProduct")]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<ActionResult<APIResponse>> UpdateProduct(int id, [FromForm] ProductUpdateDTO productUpdateDTO)
        {
            if (id == 0 || productUpdateDTO == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Not Found");
                return BadRequest();
            }
            Product product = _mapper.Map<Product>(productUpdateDTO);
            await _unitOfWork.Product.UpdateAsync(product);
            _response.StatusCode = HttpStatusCode.NoContent;
            return Ok(_response);
        }
        [HttpDelete("{id:int}", Name = "DeleteProduct")]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<ActionResult<APIResponse>> DeleteProduct(int id)
        {
            if (id == 0)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Id = 0 Ivalid");
                return BadRequest();
            }
            Product product = await _unitOfWork.Product.GetAsync(u => u.Id == id);
            if (product == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Not Found");
                return NotFound();
            }
            await _unitOfWork.Product.RemoveAsync(product);
            _response.StatusCode = HttpStatusCode.NoContent;
            return Ok(_response);
        }
    }
}
