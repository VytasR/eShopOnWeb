using System.Collections.Generic;

namespace Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;

public class OrderReservationRequest
{
    public List<OrderItemReservation> Items { get; set; }
}
