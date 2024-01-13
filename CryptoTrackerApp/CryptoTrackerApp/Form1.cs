using Newtonsoft.Json;
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
using static CryptoTrackerApp.Form1;
using static CryptoTrackerApp.Program;

namespace CryptoTrackerApp
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            dataGridView1.DataSource = new List<CryptoCurrency>();
        }

        private void button1_Click(object sender, EventArgs e)
        {
           
        }

        private async Task APIStuff()
        {
            try
            {
                //HttpResponseMessage response = await _httpClient.GetAsync("/simple/supported_vs_currencies");
                //response.EnsureSuccessStatusCode();
                //string responseBody = await response.Content.ReadAsStringAsync();
                // Above three lines can be replaced with new helper method below
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
            optionsForm.ShowDialog();
        }


        public class Currency
        {
            public string Name { get; set; }
        }

        public class CryptoCurrency
        {
            public string Id { get; set; }
            public string Symbol { get; set; }
            public string Name { get; set; }
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
