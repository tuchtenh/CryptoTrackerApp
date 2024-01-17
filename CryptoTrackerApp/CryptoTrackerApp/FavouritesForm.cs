using DocumentFormat.OpenXml.Office2010.PowerPoint;
using Newtonsoft.Json;
using System;
using System.Collections;
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
using static CryptoTrackerApp.MainMenuForm;
using static CryptoTrackerApp.Program;

namespace CryptoTrackerApp
{
    public partial class FavouritesForm : Form
    {
        private BindingList<CryptoCurrency> allCurrenciesList = new BindingList<CryptoCurrency>();
        private BindingList<CryptoCurrency> favouriteCurrenciesList = new BindingList<CryptoCurrency>();
        private bool listsChanged;

        public FavouritesForm()
        {
            InitializeComponent();
            this.FormClosing += new FormClosingEventHandler(OptionsForm_FormClosing);
            allCurrenciesList = LoadList("allCurrenciesList.json", allCurrenciesList);
            favouriteCurrenciesList = LoadList("favouriteCurrenciesList.json", favouriteCurrenciesList);
            dataGridView1.AllowUserToAddRows = false;
            dataGridView2.AllowUserToAddRows = false;
            dataGridView1.DataSource = allCurrenciesList;
            dataGridView2.DataSource = favouriteCurrenciesList;
            listsChanged = false;
            button1.Enabled = false;
        }

        private async void LoadDataToDataGridView()
        {
            allCurrenciesList = await FetchCryptoData();
            dataGridView1.DataSource = allCurrenciesList;
        }

        private async Task<BindingList<CryptoCurrency>> FetchCryptoData()
        {
            try
            {
                var response = await HttpClientInstance.Client.GetStringAsync("coins/list?include_platform=false");
                var cryptos = JsonConvert.DeserializeObject<BindingList<CryptoCurrency>>(response);
                return cryptos;
            }
            catch (HttpRequestException ex)
            {
                if (ex.Message.Contains("429"))
                {
                    MessageBox.Show("Too many request. Try again later");
                    Console.WriteLine("\nException Caught!\nMessage :{0} ", ex.Message);
                    return new BindingList<CryptoCurrency>();
                }
                else
                {
                    Console.WriteLine("\nException Caught!\nMessage :{0} ", ex.Message);
                    return new BindingList<CryptoCurrency>();
                }                
            }
        }

        // Save
        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (listsChanged) {
                string json = JsonConvert.SerializeObject(favouriteCurrenciesList, Formatting.Indented);
                File.WriteAllText("favouriteCurrenciesList.json", json);
                json = JsonConvert.SerializeObject(allCurrenciesList, Formatting.Indented);
                File.WriteAllText("allCurrenciesList.json", json);
                listsChanged = false;
                button1.Enabled = false;
            }
            MessageBox.Show("Data saved", "Saved", MessageBoxButtons.OK);
        }

        // Close
        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OptionsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (listsChanged)
            {
                var confirmResult = MessageBox.Show("Are you sure you want to close without saving?", "Close", MessageBoxButtons.YesNo);
                _ = (confirmResult == DialogResult.Yes) ? (e.Cancel = false) : (e.Cancel = true);
            }
            else
            {
                e.Cancel = false;
            }

        }

        // Reset
        private void ResetButton_Click(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show("Reset?","Reset",MessageBoxButtons.OKCancel);
            if (confirmResult == DialogResult.OK)
            {
                LoadDataToDataGridView();
                listsChanged = true;
                button1.Enabled = true;
            }
        }

        // -->
        private void AddToFavouritesButton_Click(object sender, EventArgs e)
        {
            AddSelectedRow(dataGridView1, dataGridView2);
            listsChanged = true;
            button1.Enabled = true;
        }

        private void AddSelectedRow(DataGridView source, DataGridView destination)
        {
            if (source.CurrentRow != null)
            {
                foreach(DataGridViewRow row in source.SelectedRows)
                {
                    CryptoCurrency crypto = row.DataBoundItem as CryptoCurrency;
                    if (crypto != null)
                    {
                        allCurrenciesList.Remove(crypto);
                        if (!favouriteCurrenciesList.Any(c => c.Id == crypto.Id))
                        {
                            favouriteCurrenciesList.Add(crypto);
                        }
                    }
                }
                source.Refresh();
                destination.Refresh();
            }
        }

        // <--
        private void RemoveFromFavouritesButton_Click(object sender, EventArgs e)
        {
            RemoveSelectedRow(dataGridView2, dataGridView1);
            listsChanged = true;
            button1.Enabled = true;
        }

        private void RemoveSelectedRow(DataGridView source, DataGridView destination)
        {
            if (source.CurrentRow != null)
            {
                foreach (DataGridViewRow row in source.SelectedRows)
                {
                    CryptoCurrency crypto = row.DataBoundItem as CryptoCurrency;
                    if (crypto != null)
                    {
                        favouriteCurrenciesList.Remove(crypto);
                        if (!allCurrenciesList.Any(c => c.Id == crypto.Id))
                        {
                            allCurrenciesList.Add(crypto);
                        }
                    }
                }
                source.Refresh();
                destination.Refresh();
            }
        }

        private BindingList<CryptoCurrency> LoadList(string filePath, BindingList<CryptoCurrency> cryptos)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    using (StreamReader r = new StreamReader(filePath))
                    {
                        string json = File.ReadAllText(filePath);
                        cryptos = JsonConvert.DeserializeObject<BindingList<CryptoCurrency>>(json);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error reading or deserializing the file: " + ex.Message);
                }
            }
            return cryptos;
        }


        private void AdvancedButton_Click(object sender, EventArgs e)
        {
            AdvancedForm advancedForm = new AdvancedForm();
            advancedForm.FormClosing += new FormClosingEventHandler(this.AdvancedForm_FormClosing);
            advancedForm.ShowDialog();
        }

        private void AdvancedForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Console.WriteLine("Exit Advamced");
        }
    }
}
