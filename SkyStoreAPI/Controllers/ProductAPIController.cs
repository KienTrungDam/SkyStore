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
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public ProductAPIController(IUnitOfWork unitOfWork, IMapper mapper, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _response = new APIResponse();
        _webHostEnvironment = webHostEnvironment;
        }
        [HttpGet]
        //[Authorize]
        public async Task<ActionResult<APIResponse>> GetProducts()
        {
            IEnumerable<Product> products = await _unitOfWork.Product.GetAllAsync(includeProperties: "Category");
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = _mapper.Map<IEnumerable<ProductDTO>>(products);
            return Ok(_response);
        }
        [HttpGet("{id:int}", Name = "GetProduct")]
        //[Authorize(Roles = SD.Role_Customer)]
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
        //[Authorize(Roles = SD.Role_Admin)]
        public async Task<ActionResult<APIResponse>> CreateProduct([FromForm] ProductCreateDTO productCreateDTO)
        {
            try
            {
                if (ModelState.IsValid)
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
                    if (productCreateDTO.ImageUrl == null || productCreateDTO.ImageUrl.Length == 0)
                    {
                        _response.StatusCode = HttpStatusCode.NotFound;
                        _response.IsSuccess = false;
                        _response.ErrorMessages.Add("Image is required");
                        return BadRequest(_response);
                    }
                    //Image
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(productCreateDTO.ImageUrl.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\Product\");

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        productCreateDTO.ImageUrl.CopyTo(fileStream);
                    }
                    Product product = _mapper.Map<Product>(productCreateDTO);
                    product.ImageUrl = @"\images\Product\" + fileName;


                    //var product = _mapper.Map<Product>(productCreateDTO);
                    await _unitOfWork.Product.CreateAsync(product);
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.Result = _mapper.Map<ProductDTO>(product);

                    return CreatedAtRoute("GetProduct", new { id = product.Id }, _response);
                }
                else
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add("Menu Item is not valid");
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add(ex.Message);
            }
            return BadRequest(_response);
        }
        [HttpPut("{id:int}", Name = "UpdateProduct")]
        //[Authorize(Roles = SD.Role_Admin)]
        public async Task<ActionResult<APIResponse>> UpdateProduct(int id, [FromForm] ProductUpdateDTO productUpdateDTO)
        {
            try
            {
                if (id == 0 || productUpdateDTO == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Not Found");
                    return BadRequest();
                }
                Product product = await _unitOfWork.Product.GetAsync(u => u.Id == id);
                if (productUpdateDTO.ImageUrl != null && productUpdateDTO.ImageUrl.Length > 0)
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    var oldImagePath = Path.Combine(wwwRootPath, product.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                    //set filename
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(productUpdateDTO.ImageUrl.FileName);
                    //path
                    string productPath = Path.Combine(wwwRootPath, @"images\Product\");
                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        productUpdateDTO.ImageUrl.CopyTo(fileStream);
                    }
                    product.ImageUrl = @"\images\Product\" + fileName;
                }
                product.Price = productUpdateDTO.Price;
                product.Name = productUpdateDTO.Name;
                product.CategoryId = productUpdateDTO.CategoryId;
                product.Description = productUpdateDTO.Description;


                //Product productupdate = _mapper.Map<Product>(productUpdateDTO);
                await _unitOfWork.Product.UpdateAsync(product);
                _response.StatusCode = HttpStatusCode.NoContent;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.Message);
                _response.StatusCode = HttpStatusCode.BadRequest;
            }
            return BadRequest(_response);
        }
        [HttpDelete("{id:int}", Name = "DeleteProduct")]
        //[Authorize(Roles = SD.Role_Admin)]
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
            string wwwRootPath = _webHostEnvironment.WebRootPath;

            var oldImagePath = Path.Combine(wwwRootPath, product.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
            await _unitOfWork.Product.RemoveAsync(product);
            _response.StatusCode = HttpStatusCode.NoContent;
            return Ok(_response);
        }
    }
}
