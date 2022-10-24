﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeismicDataAnalysis.Model
{
    public class ChannelData
    {
        public string PhysicalParameter { get; set; }
        public int NumberOfPoints { get; set; }
        public double LengthOfRecord { get; set; }
        public int StationNumber { get; set; }

        //День недели, месяц, число, год и время записи
        public string WeekDay { get; set; }
        public string Month { get; set; }
        public int Day { get; set; }
        public int Year { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }
    }
}
