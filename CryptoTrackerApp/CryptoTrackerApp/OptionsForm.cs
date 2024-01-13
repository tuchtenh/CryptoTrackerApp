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
    public partial class OptionsForm : Form
    {
        public OptionsForm()
        {
            InitializeComponent();
            dataGridView1.DataSource = new List<Form1.CryptoCurrency>();
            dataGridView2.DataSource = new List<Form1.CryptoCurrency>();
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private async Task<List<CryptoCurrency>> FetchCryptoData()
        {
            try
            {
                var response = await HttpClientInstance.Client.GetStringAsync("coins/list?include_platform=false");
                var cryptos = JsonConvert.DeserializeObject<List<CryptoCurrency>>(response);
                return cryptos;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", ex.Message);
                return new List<CryptoCurrency>();
            }
        }

        private async void LoadDataToDataGridView()
        {
            var cryptoData = await FetchCryptoData();
            dataGridView1.DataSource = cryptoData;
        }

        // Save
        private void button1_Click(object sender, EventArgs e)
        {

        }

        // Close
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Reset
        private void button3_Click(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show("Reset?","Reset",MessageBoxButtons.OKCancel);
            if (confirmResult == DialogResult.OK)
            {
                LoadDataToDataGridView();
            }
        }

        // Add/Remove
        private void button4_Click(object sender, EventArgs e)
        {
            MoveSelectedRow(dataGridView1, dataGridView2);
        }

        private void MoveSelectedRow(DataGridView source, DataGridView destination)
        {
            if(source.CurrentRow != null)
            {
                DataGridViewRow selectedRow = source.CurrentRow;
                destination.Rows.Add(selectedRow.Clone());

            }
        }
    }
}
