using System.Text.Json;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderItemsReserver.Mappers;

namespace OrderItemsReserver.Functions;

public class ReserveItems
{
    private readonly ILogger<ReserveItems> _logger;
    private readonly Container _container;

    public ReserveItems(ILogger<ReserveItems> logger, IConfiguration configuration)
    {
        _logger = logger;

        var connectionString = configuration["CosmosDbConnectionString"];
        var cosmosClient = new CosmosClient(connectionString);
        _container = cosmosClient.GetContainer("DeliveryDB", "Orders");
    }

    [Function("ReserveItems")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        _logger.LogInformation("ReserveItems processing a request.");

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrEmpty(requestBody))
            {
                return new BadRequestObjectResult("Request body is empty");
            }

            var reservationRequest = JsonSerializer.Deserialize<OrderReservationRequest>(requestBody);

            if (reservationRequest == null || reservationRequest.Items == null || reservationRequest.Items.Count == 0)
            {
                return new BadRequestObjectResult("Invalid reservation request format or no items to reserve");
            }

            var orderDocument = CosmosOrderDocumentMapper.ToCosmosDocument(reservationRequest);

            var createResponse = await _container.CreateItemAsync(orderDocument, new PartitionKey(orderDocument.orderStatus));

            _logger.LogInformation("Created Order reservation for Order '{OrderId}'", createResponse.Resource.orderId);

            return new OkObjectResult($"Items for Order '{reservationRequest.OrderId}' reserved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in OrderItemsReserver: {ErrorMessage}", ex.Message);
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
