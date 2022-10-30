using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.DataVisualization;

namespace SeismicDataAnalysis.Model
{
    public static class DataTransformUtils
    {
        public static ChannelData TransformRawData(string rawData)
        {
            ChannelData chan = new ChannelData();
            //Считываем тип физического параметра
            chan.PhysicalParameter = rawData[..24];
            //Считываем количество значений измерения
            int pointsNumberEndIndex = rawData.IndexOf(" points of accel data");
            int pointsNumberStartIndex = rawData[..pointsNumberEndIndex].LastIndexOf(' ') - 1;
            int pointsNumber;
            if (int.TryParse(rawData[pointsNumberStartIndex..pointsNumberEndIndex], out pointsNumber))
            {
                chan.NumberOfPoints = pointsNumber;
            }
            //Считываем длительность измерения
            int recordSpaceStartIndex = pointsNumberEndIndex + 41;
            int recordSpaceEndIndex = rawData[recordSpaceStartIndex..].IndexOf(' ') + recordSpaceStartIndex;
            string strRecordSpace = "0" + rawData[recordSpaceStartIndex..recordSpaceEndIndex];
            strRecordSpace = strRecordSpace.Replace('.', ',');
            double recordSpace;
            if (double.TryParse(strRecordSpace, out recordSpace))
            {
                chan.SpaceOfRecord = recordSpace;
            }

            //Считываем номер станции
            int stationNumberStartIndex = rawData.IndexOf("Station No.") + 12;
            int stationNumberEndIndex = rawData[stationNumberStartIndex..].IndexOf(' ') + stationNumberStartIndex;
            string strStationNumber = rawData[stationNumberStartIndex..stationNumberEndIndex];
            int stationNumber;
            if (int.TryParse(strStationNumber, out stationNumber))
            {
                chan.StationNumber = stationNumber;
            }

            //Считываем дату и время записи
            int recordWDStartIndex = rawData.IndexOf("Rcrd") + 8;
            int recordWDEndIndex = recordWDStartIndex + 3;
            chan.WeekDay = rawData[recordWDStartIndex..recordWDEndIndex];

            int recordMonthStartIndex = recordWDEndIndex + 1;
            int recordMonthEndIndex = rawData[recordMonthStartIndex..].IndexOf(' ') + recordMonthStartIndex;
            chan.Month = rawData[recordMonthStartIndex..recordMonthEndIndex];

            int recordDayStartIndex = recordMonthEndIndex + 1;
            int recordDayEndIndex = rawData[recordDayStartIndex..].IndexOf(',') + recordDayStartIndex;
            string strRecordDay = rawData[recordDayStartIndex..recordDayEndIndex];
            int recordDay;
            if (int.TryParse(strRecordDay, out recordDay))
            {
                chan.Day = recordDay;
            }

            int recordYearStartIndex = recordDayEndIndex + 2;
            int recordYearEndIndex = recordYearStartIndex + 4;
            string strRecordYear = rawData[recordYearStartIndex..recordYearEndIndex];
            int recordYear;
            if(int.TryParse(strRecordYear, out recordYear))
            {
                chan.Year = recordYear;
            }

            int recordTimeStartIndex = recordYearEndIndex + 1;
            int recordTimeEndIndex = rawData[recordTimeStartIndex..].IndexOf('.') + recordTimeStartIndex;
            string[] timeArray = rawData[recordTimeStartIndex..recordTimeEndIndex].Split(':');
            int Hour;
            if (int.TryParse(timeArray[0], out Hour))
            {
                chan.Hour = Hour;
            }
            int Minute;
            if (int.TryParse(timeArray[1], out Minute))
            {
                chan.Minute = Minute;
            }
            int Second;
            if (int.TryParse(timeArray[2], out Second))
            {
                chan.Second = Second;
            }

            //Считываем название станции
            int bldgIndex = rawData.IndexOf("Bldg");
            int stationSeparatorIndex = rawData[..bldgIndex].LastIndexOf(" -");
            int stationNameIndex = rawData[..stationSeparatorIndex].LastIndexOf(")") + 3;
            string stationName = rawData[stationNameIndex..(bldgIndex + 4)];
            chan.StationName = stationName;

            //Считываем данные измерений ускорений
            int measuresStartIndex = rawData.IndexOf("8f10.6") + 9;
            int measuresEndIndex = rawData.IndexOf("&  ----------  End of data for channel");
            string strAccelerations = rawData[measuresStartIndex..measuresEndIndex];
            List<double> accelerations = new List<double>();
            int k = 0;
            for (int i = 0; i < strAccelerations.Length; i = k)
            {
                int j = i;
                while (strAccelerations[j] == ' ')
                {
                    j++;
                }
                int lastIndex = strAccelerations[j..].IndexOf(' ') + j;
                //Создаем строковое значение ускорения, учитывая отрицательные ускорения и отсутствие разряда целых чисел при равенстве этого разряда нулю
                if (lastIndex - j != -1)
                {
                    string strBaseA = strAccelerations[j..lastIndex];
                    string strA = "";
                    if (strBaseA[0] == '.')
                    {
                        strA = "0" + strBaseA;
                        strA = strA.Replace('.', ',');
                    }
                    else if (strBaseA[0] == '-' && strBaseA[1] == '.')
                    {
                        strA = "-0" + strBaseA[1..];
                        strA = strA.Replace('.', ',');
                    }
                    else
                    {
                        strA = strA.Replace('.', ',');
                    }
                    double A;
                    if (double.TryParse(strA, out A))
                    {
                        accelerations.Add(A);
                    }
                    k = lastIndex + 1;
                }
                else
                {
                    break;
                }
            }
            chan.AccelerationsArray = accelerations;
            return new ChannelData();
        }
    }
}
