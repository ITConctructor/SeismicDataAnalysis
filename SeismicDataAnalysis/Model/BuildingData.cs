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
    public class BuildingData
    {
        public string BuildingName { get; set; } = "";
        public string BuildingLength { get; set; } = "";
        public string BuildingWidth { get; set; } = "";
        public string StructuralReference { get; set; } = "";
        public ObservableCollection<EarthquakeData> EarthquakeData { get; set; }

        public override string ToString()
        {
            return BuildingName;
        }
    }
}
