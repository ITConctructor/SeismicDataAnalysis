using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            int recordSpaceStartIndex = pointsNumberEndIndex + 42;
            int recordSpaceEndIndex = rawData[recordSpaceStartIndex..].IndexOf(' ') - 1;
            string strRecordSpace = "0" + rawData[recordSpaceStartIndex..recordSpaceEndIndex];
            int recordSpace;
            if (int.TryParse(strRecordSpace, out recordSpace))
            {
                chan.SpaceOfRecord = recordSpace;
            }

            //Считываем номер станции
            int stationNumberStartIndex = rawData.IndexOf("Station No.") + 13;
            int stationNumberEndIndex = rawData[stationNumberStartIndex..].IndexOf(' ') - 1;
            string strStationNumber = rawData[stationNumberStartIndex..recordSpaceEndIndex];
            int stationNumber;
            if (int.TryParse(strStationNumber, out stationNumber))
            {
                chan.StationNumber = stationNumber;
            }

            //Считываем дату и время записи
            int recordWDStartIndex = rawData.IndexOf("RCRD") + 8;
            int recordWDEndIndex = recordWDStartIndex + 2;
            chan.WeekDay = rawData[recordWDStartIndex..recordWDEndIndex];

            int recordMonthStartIndex = recordWDEndIndex + 2;
            int recordMonthEndIndex = rawData[recordMonthStartIndex..].IndexOf(' ') - 1;
            chan.Month = rawData[recordMonthStartIndex..recordMonthEndIndex];

            int recordDayStartIndex = recordMonthEndIndex + 2;
            int recordDayEndIndex = rawData[recordDayStartIndex..].IndexOf(',') - 1;
            string strRecordDay = rawData[recordDayStartIndex..recordDayEndIndex];
            int recordDay;
            if (int.TryParse(strRecordDay, out recordDay))
            {
                chan.Day = recordDay;
            }

            int recordYearStartIndex = recordDayEndIndex + 3;
            int recordYearEndIndex = recordYearStartIndex + 3;
            string strRecordYear = rawData[recordYearStartIndex..recordYearEndIndex];
            int recordYear;
            if(int.TryParse(strRecordYear, out recordYear))
            {
                chan.Year = recordYear;
            }

            int recordHourStartIndex = recordYearEndIndex + 2;
            int recordHourEndIndex = recordHourStartIndex + 1;
            string strRecordHour = rawData[recordHourStartIndex..recordHourEndIndex];
            int recordHour;
            if (int.TryParse(strRecordHour, out recordHour))
            {
                chan.Hour = recordHour;
            }

            int recordTimeStartIndex = recordYearEndIndex + 2;
            int recordTimeEndIndex = rawData[recordTimeStartIndex..].IndexOf('.') - 1;
            string[] timeArray = rawData[recordTimeStartIndex..recordTimeEndIndex].Split(':');
            chan.Hour = int.Parse(timeArray[0]);
            chan.Minute = int.Parse(timeArray[1]);
            chan.Second = int.Parse(timeArray[2]);
            return new ChannelData();

            //Считываем название станции
            int bldgIndex = rawData.IndexOf("Bldg");
            int stationSeparatorIndex = rawData[..bldgIndex].LastIndexOf(" _");
            int stationNameIndex = rawData[..stationSeparatorIndex].LastIndexOf(' ');
            string stationName = rawData[stationNameIndex..(bldgIndex + 3)];
            chan.StationName = stationName;
        }
    }
}
