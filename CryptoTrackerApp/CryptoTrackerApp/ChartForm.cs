using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static CryptoTrackerApp.Form1;
using static CryptoTrackerApp.Program;

namespace CryptoTrackerApp
{
    public partial class ChartForm : Form
    {
        List<DateTime> xValues = new List<DateTime>();
        List<double> yValues = new List<double>();

        JArray pricesArray = new JArray();
        public ChartForm()
        {
            InitializeComponent();
            DrawChart();
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private async Task<JArray> FetchPriceFromAPIAsync()
        {
            //string ids = "bitcoin";//string.Join("%2C", favouriteCurrenciesList.Select(c => c.Id));
            //string currency = "usd";
            //string url = $"simple/price?ids={ids}&vs_currencies={currency}&include_24hr_change=true&precision=8";
            string url = "https://api.coingecko.com/api/v3/coins/dogecoin/market_chart?vs_currency=usd&days=30";
            try
            {
                var response = await HttpClientInstance.Client.GetStringAsync(url);
                JObject jObject = JObject.Parse(response);
                JArray pricesArray = (JArray)jObject["prices"];
                return pricesArray;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", ex.Message);
                return new JArray();
            }
        }
        private async void DrawChart()
        {
            pricesArray = await FetchPriceFromAPIAsync();

            foreach (JToken token in pricesArray)
            {
                long timestamp = (long)token[0];
                double price = (double)token[1];
                DateTime time = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;

                chart1.Series[0].Points.AddXY(time, price);
            }
            chart1.ChartAreas[0].RecalculateAxesScale();
            chart1.ChartAreas[0].AxisY.Name = "USD";
            chart1.Titles[0].Text = "24h Dogecoin price history in USD";
            chart1.Titles[1].Text = "USD";
            chart1.Titles[2].Text = "Time";
            chart1.Series[0].LegendText = "Dogecoin";
            chart1.Invalidate(); // Refresh chart
        }

        private void ChartForm_Load(object sender, EventArgs e)
        {
            chart1.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.Red;
            chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.Red;
            chart1.Series["Series1"].Color = Color.Green;
            //chart1.ChartAreas[0].AxisX.Interval = 2.0;
            chart1.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Days;
            //chart1.ChartAreas[0].AxisX.LabelStyle.Format = "HH:mm:ss";
            chart1.ChartAreas[0].AxisX.LabelStyle.Format = "MMMM dd";
            chart1.ChartAreas[0].AxisY.IsStartedFromZero = false;
        }
    }
}
