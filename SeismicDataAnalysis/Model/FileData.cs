using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeismicDataAnalysis.Model
{
    public class FileData
    {
        public string FileName { get; set; } = "";
        public string PhysicalParameter { get; set; } = "";
        public int NumberOfAccelerationsPoints { get; set; }
        public int NumberOfVelocitysPoints { get; set; }
        public int NumberOfDisplacementsPoints { get; set; }
        public double SpaceOfRecord { get; set; }
        public int StationNumber { get; set; }
        public int ChannelNumber { get; set; }

        //День недели, месяц, число, год и время записи
        public string WeekDay { get; set; } = "";
        public int Month { get; set; }
        public int Day { get; set; }
        public int Year { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }
        public string FullDate { get; set; } = "";

        public string BuildingName { get; set; } = "";
        public List<double> AccelerationsArray { get; set; } = new List<double>();
        public List<double> VelocitysArray { get; set; } = new List<double>();
        public List<double> DisplacementsArray { get; set; } = new List<double>();

        public double PeakAcceleration { get; set; }
        public double PeakVelocity { get; set; }
        public double PeakDisplacement { get; set; }
        public string Location { get; set; } = "";
        public string LocationHeight { get; set; } = "";
        public string LocationX { get; set; } = "";
        public string LocationY { get; set; } = "";
        public SerializableDictionary<double, SerializableDictionary<string, List<double>>> DampingsData { get; set; } = new SerializableDictionary<double, SerializableDictionary<string, List<double>>>();
        public double Period { get; set; }
        public List<double> Dampings { get; set; } = new List<double>();
        public List<double> FourierAmplitudeArray { get; set; } = new List<double>();
        public double InitialVelocity { get; set; }
        public double InitialDisplacement { get; set; }
        public double Sensitivity { get; set; }
        public string StationLatitude { get; set; } = "";
        public string StationLongitude { get; set; } = "";
        public double RecordLength { get; set; }
        public double Damping { get; set; }
        public double UncorMax { get; set; }
        public string Orientation { get; set; } = "";
        public string BuildingLength { get; set; } = "";
        public string BuildingWidth { get; set; } = "";

        public override string ToString()
        {
            return FileName;
        }
    }
}
