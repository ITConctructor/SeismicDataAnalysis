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
        public string Month { get; set; } = "";
        public int Day { get; set; }
        public int Year { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }

        public string StationName { get; set; } = "";
        public List<double> AccelerationsArray { get; set; } = new List<double>();
        public List<double> VelocitysArray { get; set; } = new List<double>();
        public List<double> DisplacementsArray { get; set; } = new List<double>();

        public double PeakAcceleration { get; set; }
        public double PeakVelocity { get; set; }
        public double PeakDisplacement { get; set; }

        public override string ToString()
        {
            return FileName;
        }
    }
}
