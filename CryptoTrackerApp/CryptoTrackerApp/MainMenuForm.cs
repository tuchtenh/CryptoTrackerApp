using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
using System.Timers;
using System.Windows.Forms;
using static CryptoTrackerApp.MainMenuForm;
using static CryptoTrackerApp.Program;

namespace CryptoTrackerApp
{
    public partial class MainMenuForm : Form
    {
        private List<CryptoCurrency> favouriteCurrenciesList = new List<CryptoCurrency>();
        private BindingList<CryptoPrice> currencyPriceList = new BindingList<CryptoPrice>();
        private System.Timers.Timer _timer;
        private int updateInterval = 120 * 1000;

        public MainMenuForm()
        {
            InitializeComponent();
            this.FormClosing += new FormClosingEventHandler(MainMenuForm_FormClosing);
            favouriteCurrenciesList = LoadFavourites();
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.DataSource = currencyPriceList;
            LoadSettings();
        }
        private void MainMenuForm_Load(object sender, EventArgs e)
        {
            if (favouriteCurrenciesList.Count > 0)
            {
                UpdateCryptoPriceAPI();
            }
            SetupTimer();
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            UpdateCryptoPriceAPI();
            _timer.Stop();
            _timer.Start();
        }

        private async void UpdateCryptoPriceAPI()
        {
            currencyPriceList.Clear();

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
            dataGridView1.Refresh();
        }

        private async Task<Dictionary<string, PriceAPIResponse>> FetchCryptoPriceAPI()
        {
            string ids = string.Join("%2C", favouriteCurrenciesList.Select(c => c.Id));
            string currency = "usd";
            string url = $"simple/price?ids={ids}&vs_currencies={currency}&include_24hr_change=true&precision=8";
            try
            {
                var response = await HttpClientInstance.Client.GetStringAsync(url);
                var prices = JsonConvert.DeserializeObject<Dictionary<string, PriceAPIResponse>>(response);
                return prices;
            }
            catch (HttpRequestException ex)
            {
                if (ex.Message.Contains("429"))
                {
                    MessageBox.Show("Too many request. Try refreshing later");
                    Console.WriteLine("\nException Caught!\nMessage :{0} ", ex.Message);
                    return new Dictionary<string, PriceAPIResponse>();
                }
                else
                {
                    Console.WriteLine("\nException Caught!\nMessage :{0} ", ex.Message);
                    return new Dictionary<string, PriceAPIResponse>();
                }
            }
        }

        private void FavouritesButton_Click(object sender, EventArgs e)
        {
            _timer.Stop();
            FavouritesForm favouritesForm = new FavouritesForm();
            favouritesForm.FormClosing += new FormClosingEventHandler(this.FavouritesForm_FormClosing);
            favouritesForm.ShowDialog();
        }
        private void FavouritesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            favouriteCurrenciesList = LoadFavourites();
            UpdateCryptoPriceAPI();
            LoadSettings();
            _timer.Interval = updateInterval * 1000;
            _timer.Start();
        }

        private class PriceAPIResponse
        {
            [JsonProperty("usd")]
            public double? Price { get; set; }

            [JsonProperty("usd_24h_change")]
            public double? Change { get; set; }
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

        private void ChartButton_Click(object sender, EventArgs e)
        {
            _timer.Stop();
            ChartForm chartForm = new ChartForm();
            chartForm.SetSelectedRow(dataGridView1.SelectedRows[0]);
            chartForm.ShowDialog();
        }
        private void ChartButton_FormClosing(object sender, EventArgs e)
        {
            _timer.Start();
        }
        private void MainMenuForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            var confirmResult = MessageBox.Show("Are you sure you want to close the application?", "Close", MessageBoxButtons.YesNo);
            _ = (confirmResult == DialogResult.Yes) ? (e.Cancel = false) : (e.Cancel = true);
        }
        private void SetupTimer()
        {
            _timer = new System.Timers.Timer(updateInterval*1000);
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }
        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            if (dataGridView1.InvokeRequired)
            {
                dataGridView1.Invoke(new MethodInvoker(delegate
                {
                    UpdateCryptoPriceAPI();
                }));
            }
            else
            {
                UpdateCryptoPriceAPI();
            }
        }
        private void LoadSettings()
        {
            if (File.Exists("advancedSettings.json"))
            {
                try
                {
                    using (StreamReader r = new StreamReader("advancedSettings.json"))
                    {
                        string json = File.ReadAllText("advancedSettings.json");
                        List<object> loadedSettings = JsonConvert.DeserializeObject<List<object>>(json);
                        updateInterval = Convert.ToInt32(loadedSettings[0]);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error reading or deserializing the file: " + ex.Message);
                }

            }
            else
            {
                updateInterval = 120;
            }
        }
    }

}
