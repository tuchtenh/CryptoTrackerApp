using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using CryptoTrackerApp;

namespace CryptoTrackerApp
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainMenuForm());
        }

        public static class HttpClientInstance
        {
            public static readonly HttpClient Client = new HttpClient
            {
                BaseAddress = new Uri("https://api.coingecko.com/api/v3/")
            };
        }
    }
}
