using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace SteamPrices
{
    class Program
    {
        private static readonly CultureInfo currentCulture = CultureInfo.CurrentCulture;
        private static readonly RegionInfo regionInfo = new RegionInfo(currentCulture.Name);

        private static readonly string val = "https://cdn.jsdelivr.net/gh/fawazahmed0/currency-api@1/latest/currencies/usd/";
        private static readonly string url = $"https://steamcommunity.com/market/priceoverview/?&currency=1&appid=730&market_hash_name=";

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            using (StreamReader reader = new StreamReader("Items.txt"))
            {
                while (!reader.EndOfStream)
                {
                    string item = reader.ReadLine();
                    double itemPrice = GetPrice(item);
                    double converted = Math.Round(itemPrice * GetConversion(), 2);

                    Console.WriteLine($"{item}: {itemPrice}$ ({converted + regionInfo.CurrencySymbol})");
                    Thread.Sleep(3000);
                }
            }

            Console.ReadKey();
        }

        private static double GetPrice(string name)
        {
            using (HttpClient client = new HttpClient())
            {
                string response = client.GetStringAsync(url + name).Result;
                JObject json = JObject.Parse(response);

                JToken lowestToken = json.SelectToken("lowest_price");
                JToken medianToken = json.SelectToken("median_price");

                if (lowestToken == null && medianToken == null)
                {
                    return 0.0;
                }
                else if (lowestToken == null)
                {
                    return double.Parse(medianToken.ToString().TrimStart('$'), CultureInfo.InvariantCulture);
                }
                else if (medianToken == null)
                {
                    return double.Parse(lowestToken.ToString().TrimStart('$'), CultureInfo.InvariantCulture);
                }
                else
                {
                    string lowest_price_string = lowestToken.ToString().TrimStart('$');
                    string median_price_string = medianToken.ToString().TrimStart('$');

                    double lowest_price = double.Parse(lowest_price_string, CultureInfo.InvariantCulture);
                    double median_price = double.Parse(median_price_string, CultureInfo.InvariantCulture);

                    double price = (lowest_price + median_price) / 2;

                    int decimalIndex = lowest_price_string.IndexOf('.');
                    int decimalPlaces = decimalIndex == -1 ? 0 : lowest_price_string.Substring(decimalIndex + 1).Length;

                    double rounded_price = Math.Round(price, decimalPlaces);

                    return rounded_price;
                }
            }
        }

        private static double GetConversion()
        {
            string currency = regionInfo.ISOCurrencySymbol.ToLower();

            using (HttpClient client = new HttpClient())
            {
                string response = client.GetStringAsync(val + currency + ".json").Result;
                JObject json = JObject.Parse(response);
                double rate = (double)json[currency];

                return rate;
            }
        }
    }
}