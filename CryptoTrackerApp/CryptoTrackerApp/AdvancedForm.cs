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
        private int _updateInterval;
        private bool _valuesChanged;
        public AdvancedForm()
        {
            InitializeComponent();
            this.FormClosing += new FormClosingEventHandler(AdvancedForm_FormClosing);
            LoadSettings();
            trackBar1.Maximum = 900;
            trackBar1.Minimum = 15;
            trackBar1.TickFrequency = 60;
            trackBar1.LargeChange = 30;
            trackBar1.SmallChange = 30;
            trackBar1.Value = _updateInterval;
            label2.Text = "" + trackBar1.Value + " Seconds";

            _valuesChanged = false;
            button1.Enabled = false;
        }
        private void AdvancedForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_valuesChanged)
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
            _updateInterval = trackBar1.Value;
            _valuesChanged = true;
            button1.Enabled = true;
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
                        _updateInterval = Convert.ToInt32(loadedSettings[0]);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error reading or deserializing the file: " + ex.Message);
                }
                
            } else {
                _updateInterval = 120;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_valuesChanged)
            {
                List<object> saveSettings = new List<object>();
                saveSettings.Add(_updateInterval);
                string json = JsonConvert.SerializeObject(saveSettings, Formatting.Indented);
                File.WriteAllText("advancedSettings.json", json);
                _valuesChanged = false;
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
