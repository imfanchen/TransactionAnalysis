using System.Globalization;
using System.Text.Json;

namespace TransactionAnalysis;

public class Solution
{
    public static async Task<List<string>> TransferAmount(string userName, string city)
    {
        string baseUrl = "https://jsonmock.hackerrank.com/api/transactions";
        using HttpClient client = new HttpClient();
        int page = 1;
        double maxCredit = 0;
        double maxDebit = 0;

        while (true)
        {
            string url = $"{baseUrl}?page={page}";
            HttpResponseMessage response = await client.GetAsync(url);
            string json = await response.Content.ReadAsStringAsync();
            JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true
            };
            ApiResponse? apiResponse = JsonSerializer.Deserialize<ApiResponse>(json, options);

            foreach (Transaction txn in apiResponse?.Data ?? Enumerable.Empty<Transaction>())
            {
                if (txn.UserName == userName && txn.Location.City == city)
                {
                    double amount = double.Parse(txn.Amount.Replace("$", "").Replace(",", ""), CultureInfo.InvariantCulture);

                    if (txn.TxnType == "credit")
                    {
                        maxCredit = Math.Max(maxCredit, amount);
                    }
                    else if (txn.TxnType == "debit")
                    {
                        maxDebit = Math.Max(maxDebit, amount);
                    }
                }
            }

            if (page >= apiResponse?.TotalPages)
                break;

            page++;
        }

        return
        [
            string.Format(CultureInfo.InvariantCulture, "${0:N2}", maxCredit),
            string.Format(CultureInfo.InvariantCulture, "${0:N2}", maxDebit)
        ];
    }

    public static async Task Main(string[] args)
    {
        var result = await TransferAmount("Bob Martin", "Bourg");
        Console.WriteLine("Max Credit: " + result[0]);  // $3,717.84
        Console.WriteLine("Max Debit: " + result[1]);  // $3,568.55
    }
}
