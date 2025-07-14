using System.Globalization;
using System.Text.Json;

namespace TransactionAnalysis;

public class Solution
{
    private static readonly Lock _lockObject = new();
    private static readonly HttpClient _httpClient = new();
    private static readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    public static async Task<List<string>> TransferAmount(string name, string city)
    {
        string baseUrl = "https://jsonmock.hackerrank.com/api/transactions";
        double maxCredit = 0;
        double maxDebit = 0;

        int totalPage = GetApiResponse(baseUrl).Result?.TotalPages ?? 1;

        var tasks = Enumerable.Range(1, totalPage + 1).Select(async page =>
        {
            ApiResponse apiResponse = await GetApiResponse(baseUrl, page);

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

        await Task.WhenAll(tasks);

        return
        [
            string.Format(CultureInfo.InvariantCulture, "${0:N2}", maxCredit),
            string.Format(CultureInfo.InvariantCulture, "${0:N2}", maxDebit)
        ];
    }

    private static async Task<ApiResponse> GetApiResponse(string baseUrl, int page = 1)
    {
        string url = $"{baseUrl}?page={page}";
        // Schedule the HTTP request on the thread pool
        Task<HttpResponseMessage> response = _httpClient.GetAsync(url);
        // Do other work that doesn't depend on the HTTP response
        DoOtherWork();
        // Yield the control to the caller and wait for the response
        HttpResponseMessage message = await response;
        if (!message.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Request to {url} failed with status code {message.StatusCode}");
        }
        string jsonString = await message.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(jsonString))
        {
            throw new Exception("Empty JSON response.");
        }
        ApiResponse? apiResponse;
        try
        {
            apiResponse = JsonSerializer.Deserialize<ApiResponse>(jsonString, _options);
        }
        catch (JsonException ex)
        {
            throw new Exception("Invalid JSON format.", ex);
        }

        if (apiResponse == null || apiResponse.Data == null)
        {
            throw new Exception("Missing required data in JSON response.");            
        }
        return apiResponse;
    }

    public static void DoOtherWork()
    {
        // Simulate doing other work that doesn't depend on the response
        // This could be logging, updating UI, etc.
        Console.WriteLine("Doing other work while waiting for HTTP response...");
    }

    public static async Task Main(string[] args)
    {
        var result = await TransferAmount("Bob Martin", "Bourg");
        Console.WriteLine("Max Credit: " + result[0]);  // $3,717.84
        Console.WriteLine("Max Debit: " + result[1]);  // $3,568.55
    }
}
