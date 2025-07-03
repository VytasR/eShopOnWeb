using System.Collections.Generic;

namespace Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;

public class OrderReservationRequest
{
    public int OrderId { get; set; }
    public Address ShippingAddress { get; set; }
    public List<OrderItemReservation> Items { get; set; }
    public decimal FinalPrice { get; set; }
}
