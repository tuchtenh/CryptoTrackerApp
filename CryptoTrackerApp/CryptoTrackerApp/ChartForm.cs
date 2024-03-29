﻿using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2016.Excel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static ClosedXML.Excel.XLPredefinedFormat;
using static CryptoTrackerApp.MainMenuForm;
using static CryptoTrackerApp.Program;

namespace CryptoTrackerApp
{
    public partial class ChartForm : Form
    {
        JArray _pricesArray = new JArray();
        CryptoPrice _cryptocurrencyData;

        public ChartForm()
        {
            InitializeComponent();
        }

        private void ChartForm_Load(object sender, EventArgs e)
        {
            chart1.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.Red;
            chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.Red;
            chart1.Series["Series1"].Color = Color.Green;
            chart1.ChartAreas[0].AxisY.IsStartedFromZero = false;
            Draw24HourChart();
        }

        public void SetSelectedRow(DataGridViewRow cryptocurrencyData)
        {
            this._cryptocurrencyData = cryptocurrencyData.DataBoundItem as CryptoPrice;
        }

        private async Task<JArray> FetchPriceFromAPIAsync(string currencyId, int dayHistory)
        {
            string url = $"coins/{currencyId}/market_chart?vs_currency=usd&days={dayHistory}";
            try
            {
                var response = await HttpClientInstance.Client.GetStringAsync(url);
                JObject jObject = JObject.Parse(response);
                JArray pricesArray = (JArray)jObject["prices"];
                return pricesArray;
            }
            catch (HttpRequestException ex)
            {
                if (ex.Message.Contains("429"))
                {
                    MessageBox.Show("Too many request. Try again later");
                    Console.WriteLine("\nException Caught!\nMessage :{0} ", ex.Message);
                    return new JArray();
                } else
                {
                    Console.WriteLine("\nException Caught!\nMessage :{0} ", ex.Message);
                    return new JArray();
                }
            }
        }
        private async void DrawChart(string currencyId, int dayHistory)
        {
            chart1.Series[0].Points.Clear();
            _pricesArray = await FetchPriceFromAPIAsync(currencyId, dayHistory);

            foreach (JToken token in _pricesArray)
            {
                long timestamp = (long)token[0];
                double price = (double)token[1];
                System.DateTime time = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;

                chart1.Series[0].Points.AddXY(time, price);
            }
            chart1.ChartAreas[0].RecalculateAxesScale();
            chart1.Invalidate();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                
                saveFileDialog.DefaultExt = "xlsx";
                saveFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

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
                    Console.WriteLine("\nException Caught!\nMessage :{0} ", ex.Message);
                    MessageBox.Show("File is in use.\nPlease close and try again.", "Export", MessageBoxButtons.OK);
                }
                
            }
        }
        
        private void Draw24HourChart()
        {
            chart1.ChartAreas[0].AxisY.Name = "USD";
            chart1.Titles[0].Text = $"24h {_cryptocurrencyData.Name} price history in USD";
            chart1.Titles[1].Text = "USD";
            chart1.Titles[2].Text = "Time";
            chart1.Series[0].LegendText = $"{_cryptocurrencyData.Name}";
            chart1.ChartAreas[0].AxisX.Interval = 1;
            chart1.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Hours;
            chart1.ChartAreas[0].AxisX.LabelStyle.Format = "HH:mm:ss";
            DrawChart(_cryptocurrencyData.Id, 1);
        }
        private void Day1Button_Click(object sender, EventArgs e)
        {
            Draw24HourChart();
        }

        private void Day3Button_Click(object sender, EventArgs e)
        {
            ChartSettings(3, 0.5, timeFormat: "dd/MM HH:mm");
        }

        private void Day7Button_Click(object sender, EventArgs e)
        {
            ChartSettings(7, 1);
        }

        private void Day14Button_Click(object sender, EventArgs e)
        {
            ChartSettings(14, 2);
        }

        private void Day30Button_Click(object sender, EventArgs e)
        {
            ChartSettings(30, 3);
        }

        private void Day90Button_Click(object sender, EventArgs e)
        {
            ChartSettings(90, 9);
        }

        private void ChartSettings(int days, double interval, string timeFormat = "dd/MM")
        {
            chart1.Titles[0].Text = $"{days} Day {_cryptocurrencyData.Name} price history in USD";
            chart1.Series[0].LegendText = $"{_cryptocurrencyData.Name}";
            chart1.ChartAreas[0].AxisX.Interval = interval;
            chart1.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Days;
            chart1.ChartAreas[0].AxisX.LabelStyle.Format = "dd/MM";
            DrawChart(_cryptocurrencyData.Id, days);
        }
    }
}
