using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeismicDataAnalysis.Model
{
    public class DataBase
    {
        public List<string> LoadedData { get; set; } = new List<string>();
        public ObservableCollection<FileData> TransformedData { get; set; } = new ObservableCollection<FileData>();
        public ObservableCollection<BuildingData> Buildings { get; set; } = new ObservableCollection<BuildingData>();
    }
}
