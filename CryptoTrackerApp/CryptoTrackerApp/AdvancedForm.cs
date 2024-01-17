using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static CryptoTrackerApp.MainMenuForm;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;

namespace CryptoTrackerApp
{
    public partial class AdvancedForm : Form
    {
        private string globalCurrency;
        private int updateInterval;
        private bool valuesChanged;
        public AdvancedForm()
        {
            InitializeComponent();
            this.FormClosing += new FormClosingEventHandler(AdvancedForm_FormClosing);
            LoadSettings();

            comboBox1.SelectedIndexChanged -= comboBox1_SelectedIndexChanged;
            ComboItem[] ComboItemArray = new ComboItem[] {
                new ComboItem{ Name = "USD - United States Dollar", Value = "usd" },
                new ComboItem{ Name = "EUR - Euro", Value = "eur"}
            };
            comboBox1.DataSource = ComboItemArray;
            comboBox1.DisplayMember = "Name";
            var selectedItem = ComboItemArray.Select(c => c.Value == globalCurrency);
            if (selectedItem != null)
            {
                comboBox1.SelectedItem = selectedItem;
            }
            else { comboBox1.SelectedItem = ComboItemArray[0]; }
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;

            trackBar1.Maximum = 900;
            trackBar1.Minimum = 60;
            trackBar1.TickFrequency = 60;
            trackBar1.LargeChange = 30;
            trackBar1.SmallChange = 30;
            trackBar1.Value = updateInterval;
            label2.Text = "" + trackBar1.Value + " Seconds";

            valuesChanged = false;
            button1.Enabled = false;
        }
        private void AdvancedForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (valuesChanged)
            {
                var confirmResult = MessageBox.Show("Are you sure you want to close without saving?", "Close", MessageBoxButtons.YesNo);
                _ = (confirmResult == DialogResult.Yes) ? (e.Cancel = false) : (e.Cancel = true);
            }
            else
            {
                e.Cancel = false;
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label2.Text = "" + trackBar1.Value + " Seconds";
            updateInterval = trackBar1.Value;
            valuesChanged = true;
            button1.Enabled = true;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboItem selected = (ComboItem)comboBox1.SelectedItem;
            globalCurrency = selected.Value;
            valuesChanged = true;
            button1.Enabled = true;
        }

        class ComboItem
        {
            public string Name { get; set; }
            public string Value { get; set; }
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
                        globalCurrency = loadedSettings[0].ToString();
                        updateInterval = Convert.ToInt32(loadedSettings[1]);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error reading or deserializing the file: " + ex.Message);
                }
                
            } else {
                globalCurrency = "usd";
                updateInterval = 60;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (valuesChanged)
            {
                List<object> saveSettings = new List<object>();
                saveSettings.Add(globalCurrency);
                saveSettings.Add(updateInterval);
                string json = JsonConvert.SerializeObject(saveSettings, Formatting.Indented);
                File.WriteAllText("advancedSettings.json", json);
                valuesChanged = false;
                button1.Enabled = false;
            }
            MessageBox.Show("Data saved", "Saved", MessageBoxButtons.OK);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
