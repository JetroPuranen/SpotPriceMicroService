using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

class Program
{
    private static int currentId = 1;

    static async Task Main()
    {
        while (true)
        {
            var prices = await GetElectricityPrices();

            
            AssignIds(prices);

           
            await SavePricesToDatabase(prices);

            await Task.Delay(TimeSpan.FromHours(1));
        }
    }

    static void AssignIds(List<ElectricityPrice> prices)
    {
        foreach (var price in prices)
        {
            price.Id = currentId++;
        }
    }

    static async Task<List<ElectricityPrice>> GetElectricityPrices()
    {
        using (var httpClient = new HttpClient())
        {
            var apiUrl = "https://api.porssisahko.net/v1/latest-prices.json";

            try
            {
                var response = await httpClient.GetStringAsync(apiUrl);
                var pricesResponse = JsonConvert.DeserializeObject<PricesResponse>(response);

                if (pricesResponse != null && pricesResponse.Prices != null)
                {
                    return pricesResponse.Prices;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Virhe sähkön hintojen haussa: {ex.Message}");
            }

            return new List<ElectricityPrice>();
        }
    }

    static async Task SavePricesToDatabase(List<ElectricityPrice> prices)
    {
       
        string jsonPrices = JsonConvert.SerializeObject(prices);

        
        var apiUrl = ""; //Update with the actual port and endpoint

        //HttpClient
        using (var httpClient = new HttpClient())
        {
            
            var content = new StringContent(jsonPrices, Encoding.UTF8, "application/json");

            try
            {
                //HTTP POST-request
                var response = await httpClient.PostAsync(apiUrl, content);

                
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Hinnat lähetetty onnistuneesti.");
                }
                else
                {
                    Console.WriteLine($"Virhe hinnatietojen lähettämisessä: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Virhe hinnatietojen lähettämisessä: {ex.Message}");
            }
        }
    }
}

class PricesResponse
{
    [JsonProperty("prices")]
    public List<ElectricityPrice> Prices { get; set; }
}

class ElectricityPrice
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("price")]
    public decimal Price { get; set; }

    [JsonProperty("startDate")]
    public DateTime StartDate { get; set; }

    [JsonProperty("endDate")]
    public DateTime EndDate { get; set; }
}
