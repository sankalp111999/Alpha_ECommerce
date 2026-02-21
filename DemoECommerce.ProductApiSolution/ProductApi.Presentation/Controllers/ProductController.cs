using eCommerce.SharedLibrary.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Application.DTOs;
using ProductApi.Application.DTOs.Conversions;
using ProductApi.Application.Interfaces;

namespace ProductApi.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController(IProduct productInterface) : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]       //open to general public, no authentication required
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProduct()
        {
            var products = await productInterface.GetAllAsync();
            if(!products.Any())
            {
                return NotFound("No products detected in the database");
            }
            var (_, list) = ProductConversion.FromEntity(null!, products);        //Result: sirf list variable mein store hoga, pehla value garbage!
            return list!.Any() ? Ok(list) : NotFound("No products detected in the database");
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]    //open to general public, no authentication required
        public async Task<ActionResult<ProductDTO>> GetProductByID(int id)
        {
            var product = await productInterface.FindByIdAsync(id);
            if (product is null)
            {
                return NotFound("product requested not found");
            }
            var (_product, _) = ProductConversion.FromEntity(product, null!);   //Result: sirf _product variable mein store hoga, dusra value garbage! 
            return _product is not null ? Ok(_product) : NotFound("product requested not found");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Response>> AddProduct(ProductDTO product)
        {
            ///check if model is valid
            if(!ModelState.IsValid) 
                return BadRequest(ModelState);

            //Convert to Entity
            var _product = ProductConversion.ToEntity(product);
            var response = await productInterface.CreateAsync(_product);
            return response.Flag is true ? Ok(response) : BadRequest(response);

        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Response>> UpdateProduct(ProductDTO product)
        {
            //check if model is valid
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //Convert to entity
            var _product = ProductConversion.ToEntity(product);
            var response = await productInterface.UpdateAsync(_product);
            return response.Flag is true ? Ok(response) : BadRequest(response);
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Response>> DeleteProduct(ProductDTO product)
        {
            //convert to entity
            var _product = ProductConversion.ToEntity(product);
            //delete
            var response = await productInterface.DeleteAsync(_product);
            return response.Flag is true ? Ok(response) : BadRequest(response);
        }
    }
}
