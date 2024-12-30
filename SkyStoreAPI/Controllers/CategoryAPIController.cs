using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SkyStoreAPI.Models;
using SkyStoreAPI.Models.DTO;
using SkyStoreAPI.Repository.IRepository;
using System.Net;

namespace SkyStoreAPI.Controllers
{
    [Route("api/CategoryAPI")]
    [ApiController]
    public class CategoryAPIController : ControllerBase
    {
        protected APIResponse _response;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        
        public CategoryAPIController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _response = new APIResponse();
        }
        [HttpGet]
        public async Task<ActionResult> GetCategories()
        {
            IEnumerable<Category> categories = await _unitOfWork.Category.GetAllAsync();
            _response.StatusCode =  HttpStatusCode.OK;
            _response.Result = _mapper.Map<IEnumerable<CategoryDTO>>(categories);
            return Ok(_response);  
        }
        [HttpGet("{id:int}", Name = "GetCategory")]
        public async Task<ActionResult> GetCategory(int id)
        {
            if(id == 0)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;    
                _response.ErrorMessages.Add("Invalid Category ID");
                return BadRequest(_response);
            }
            Category category = await _unitOfWork.Category.GetAsync(u => u.Id == id);
            if(category == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Not Found");
                return NotFound(_response);
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = _mapper.Map<CategoryDTO>(category);
            return Ok(_response);
        }
        [HttpPost]
        public async Task<ActionResult> CreateCategory([FromForm] CategoryCreateDTO categoryCreateDTO)
        {
            var temp = await _unitOfWork.Category.GetAsync(u => u.Name == categoryCreateDTO.Name);
            if(temp != null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Name is already exist");
                return BadRequest(_response);
            }
            if(categoryCreateDTO == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Not Found");
                return BadRequest(_response);
            }
            var category = _mapper.Map<Category>(categoryCreateDTO);
            await _unitOfWork.Category.CreateAsync(category);
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = _mapper.Map<CategoryDTO>(category);

            return CreatedAtRoute("GetCategory", new { id = category.Id }, _response);
        }
        [HttpPut("{id:int}", Name = "UpdateCategory")]
        public async Task<ActionResult> UpdateCategory(int id, [FromForm] CategoryUpdateDTO categoryUpdateDTO)
        {
            if(id == 0 || categoryUpdateDTO == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Not Found");
                return BadRequest();
            }
            Category category = _mapper.Map<Category>(categoryUpdateDTO);
            await _unitOfWork.Category.UpdateAsync(category);
            _response.StatusCode = HttpStatusCode.NoContent;
            return Ok(_response);
        }
        [HttpDelete("{id:int}", Name = "DeleteCategory")]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            if (id == 0)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Id = 0 Ivalid");
                return BadRequest();
            }
            Category category = await _unitOfWork.Category.GetAsync(u => u.Id == id);
            if (category == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Not Found");
                return NotFound();
            }
            await _unitOfWork.Category.RemoveAsync(category);
            _response.StatusCode = HttpStatusCode.NoContent;
            return Ok(_response);
        }
    }
}
