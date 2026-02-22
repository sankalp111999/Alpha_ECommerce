using FakeItEasy;                          // | FakeItEasy = Mocking library (creates fake objects for testing)
using FluentAssertions;                   //  | Should() = Easy readable assertions
using Microsoft.AspNetCore.Http;         //   | HTTP Status codes (200, 404)
using Microsoft.AspNetCore.Mvc;         //    | Controller return types (OkObjectResult)
using ProductApi.Application.DTOs;      
using ProductApi.Application.Interfaces;
using ProductApi.Domain.Entities;
using ProductApi.Presentation.Controllers;
using System;

/*----------Test_Method------------
ARRANGE → Test data + Mock setup
ACT     → Controller method call  
ASSERT  → Result verify (Status + Data)
*/

namespace UnitTest.ProductApi.Controllers
{
    public class ProductControllerTest
    {
        private readonly IProduct productInterface;                 //| Fake IProduct
        private readonly ProductController productsController;     // | Real Controller

        public ProductControllerTest()
        {
            //Set up the dependencies for the ProductController
            productInterface = A.Fake<IProduct>();          //| Fake IProduct       // A = FakeItEasy का main class

            //set up system under test (SUT)
            productsController = new ProductController(productInterface);    //| Real Controller with fake interface
        }

        //GET ALL PRODUCTS
        [Fact]                    // xUnit attribute = "Ye एक test method है"
        public async Task GetProduct_WhenProductExists_ReturnOkResponseWithProducts()
        {
            //ARRANGE    ------------>     (Setup/Test Data तैयार)
            var products = new List<Product>()
            {
                new(){Id = 1, Name ="Product 1", Quantity=10, Price = 100.70m },
                new(){Id = 2, Name ="Product 2", Quantity=120, Price = 131.74m },
            };

            //set up the fake response for GetAllAsync method
            A.CallTo(() => productInterface.GetAllAsync()).Returns(products);   //जब GetAllAsync() call हो → हमारा fake data return करो!

            //Act  ------------>     Controller का real method execute करो!
            var result = await productsController.GetProduct();

            //Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);

            var returnedProducts = okResult.Value as IEnumerable<ProductDTO>;
            returnedProducts.Should().NotBeNull(); 
            returnedProducts.Should().HaveCount(2);       // 2 products?
            returnedProducts!.First().Id.Should().Be(1);       // First ID = 1?
            returnedProducts!.Last().Id.Should().Be(2);
        }

        [Fact]
        public async Task GetProduct_WhenNoProductsExist_ReturnNotFoundResponse()
        {
            //ARRANGE
            var products = new List<Product>(); //empty list
            //set up the fake response for GetAllAsync method
            A.CallTo(() => productInterface.GetAllAsync()).Returns(products);
            //Act
            var result = await productsController.GetProduct();
            //Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            var message = notFoundResult.Value as string;
            message.Should().Be("No products detected in the database");
        }

        //Create Product
        [Fact]
        public async Task CreateProduct_WhenModelSatateIsInvalid_ReturnBadRequest()
        {
            //Arrange
            var productDTO = new ProductDTO(1, "Product 1", 10, 100.70m);
            productsController.ModelState.AddModelError("Name", "Required");   //ModelState को Invalid बनाना है, इसलिए एक error add कर रहे हैं0
            
            //Act
            var result = await productsController.AddProduct(productDTO);
            
            //Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }
    }
}
