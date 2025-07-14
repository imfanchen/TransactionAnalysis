using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

public class TransactionService : IHostedService
{
    private readonly ILogger _logger;
    private readonly HttpClient _client;
    private readonly string _name;
    private readonly string _city;
    private static readonly Lock _lockObject = new();
    private static readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };
    private const string BASE_URL = "https://jsonmock.hackerrank.com/api/transactions";

    public TransactionService(HttpClient client, ILogger<TransactionService> logger, IConfiguration config)
    {
        _client = client;
        _logger = logger;
        _name = config["name"] ?? "";
        _city = config["city"] ?? "";

        if (string.IsNullOrWhiteSpace(_name) || string.IsNullOrWhiteSpace(_city))
        {
            _logger.LogError("Name and City must be provided and cannot be empty or whitespace. Exiting program.");
            Environment.Exit(1);
        }
    }

    private async Task<List<string>> TransferAmount(string name, string city, CancellationToken cancellationToken)
    {
        double maxCredit = 0;
        double maxDebit = 0;

        int totalPage = (await GetApiResponse(1, cancellationToken))?.TotalPages ?? 1;

        var tasks = Enumerable.Range(1, totalPage + 1).Select(async page =>
        {
            ApiResponse apiResponse = await GetApiResponse(page, cancellationToken);

            foreach (Transaction txn in apiResponse.Data)
            {
                if (txn.UserName == name && txn.Location.City == city)
                {
                    double amount = double.Parse(txn.Amount.Replace("$", "").Replace(",", ""), CultureInfo.InvariantCulture);
                    lock (_lockObject)
                    {
                        if (txn.TxnType == "credit")
                        {
                            maxCredit = Math.Max(maxCredit, amount);
                        }
                        if (txn.TxnType == "debit")
                        {
                            maxDebit = Math.Max(maxDebit, amount);
                        }
                    }
                }
            }
        });

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (AggregateException ex)
        {
            foreach (var inner in ex.InnerExceptions)
            {
                _logger.LogError(inner, "Task failed.");
            }
        }

        return
        [
            string.Format(CultureInfo.InvariantCulture, "${0:N2}", maxCredit),
            string.Format(CultureInfo.InvariantCulture, "${0:N2}", maxDebit)
        ];
    }

    private async Task<ApiResponse> GetApiResponse(int page, CancellationToken cancellationToken)
    {
        string url = $"{BASE_URL}?page={page}";
        // Schedule the HTTP request on the thread pool
        Task<HttpResponseMessage> response = _client.GetAsync(url, cancellationToken);
        // Do other work that doesn't depend on the HTTP response
        DoOtherWork();
        // Yield the control to the caller until the response is ready
        HttpResponseMessage message = await response;

        if (message.IsSuccessStatusCode)
        {
            _logger.LogInformation("Response from {Url} with status code {StatusCode}", url, message.StatusCode);
        }
        else
        {
            throw new HttpRequestException($"Request to {url} failed with status code {message.StatusCode}");
        }
        string jsonString = await message.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(jsonString))
        {
            throw new HttpRequestException($"Request to {url} failed because of empty JSON response");
        }
        ApiResponse? apiResponse;
        try
        {
            apiResponse = JsonSerializer.Deserialize<ApiResponse>(jsonString, _options);
        }
        catch (JsonException ex)
        {
            throw new HttpRequestException($"Request to {url} failed because of invalid JSON format.", ex);
        }

        if (apiResponse == null || apiResponse.Data == null)
        {
            throw new HttpRequestException($"Request to {url} failed because of missing required data in JSON response.");
        }
        return apiResponse;
    }

    private void DoOtherWork()
    {
        // Simulate doing other work that doesn't depend on the HTTP response
        _logger.LogDebug("Doing other work while waiting for HTTP response...");
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("TransactionService is starting.");
        var result = await TransferAmount(_name, _city, cancellationToken);
        _logger.LogInformation("Max Credit: {MaxCredit}", result[0]);  // default: $3,717.84
        _logger.LogInformation("Max Debit: {MaxDebit}", result[1]);  // default: $3,568.55
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("TransactionService is stopping.");
        // Perform any necessary cleanup here
        await Task.CompletedTask;
    }
}
