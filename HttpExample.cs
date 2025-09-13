using System.Net;
using System.Text.Json.Nodes;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace My.Functions;

public class HttpExample
{
    private readonly ILogger<HttpExample> _logger;

    public HttpExample(ILogger<HttpExample> logger)
    {
        _logger = logger;
    }

    [Function("HttpExample")]
    public async Task<MultiResponse> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
        FunctionContext executionContext
    )
    {
        var logger = executionContext.GetLogger("HttpExample");
        logger.LogInformation("C# HTTP trigger function processed a request.");

        string? readBody = await req.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(readBody))
        {
            throw new Exception("Ther's nothing to read from bdoy");
        }

        JsonNode? data = JsonNode.Parse(readBody);
        if (data is null)
        {
            throw new Exception($"There's no data to read.");
        }

        string name = (string)data["name"]!;
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new Exception("Name cannot be empty.");
        }

        var message = $"Hello {name}";

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        await response.WriteStringAsync(message);

        return new MultiResponse()
        {
            Form = new VisitorForm { id = Guid.NewGuid().ToString(), Name = name },
            HttpResponse = response,
        };
    }
}

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

    [HttpResult]
    public HttpResponseData? HttpResponse { get; set; }
}

public class VisitorForm
{
    public string id { get; set; } = null!;
    public string Name { get; set; } = null!;
}
