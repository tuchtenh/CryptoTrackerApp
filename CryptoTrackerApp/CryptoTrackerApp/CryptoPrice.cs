using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoTrackerApp
{
    internal class CryptoPrice
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double? Price { get; set; }

        private double? change;
        public double? PercentageChange24Hr
        {
            get => change;
            set => change = (value.HasValue) ? (Math.Round(value.Value, 2)) : ((double?)null);
        }
    }
}
