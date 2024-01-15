using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static CryptoTrackerApp.Form1;
using static CryptoTrackerApp.Program;

namespace CryptoTrackerApp
{
    public partial class Form1 : Form
    {
        private List<CryptoCurrency> favouriteCurrenciesList = new List<CryptoCurrency>();
        private List<CryptoPrice> currencyPriceList = new List<CryptoPrice>();

        public Form1()
        {
            InitializeComponent();
            favouriteCurrenciesList = LoadFavourites();
            if(favouriteCurrenciesList.Count > 0)
            {
                UpdateCryptoPriceAPI();
            }
        }

        // Refresh
        private void button1_Click(object sender, EventArgs e)
        {
            UpdateCryptoPriceAPI();
        }

        private async void UpdateCryptoPriceAPI()
        {
            currencyPriceList = new List<CryptoPrice>();
            Dictionary<string, PriceAPIResponse> cryptoResponse = new Dictionary<string, PriceAPIResponse>();
            cryptoResponse = await FetchCryptoPriceAPI();

            foreach (var favCurrency in favouriteCurrenciesList)
            {
                CryptoPrice pricedCoin = new CryptoPrice
                {
                    Id = favCurrency.Id,
                    Name = favCurrency.Name
                };

                if (cryptoResponse.TryGetValue(favCurrency.Id, out var priceApiResponse))
                {
                    pricedCoin.Price = priceApiResponse.Price;
                    pricedCoin.PercentageChange24Hr = priceApiResponse.Change;
                }
                if (!currencyPriceList.Any(c => c.Id == pricedCoin.Id))
                {
                    currencyPriceList.Add(pricedCoin);
                }
                
            }
            dataGridView1.DataSource = currencyPriceList;
        }

        private async Task<Dictionary<string, PriceAPIResponse>> FetchCryptoPriceAPI()
        {
            string ids = string.Join(",", favouriteCurrenciesList.Select(c => c.Id));
            string currency = "usd";
            string url = $"simple/price?ids={ids}&vs_currencies={currency}&include_24hr_change=true&precision=2";
            try
            {
                var response = await HttpClientInstance.Client.GetStringAsync(url);
                var prices = JsonConvert.DeserializeObject<Dictionary<string, PriceAPIResponse>>(response);
                return prices;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", ex.Message);
                return new Dictionary<string, PriceAPIResponse>();
            }
        }

        private async Task APIStuff()
        {
            try
            {
                var responseBody = await HttpClientInstance.Client.GetStringAsync("simple/supported_vs_currencies");
                await Task.Delay(5000);
                List<string> currencies = JsonConvert.DeserializeObject<List<string>>(responseBody);
                foreach (var currency in currencies)
                {
                    Console.WriteLine(currency);
                }
                Console.WriteLine(responseBody);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", ex.Message);
            }
        }



        private void button2_Click(object sender, EventArgs e)
        {
            OptionsForm optionsForm = new OptionsForm();
            optionsForm.FormClosing += new FormClosingEventHandler(this.OptionsForm_FormClosing);
            optionsForm.ShowDialog();
        }
        private void OptionsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            favouriteCurrenciesList = LoadFavourites();
            UpdateCryptoPriceAPI();
        }


        public class CryptoCurrency
        {
            public string Id { get; set; }
            public string Symbol { get; set; }
            public string Name { get; set; }
        }

        public class PriceAPIResponse
        {
            [JsonProperty("usd")]
            public double? Price { get; set; }

            [JsonProperty("usd_24h_change")]
            public double? Change { get; set; }
        }

        private class CryptoPrice
        {

            public string Id { get; set; }
            public string Name { get; set; }
            public double? Price { get; set; }

            private double? change;
            public double? PercentageChange24Hr
            {
                get => change;
                set => change = value.HasValue ? Math.Round(value.Value, 2) : (double?)null;
            }
        }

        private List<CryptoCurrency> LoadFavourites()
        {
            List<CryptoCurrency> cryptos = new List<CryptoCurrency>();
            if (File.Exists("favouriteCurrenciesList.json"))
            {
                try
                {
                    using (StreamReader r = new StreamReader("favouriteCurrenciesList.json"))
                    {
                        string json = File.ReadAllText("favouriteCurrenciesList.json");
                        cryptos = JsonConvert.DeserializeObject<List<CryptoCurrency>>(json);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error reading or deserializing the file: " + ex.Message);
                }
            }
            return cryptos;
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }

    }
}
