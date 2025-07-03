using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;

namespace OrderItemsReserver.Models;

public class CosmosOrderDocument
{
    public string? id { get; set; }

    public int? orderId { get; set; }

    public Address? shippingAddress { get; set; }

    public List<OrderItemReservation>? items { get; set; }

    public decimal? finalPrice { get; set; }

    public string? orderStatus { get; set; }
}
