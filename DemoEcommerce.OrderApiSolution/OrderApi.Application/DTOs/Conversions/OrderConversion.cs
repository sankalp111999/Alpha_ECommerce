using OrderApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderApi.Application.DTOs.Conversions
{
    public static class OrderConversion
    {
        public static Order ToEntity(OrderDTO order) => new Order()
        {
            id = order.Id,
            ClientId = order.ClientId,
            ProductId = order.ProductId,
            PurchaseQuantity = order.PurchaseQuantity,
            OrderDate = order.OrderDate
        };

        public static (OrderDTO?, IEnumerable<OrderDTO>?) FromEntity(Order? order, IEnumerable<Order>? orders)
        {
            //return single order
            if (order is not null || orders is null)
            {
                var singleOrder = new OrderDTO(
                    order!.id,
                    order.ProductId,
                    order.ClientId,
                    order.PurchaseQuantity,
                    order.OrderDate);

                return (singleOrder, null);
            }

            //return multiple orders
            if (orders is not null || order is null)
            {
                var _orders = orders!.Select(o =>
                    new OrderDTO(
                        o.id,
                        o.ProductId,
                        o.ClientId,
                        o.PurchaseQuantity,
                        o.OrderDate)).ToList();

                return (null, _orders);
            }

            // Ensure all code paths return a value
            return (null, null);
        }
    }
}
