using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using OrderItemsReserver.Models;

namespace OrderItemsReserver.Mappers;

public static class CosmosOrderDocumentMapper
{
    public static CosmosOrderDocument ToCosmosDocument(this OrderReservationRequest request)
    {
        return new CosmosOrderDocument
        {
            id = Guid.NewGuid().ToString(),
            orderId = request.OrderId,
            shippingAddress = request.ShippingAddress,
            items = request.Items,
            finalPrice = request.FinalPrice,
            orderStatus = "New"
        };
    }
}
