using System.Text.Json;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.Extensions.Logging;

namespace OrderItemsReserver;

public class ReserveItems
{
    private readonly ILogger<ReserveItems> _logger;

    public ReserveItems(ILogger<ReserveItems> logger)
    {
        _logger = logger;
    }

    [Function("ReserveItems")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        _logger.LogInformation("ReserveItems processing a request.");

        try
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrEmpty(requestBody))
            {
                return new BadRequestObjectResult("Request body is empty");
            }

            var reservationRequest = JsonSerializer.Deserialize<OrderReservationRequest>(requestBody);

            if (reservationRequest == null || reservationRequest.Items == null || reservationRequest.Items.Count == 0)
            {
                return new BadRequestObjectResult("Invalid reservation request format or no items to reserve");
            }

            string? connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Azure Storage connection string is not configured. Please add the 'AzureWebJobsStorage' setting to the application configuration.");
            }

            string containerName = Environment.GetEnvironmentVariable("ReservationsContainerName") ?? "order-reservations";
            string fileName = $"catalog_item_reservation_{DateTime.UtcNow:yyyyMMddHHmmss}.json";

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();
            var blobClient = containerClient.GetBlobClient(fileName);

            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(requestBody);
                writer.Flush();
                stream.Position = 0;

                await blobClient.UploadAsync(stream, true);
            }

            _logger.LogInformation("Reservation saved to blob: {FileName}", fileName);

            return new OkObjectResult($"Order items reserved successfully. Reservation ID: {fileName}");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in OrderItemsReserver: {ErrorMessage}", ex.Message);
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
