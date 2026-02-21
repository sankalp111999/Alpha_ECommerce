using OrderApi.Application.DTOs;
using OrderApi.Application.DTOs.Conversions;
using OrderApi.Application.Interfaces;
using Polly.Registry;
using System.Net.Http.Json;

namespace OrderApi.Application.Services
{
    public class OrderService(IOrder orderInterface, HttpClient httpClient, ResiliencePipelineProvider<string> resiliencePipeline) : IOrderService
    {
        //GET PRODUCT
        public async Task<ProductDTO> GetProduct(int productId)
        {
            //Call Product API using HttpClient
            // Redirect this call to the API Gateway since product API is not response to outsiders.
            var getProduct = await httpClient.GetAsync($"/api/Product/{productId}");
            if (!getProduct.IsSuccessStatusCode)
            {
                return null!;
            }
            var product = await getProduct.Content.ReadFromJsonAsync<ProductDTO>();
            return product!;
        }

        //GET USER
        public async Task<AppUserDTO> GetUser(int userId)
        {
            var getUser = await httpClient.GetAsync($"/api/Authentication/{userId}");
            if (!getUser.IsSuccessStatusCode)
            {
                return null!;
            }
            var product = await getUser.Content.ReadFromJsonAsync<AppUserDTO>();
            return product!;
        }

        //GET Order Details by id
        public async Task<OrderDetailsDTO> GetOrderDetails(int orderId)
        {
            //Prepare order 
            var order = await orderInterface.FindByIdAsync(orderId);
            if (order is null || order!.id <= 0)
                return null!;

            //Get Retry Pipeline
            var retryPipeline = resiliencePipeline.GetPipeline("my-retry-pipeline");

            //prepare product
            var productDTO = await retryPipeline.ExecuteAsync(async token => await GetProduct(order.ProductId));

            //Prepare Client
            var appUserDTO = await retryPipeline.ExecuteAsync(async token => await GetUser(order.ClientId));

            //Populate order Details DTO
            return new OrderDetailsDTO(
                order.id,
                productDTO.Id,
                appUserDTO.Id,
                appUserDTO.Name,
                appUserDTO.Address,
                appUserDTO.Email,
                appUserDTO.PhoneNumber,
                productDTO.Name,
                order.PurchaseQuantity,
                productDTO.Price,
                productDTO.Quantity * order.PurchaseQuantity,
                order.OrderDate
                );
        }

        //Get Orders by Client Id
        public async Task<IEnumerable<OrderDTO>> GetOrdersByClientId(int clientId)
        {
            //Get all Client's Orders
            var orders = await orderInterface.GetOrdersAsync(o => o.ClientId == clientId);
            if (!orders.Any()) return null!;

            //Convert from entity to DTO
            var (_, _orders) = OrderConversion.FromEntity(null, orders);
            return _orders!;
        }
    }
}
