using System.Net;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace My.Functions;

/// <summary>
/// Azure Function that demonstrates handling of an HTTP Post
/// and writes to CosmosDB
/// </summary>
public class HttpExample
{
    /// <summary>
    /// Logger instance.
    /// </summary>
    private readonly ILogger<HttpExample> _logger;

    /// <summary>
    /// A constructor with DI of logger.
    /// </summary>
    /// <param name="logger">Injected logger</param>
    public HttpExample(ILogger<HttpExample> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles an HTTP Post req.
    /// Reads the req body.
    /// Parses JSON to string.
    /// Extracts name
    /// Returns a greeting message and store the data in CosmosDB
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [Function("HttpExample")]
    public async Task<MultiResponse> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req
    )
    {
        _logger.LogInformation("The function app is running.");

        // Reads the req body
        string? readBody = await req.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(readBody))
        {
            _logger.LogWarning("Body was empty.");

            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteAsJsonAsync("Body is required.");

            return new MultiResponse { HttpResponse = badResponse, Form = null };
        }

        // Parse JSON
        JsonNode? data = JsonNode.Parse(readBody);
        if (data is null)
        {
            _logger.LogWarning("There's no data to read");

            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteAsJsonAsync("There's not data to read.");

            return new MultiResponse { HttpResponse = badResponse, Form = null };
        }

        // Extract "name" property
        string name = (string)data["name"]!;
        if (string.IsNullOrWhiteSpace(name))
        {
            _logger.LogWarning("Name was empty here.");

            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteAsJsonAsync("Name is required.");

            return new MultiResponse { HttpResponse = badResponse, Form = null };
        }

        // Simple response message
        var message = $"Hello {name}";

        // Creates a HTTP response
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        await response.WriteStringAsync(message);

        // Return both HTTP response and CosmosDB entity
        return new MultiResponse()
        {
            Form = new VisitorForm { id = Guid.NewGuid().ToString(), Name = name },
            HttpResponse = response,
        };
    }
}

/// <summary>
/// Combined response with both HTTP result
/// and a CosmosDB output entity
/// </summary>
public class MultiResponse
{
    [CosmosDBOutput(
        "my-database",
        "my-container",
        Connection = "CosmosDbConnectionString",
        PartitionKey = "/id",
        CreateIfNotExists = true
    )]
    public VisitorForm? Form { get; set; }

    /// <summary>
    /// The HTTP response that returns to the client.
    /// </summary>
    [HttpResult]
    public HttpResponseData? HttpResponse { get; set; }
}

/// <summary>
/// Represents a visitor stored in CosmosDB
/// </summary>
public class VisitorForm
{
    /// <summary>
    /// Unique identifier for the visitor
    /// This is also used as a partition key
    /// </summary>
    public string id { get; set; } = null!;

    /// <summary>
    /// The name of the visitor.
    /// </summary>
    public string Name { get; set; } = null!;
}
