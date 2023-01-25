using Microsoft.Win32;
using SeismicDataAnalysis.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay;
using System.Windows;
using SeismicDataAnalysis.Model;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace SeismicDataAnalysis.Model
{
    public class EarthquakeData
    {
        public string Date { get; set; } = "";
        public string Magnitude { get; set; } = "";
        public string HypocenterDepth { get; set; } = "";
        public ObservableCollection<ChannelData> ChannelData { get; set; }

        public override string ToString()
        {
            return Date;
        }
    }
}
