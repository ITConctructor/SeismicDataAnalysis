using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeismicDataAnalysis.Model
{
    public class SpectrumObject
    {
        public string Name { get; set; } = "";
        public double Period { get; set; }
        public double Damping { get; set; }
        public double dRef { get; set; }
        public double Magnitude { get; set; }
        public double HypocenterDepth { get; set; }
        public double BaseAcceleration { get; set; }
        public double dX { get; set; }
        public double dY { get; set; }
        public double dZ { get; set; }
        public double Z2divZ1 { get; set; }
        public double BldgLength { get; set; }
        public double BldgWidth { get; set; }
        public double Acceleration { get; set; }
    }
}
