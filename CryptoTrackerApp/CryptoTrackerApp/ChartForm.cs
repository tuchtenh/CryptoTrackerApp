using ClosedXML.Excel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static ClosedXML.Excel.XLPredefinedFormat;
using static CryptoTrackerApp.Form1;
using static CryptoTrackerApp.Program;

namespace CryptoTrackerApp
{
    public partial class ChartForm : Form
    {
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
                System.DateTime time = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;

                chart1.Series[0].Points.AddXY(time, price);
            }
            chart1.ChartAreas[0].RecalculateAxesScale();
            chart1.ChartAreas[0].AxisY.Name = "USD";
            chart1.Titles[0].Text = "24h Dogecoin price history in USD";
            chart1.Titles[1].Text = "USD";
            chart1.Titles[2].Text = "Time";
            chart1.Series[0].LegendText = "Dogecoin";
            chart1.Invalidate();
        }

        private void ChartForm_Load(object sender, EventArgs e)
        {
            chart1.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.Red;
            chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.Red;
            chart1.Series["Series1"].Color = Color.Green;
            chart1.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Days;
            //chart1.ChartAreas[0].AxisX.LabelStyle.Format = "HH:mm:ss";
            chart1.ChartAreas[0].AxisX.LabelStyle.Format = "MMMM dd";
            chart1.ChartAreas[0].AxisY.IsStartedFromZero = false;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
                saveFileDialog.DefaultExt = "xlsx";
                saveFileDialog.Title = "Save as Excel File";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedPath = saveFileDialog.FileName;
                    ExportChartDataToExcel(chart1, selectedPath);
                }
            }
        }

        public void ExportChartDataToExcel(Chart chart, string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("ChartData");
                var series = chart.Series[0];

                worksheet.Cell(1, 1).Value = "DateTime";
                worksheet.Cell(1, 2).Value = "Price";

                int row = 2;
                foreach (var point in series.Points)
                {
                    System.DateTime dateTime = System.DateTime.FromOADate(point.XValue);
                    worksheet.Cell(row, 1).Value = dateTime;
                    worksheet.Cell(row, 2).Value = point.YValues[0];
                    row++;
                }
                try
                {
                    workbook.SaveAs(filePath);
                    MessageBox.Show("Successfully exported", "Export", MessageBoxButtons.OK);
                }
                catch (IOException ex)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", ex.Message);
                    MessageBox.Show("File is in use.\nPlease close and try again.", "Export", MessageBoxButtons.OK);
                    MessageBox.Show("Another user is already using this file.");
                }
                
            }
        }
    }
}
