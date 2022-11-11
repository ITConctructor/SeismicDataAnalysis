using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.DataVisualization;

namespace SeismicDataAnalysis.Model
{
    public static class DataTransformUtils
    {
        public static FileData TransformRawData(string rawData)
        {
            FileData chan = new FileData();
            //Считываем тип физического параметра
            chan.PhysicalParameter = rawData[..24];
            //Считываем количество значений измерения ускорения
            int pointsANumberEndIndex = rawData.IndexOf(" points of accel data");
            int pointsANumberStartIndex = rawData[..pointsANumberEndIndex].LastIndexOf(' ') - 1;
            int pointsANumber;
            if (int.TryParse(rawData[pointsANumberStartIndex..pointsANumberEndIndex], out pointsANumber))
            {
                chan.NumberOfAccelerationsPoints = pointsANumber;
            }
            //Считываем количество значений измерения скорости
            int pointsVNumberEndIndex = rawData.IndexOf(" points of veloc data");
            int pointsVNumberStartIndex = rawData[..pointsVNumberEndIndex].LastIndexOf(' ') - 1;
            int pointsVNumber;
            if (int.TryParse(rawData[pointsVNumberStartIndex..pointsVNumberEndIndex], out pointsVNumber))
            {
                chan.NumberOfVelocitysPoints = pointsVNumber;
            }
            //Считываем количество значений измерения распределения
            int pointsDNumberEndIndex = rawData.IndexOf(" points of displ data");
            int pointsDNumberStartIndex = rawData[..pointsDNumberEndIndex].LastIndexOf(' ') - 1;
            int pointsDNumber;
            if (int.TryParse(rawData[pointsDNumberStartIndex..pointsDNumberEndIndex], out pointsDNumber))
            {
                chan.NumberOfDisplacementsPoints = pointsDNumber;
            }
            //Считываем длительность измерения
            int recordSpaceStartIndex = pointsANumberEndIndex + 41;
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
            int measuresEndIndex1 = rawData.IndexOf(" points of veloc data equally spaced at  .010 sec, in cm/sec.  (8f10.7)");
            int measuresEndIndex = rawData.Substring(0, measuresEndIndex1).LastIndexOf(' ');
            string strAccelerations = rawData[measuresStartIndex..measuresEndIndex];
            chan.AccelerationsArray = ParseStringToDoubleList(strAccelerations);

            //Считываем данные измерений скоростей
            int velocStartIndex = rawData.IndexOf(" points of veloc data equally spaced at  .010 sec, in cm/sec.  (8f10.7)") + 72;
            int velocEndIndex1 = rawData.IndexOf(" points of displ data equally spaced at  .010 sec, in cm.      (8f10.7)");
            int velocEndIndex = rawData.Substring(0, velocEndIndex1).LastIndexOf(' ');
            string strVelocs = rawData[velocStartIndex..velocEndIndex];
            chan.VelocitysArray = ParseStringToDoubleList(strVelocs);

            //Считываем данные распределения
            int dispStartIndex = rawData.IndexOf(" points of displ data equally spaced at  .010 sec, in cm.      (8f10.7)") + 72;
            int dispEndIndex = rawData.IndexOf("&  ----------  End of data for channel") - 1;
            string strDisps = rawData[dispStartIndex..dispEndIndex];
            chan.DisplacementsArray = ParseStringToDoubleList(strDisps);

            //Считываем номер канала
            int numStartIndex1 = rawData.IndexOf("End of data for channel") + 24;
            int n = 1;
            while (rawData.Substring(numStartIndex1)[n] == 0)
            {
                n++;
            }
            int numStartIndex = numStartIndex1 + n;
            int numEndIndex = rawData.Substring(numStartIndex).IndexOf(' ');
            string strNum = rawData[numStartIndex..(numEndIndex + numStartIndex)];
            int num = 0;
            int.TryParse(strNum, out num);
            chan.ChannelNumber = num;

            //Считываем максимальное ускорение
            int maxAStartIndex1 = rawData.IndexOf("Peak acceleration = ") + 20;
            int indA = 0;
            while (rawData[maxAStartIndex1 + indA] == ' ')
            {
                indA++;
            }
            int maxAStartIndex = maxAStartIndex1 + indA;
            int maxAEndIndex = rawData[maxAStartIndex..].IndexOf(' ') + maxAStartIndex;
            string strMaxA = rawData[maxAStartIndex..maxAEndIndex];
            strMaxA = strMaxA.Replace('.', ',');
            double maxA;
            if(double.TryParse(strMaxA, out maxA))
            {
                chan.PeakAcceleration = maxA;
            }

            //Считываем максимальную скорость
            int maxVStartIndex1 = rawData.IndexOf("Peak   velocity   = ") + 20;
            int indV = 0;
            while (rawData[maxVStartIndex1 + indV] == ' ')
            {
                indV++;
            }
            int maxVStartIndex = maxVStartIndex1 + indV;
            int maxVEndIndex = rawData[maxVStartIndex..].IndexOf(' ') + maxVStartIndex;
            string strMaxV = rawData[maxVStartIndex..maxVEndIndex];
            strMaxV = strMaxV.Replace('.', ',');
            double maxV;
            if (double.TryParse(strMaxV, out maxV))
            {
                chan.PeakVelocity = maxV;
            }

            //Считываем максимальное распределение
            int maxDStartIndex1 = rawData.IndexOf("Peak displacement = ") + 20;
            int indD = 0;
            while (rawData[maxDStartIndex1 + indD] == ' ')
            {
                indD++;
            }
            int maxDStartIndex = maxDStartIndex1 + indD;
            int maxDEndIndex = rawData[maxDStartIndex..].IndexOf(' ') + maxDStartIndex;
            string strMaxD = rawData[maxDStartIndex..maxDEndIndex];
            strMaxD = strMaxD.Replace('.', ',');
            double maxD;
            if (double.TryParse(strMaxD, out maxD))
            {
                chan.PeakDisplacement = maxD;
            }
            return chan;
        }

        public static KeyValuePair<double, double>[] CreateKVPairs(FileData transformedData)
        {
            KeyValuePair<double, double>[] result = new KeyValuePair<double, double>[transformedData.AccelerationsArray.Count];
            //Создаем список значений по х
            double[] xAxis = new double[transformedData.AccelerationsArray.Count];
            for (int i = 0; i < transformedData.AccelerationsArray.Count; i++)
            {
                xAxis.Append(i * transformedData.SpaceOfRecord);
            }
            //Заполняем масив пар ключей-значений
            for (int i = 0; i < transformedData.AccelerationsArray.Count; i++)
            {
                KeyValuePair<double, double> pair = new KeyValuePair<double, double>(transformedData.AccelerationsArray[i], xAxis[i]);
                result.Append(pair);
            }
            return result;
        }

        public static double[] CreateArrayFromSpan(double span, int count)
        {
            double[] result = new double[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = span * i;
            }
            return result;
        }

        public static ObservableCollection<ChannelData> CreatePivots(ObservableCollection<FileData> transformedData)
        {
            ObservableCollection<ChannelData> result = new ObservableCollection<ChannelData>();
            //Исследуем все содержащиеся в файлах каналы
            List<int> chanNums = new List<int>();
            foreach (FileData file in transformedData)
            {
                if (!chanNums.Contains(file.ChannelNumber))
                {
                    chanNums.Add(file.ChannelNumber);
                }
            }
            //Создаем списки файлов одного канала
            List<List<FileData>> files = new List<List<FileData>>();
            foreach (int num in chanNums)
            {
                List<FileData> data = new List<FileData>();
                foreach (FileData file in transformedData)
                {
                    if (file.ChannelNumber == num)
                    {
                        data.Add(file);
                    }
                }
                files.Add(data);
            }
            //Заполняем базу данных каждого канала, выбирая первое ненулвое значение из файлов канала
            for (int i = 0; i < chanNums.Count; i++)
            {
                ChannelData chan = new ChannelData();
                chan.ChannelNumber = chanNums[i];
                foreach (FileData file in files[i])
                {
                    if (file.AccelerationsArray.Count != 0)
                    {
                        chan.AccelerationsArray = file.AccelerationsArray;
                        chan.NumberOfAccelerationsPoints = file.NumberOfAccelerationsPoints;
                    }
                    break;
                }
                foreach (FileData file in files[i])
                {
                    if (file.VelocitysArray.Count != 0)
                    {
                        chan.VelocitysArray = file.VelocitysArray;
                        chan.NumberOfVelocitysPoints = file.NumberOfVelocitysPoints;
                    }
                    break;
                }
                foreach (FileData file in files[i])
                {
                    if (file.DisplacementsArray.Count != 0)
                    {
                        chan.DisplacementsArray = file.DisplacementsArray;
                        chan.NumberOfDisplacementsPoints = file.NumberOfDisplacementsPoints;
                    }
                    break;
                }
                foreach (FileData file in files[i])
                {
                    if (file.WeekDay != "")
                    {
                        chan.WeekDay = file.WeekDay;
                    }
                    break;
                }
                foreach (FileData file in files[i])
                {
                    if (file.Month != "")
                    {
                        chan.Month = file.Month;
                    }
                    break;
                }
                foreach (FileData file in files[i])
                {
                    if (file.Day != 0)
                    {
                        chan.Day = file.Day;
                    }
                    break;
                }
                foreach (FileData file in files[i])
                {
                    if (file.Year != 0)
                    {
                        chan.Year = file.Year;
                    }
                    break;
                }
                foreach (FileData file in files[i])
                {
                    if (file.Hour != 0)
                    {
                        chan.Hour = file.Hour;
                    }
                    break;
                }
                foreach (FileData file in files[i])
                {
                    if (file.Minute != 0)
                    {
                        chan.Minute = file.Minute;
                    }
                    break;
                }
                foreach (FileData file in files[i])
                {
                    if (file.Second != 0)
                    {
                        chan.Second = file.Second;
                    }
                    break;
                }
                foreach (FileData file in files[i])
                {
                    if (file.StationName != "")
                    {
                        chan.StationName = file.StationName;
                    }
                    break;
                }
                foreach (FileData file in files[i])
                {
                    if (file.StationNumber != 0)
                    {
                        chan.StationNumber = file.StationNumber;
                    }
                    break;
                }
                foreach (FileData file in files[i])
                {
                    if (file.SpaceOfRecord != 0)
                    {
                        chan.SpaceOfRecord = file.SpaceOfRecord;
                    }
                    break;
                }
                foreach (FileData file in files[i])
                {
                    if (file.PeakAcceleration != 0)
                    {
                        chan.PeakAcceleration = file.PeakAcceleration;
                    }
                    break;
                }
                foreach (FileData file in files[i])
                {
                    if (file.PeakVelocity != 0)
                    {
                        chan.PeakVelocity = file.PeakVelocity;
                    }
                    break;
                }
                foreach (FileData file in files[i])
                {
                    if (file.PeakDisplacement != 0)
                    {
                        chan.PeakDisplacement = file.PeakDisplacement;
                    }
                    break;
                }
                result.Add(chan);
            }
            return result;
        }

        public static List<double> ParseStringToDoubleList(string strAccelerations)
        {
            List<double> result = new List<double>();   
            string[] strAArray = strAccelerations.Split('.');
            for (int i = 1; i < strAArray.Length; i++)
            {
                string strA3 = "";
                for (int j = 0; j < strAArray[i].Length; j++)
                {
                    if (strAArray[i][j] == ' ')
                    {
                        strA3 = strAArray[i][0..strAArray[i].IndexOf(' ')];
                        break;
                    }
                    else if (strAArray[i][j] == '-')
                    {
                        strA3 = strAArray[i][0..strAArray[i].IndexOf('-')];
                        break;
                    }
                }
                string strA1 = "";
                for (int j = strAArray[i - 1].Length - 1; j > -1; j--)
                {
                    if (strAArray[i - 1][j] == ' ' && j == strAArray[i - 1].Length - 1)
                    {
                        strA1 = "0,";
                        break;
                    }
                    else if (strAArray[i - 1][j] == ' ' && j < strAArray[i - 1].Length - 1)
                    {
                        strA1 = strAArray[i - 1][j..] + ",";
                        break;
                    }
                    else if (strAArray[i - 1][j] == '-' && j == strAArray[i - 1].Length - 1)
                    {
                        strA1 = "-0,";
                        break;
                    }
                    else if (strAArray[i - 1][j] == '-' && j < strAArray[i - 1].Length - 1)
                    {
                        strA1 = strAArray[i - 1][j..] + ",";
                        break;
                    }
                }
                string strA = strA1 + strA3;
                double A;
                if (double.TryParse(strA, out A))
                {
                    result.Add(A);
                }
            }
            return result;
        }
    }
}
