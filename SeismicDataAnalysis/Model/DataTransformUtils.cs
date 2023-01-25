using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.DataVisualization;
using System.Xml;
using System.Xml.Serialization;
using SeismicDataAnalysis;
using Algos;
using System.Reflection;

namespace SeismicDataAnalysis.Model
{
    public static class DataTransformUtils
    {
        public static FileData ParseRawData(string rawData)
        {
            FileData chan = new FileData();
            //Считываем тип физического параметра
            try
            {
                chan.PhysicalParameter = rawData[..24];
                //Считываем количество значений измерения ускорения
                int pointsANumberEndIndex = rawData.IndexOf(" points of accel data");
                int pointsANumberStartIndex = rawData[..pointsANumberEndIndex].LastIndexOf('\n') + 1;
                int pointsANumber;
                if (int.TryParse(rawData[pointsANumberStartIndex..pointsANumberEndIndex], out pointsANumber))
                {
                    chan.NumberOfAccelerationsPoints = pointsANumber;
                }
            }
            catch (Exception)
            {
                chan.NumberOfAccelerationsPoints = 0;
            }

            //Считываем количество значений измерения скорости
            try
            {
                int pointsVNumberEndIndex = rawData.IndexOf(" points of veloc data");
                int pointsVNumberStartIndex = rawData[..pointsVNumberEndIndex].LastIndexOf('\n') + 1;
                int pointsVNumber;
                if (int.TryParse(rawData[pointsVNumberStartIndex..pointsVNumberEndIndex], out pointsVNumber))
                {
                    chan.NumberOfVelocitysPoints = pointsVNumber;
                }
            }
            catch (Exception)
            {
                chan.NumberOfVelocitysPoints = 0;
            }

            //Считываем количество значений измерения распределения
            try
            {
                int pointsDNumberEndIndex = rawData.IndexOf(" points of displ data");
                int pointsDNumberStartIndex = rawData[..pointsDNumberEndIndex].LastIndexOf('\n') + 1;
                int pointsDNumber;
                if (int.TryParse(rawData[pointsDNumberStartIndex..pointsDNumberEndIndex], out pointsDNumber))
                {
                    chan.NumberOfDisplacementsPoints = pointsDNumber;
                }
            }
            catch (Exception)
            {
                chan.NumberOfDisplacementsPoints = 0;
            }

            //Считываем длительность измерения
            try
            {
                int recordSpaceStartIndex = rawData.IndexOf(" points of accel data") + 41;
                int recordSpaceEndIndex = rawData[recordSpaceStartIndex..].IndexOf(' ') + recordSpaceStartIndex;
                string strRecordSpace = "0" + rawData[recordSpaceStartIndex..recordSpaceEndIndex];
                //strRecordSpace = strRecordSpace.Replace('.', ',');
                double recordSpace;
                if (double.TryParse(strRecordSpace, out recordSpace))
                {
                    chan.SpaceOfRecord = recordSpace;
                }
            }
            catch (Exception)
            {
                chan.SpaceOfRecord = 0;
            }

            //Считываем номер станции
            try
            {
                int stationNumberStartIndex = rawData.IndexOf("station no.") + 12;
                int stationNumberEndIndex = rawData[stationNumberStartIndex..].IndexOf(' ') + stationNumberStartIndex;
                string strStationNumber = rawData[stationNumberStartIndex..stationNumberEndIndex];
                int stationNumber;
                if (int.TryParse(strStationNumber, out stationNumber))
                {
                    chan.StationNumber = stationNumber;
                }
            }
            catch (Exception)
            {
                chan.StationNumber = 0;
            }

            //Считываем дату и время записи
            try
            {
                if (rawData.Contains("time"))
                {
                    int monthSI = rawData.IndexOf("time");
                    while (rawData[monthSI] != '/')
                    {
                        monthSI++;
                    }
                    while (rawData[monthSI] != ' ')
                    {
                        monthSI--;
                    }
                    int monthEI = rawData[monthSI..].IndexOf('/') + monthSI;
                    string strmonth = rawData[monthSI..monthEI];
                    int month = 0;
                    if (int.TryParse(strmonth, out month))
                    {
                        chan.Month = month;
                    }

                    int daySI = monthEI + 1;
                    int dayEI = rawData[daySI..].IndexOf('/') + daySI;
                    string strday = rawData[daySI..dayEI];
                    int day = 0;
                    if (int.TryParse(strday, out day))
                    {
                        chan.Day = day;
                    }

                    int yearSI = dayEI + 1;
                    int yearEI = rawData[yearSI..].IndexOf(',') + yearSI;
                    string stryear = rawData[yearSI..yearEI];
                    int year = 0;
                    if (int.TryParse(stryear, out year))
                    {
                        chan.Year = year;
                    }
                }
                else
                {
                    int monthSI = rawData.IndexOf("origin");
                    while (rawData[monthSI] != '/')
                    {
                        monthSI++;
                    }
                    while (rawData[monthSI] != ' ')
                    {
                        monthSI--;
                    }
                    int monthEI = rawData[monthSI..].IndexOf('/') + monthSI;
                    string strmonth = rawData[monthSI..monthEI];
                    int month = 0;
                    if (int.TryParse(strmonth, out month))
                    {
                        chan.Month = month;
                    }

                    int daySI = monthEI + 1;
                    int dayEI = rawData[daySI..].IndexOf('/') + daySI;
                    string strday = rawData[daySI..dayEI];
                    int day = 0;
                    if (int.TryParse(strday, out day))
                    {
                        chan.Day = day;
                    }

                    int yearSI = dayEI + 1;
                    int yearEI = rawData[yearSI..].IndexOf(',') + yearSI;
                    string stryear = rawData[yearSI..yearEI];
                    int year = 0;
                    if (int.TryParse(stryear, out year))
                    {
                        chan.Year = year;
                    }
                }
                chan.FullDate = chan.Month.ToString() + "/" + chan.Day.ToString() + "/" + chan.Year.ToString();
            }
            catch (Exception)
            {

                chan.WeekDay = "DataError";
                chan.Month = 0;
                chan.Day = 0;
                chan.Year = 0;
                chan.Hour = 0;
                chan.Minute = 0;
                chan.Second = 0;
            }

            //Считываем название здания
            try
            {
                string[] bldgTypes = new string[] { "station", "bldg", "hotel", "hospital" };
                int bldgIndex1 = rawData.IndexOf("s/n");
                int bldgIndex = rawData[bldgIndex1..].IndexOf('\n') + 1 + bldgIndex1;
                foreach (string type in bldgTypes)
                {
                    if (rawData[bldgIndex..].Contains(type))
                    {
                        int bldgEI = rawData[bldgIndex..].IndexOf(type) + bldgIndex;
                        while (rawData[bldgEI] != ' ')
                        {
                            bldgEI++;
                        }
                        string bldgName = rawData[bldgIndex..bldgEI];
                        chan.BuildingName = bldgName;
                        break;
                    }
                }
            }
            catch (Exception)
            {
                chan.BuildingName = "DataError";
            }

            //Считываем данные измерений ускорений
            try
            {
                int measuresStartIndex1 = rawData.IndexOf("points of accel data equally spaced at");
                int measuresStartIndex = rawData[measuresStartIndex1..].IndexOf('\n') + 1 + measuresStartIndex1;
                int measuresEndIndex1 = rawData.IndexOf(" points of veloc data equally spaced at");
                int measuresEndIndex = rawData.Substring(0, measuresEndIndex1).LastIndexOf('\n');
                string strAccelerations = rawData[measuresStartIndex..measuresEndIndex];
                chan.AccelerationsArray = ParseStringToDoubleList(strAccelerations);
            }
            catch (Exception)
            {
                chan.AccelerationsArray = new List<double>();
            }

            //Считываем данные измерений скоростей
            try
            {
                int velocStartIndex1 = rawData.IndexOf(" points of veloc data equally spaced at");
                int velocStartIndex = rawData[velocStartIndex1..].IndexOf('\n') + velocStartIndex1 + 1;
                int velocEndIndex1 = rawData.IndexOf(" points of displ data equally spaced at");
                int velocEndIndex = rawData.Substring(0, velocEndIndex1).LastIndexOf('\n');
                string strVelocs = rawData[velocStartIndex..velocEndIndex];
                chan.VelocitysArray = ParseStringToDoubleList(strVelocs);
            }
            catch (Exception)
            {
                chan.VelocitysArray = new List<double>();
            }

            //Считываем данные распределения
            try
            {
                int dispStartIndex1 = rawData.IndexOf(" points of displ data equally spaced at");
                int dispStartIndex = rawData[dispStartIndex1..].IndexOf('\n') + dispStartIndex1 + 1;
                int dispEndIndex = rawData.IndexOf("&  ----------  end of data for channel") - 1;
                string strDisps = rawData[dispStartIndex..dispEndIndex];
                chan.DisplacementsArray = ParseStringToDoubleList(strDisps);
            }
            catch (Exception)
            {
                chan.DisplacementsArray = new List<double>();   
            }

            //Считываем номер канала
            try
            {
                int numStartIndex = rawData.IndexOf("chan") + 4;
                while (rawData[numStartIndex] == ' ')
                {
                    numStartIndex++;
                }
                int numEndIndex = rawData.Substring(numStartIndex+1).IndexOf(' ') + numStartIndex;
                string strNum = rawData[numStartIndex..numEndIndex];
                int num = 0;
                int.TryParse(strNum, out num);
                chan.ChannelNumber = num;
            }
            catch (Exception)
            {
                chan.ChannelNumber = 0;
            }

            //Считываем максимальное ускорение
            try
            {
                int maxAStartIndex1 = rawData.IndexOf("peak acceleration = ") + 20;
                int indA = 0;
                while (rawData[maxAStartIndex1 + indA] == ' ')
                {
                    indA++;
                }
                int maxAStartIndex = maxAStartIndex1 + indA;
                int maxAEndIndex = rawData[maxAStartIndex..].IndexOf(' ') + maxAStartIndex;
                string strMaxA = rawData[maxAStartIndex..maxAEndIndex];
                //strMaxA = strMaxA.Replace('.', ',');
                double maxA;
                if (double.TryParse(strMaxA, out maxA))
                {
                    chan.PeakAcceleration = maxA;
                }
            }
            catch (Exception)
            {
                chan.PeakAcceleration = 0;
            }

            //Считываем максимальную скорость
            try
            {
                int maxVStartIndex1 = rawData.IndexOf("peak   velocity   = ") + 20;
                int indV = 0;
                while (rawData[maxVStartIndex1 + indV] == ' ')
                {
                    indV++;
                }
                int maxVStartIndex = maxVStartIndex1 + indV;
                int maxVEndIndex = rawData[maxVStartIndex..].IndexOf(' ') + maxVStartIndex;
                string strMaxV = rawData[maxVStartIndex..maxVEndIndex];
                //strMaxV = strMaxV.Replace('.', ',');
                double maxV;
                if (double.TryParse(strMaxV, out maxV))
                {
                    chan.PeakVelocity = maxV;
                }
            }
            catch (Exception)
            {
                chan.PeakVelocity = 0;
            }

            //Считываем максимальное распределение
            try
            {
                
                int maxDStartIndex1 = rawData.IndexOf("peak displacement = ") + 20;
                int indD = 0;
                while (rawData[maxDStartIndex1 + indD] == ' ')
                {
                    indD++;
                }
                int maxDStartIndex = maxDStartIndex1 + indD;
                int maxDEndIndex = rawData[maxDStartIndex..].IndexOf(' ') + maxDStartIndex;
                string strMaxD = rawData[maxDStartIndex..maxDEndIndex];
                //strMaxD = strMaxD.Replace('.', ',');
                double maxD;
                if (double.TryParse(strMaxD, out maxD))
                {
                    chan.PeakDisplacement = maxD;
                }
            }
            catch (Exception)
            {
                chan.PeakDisplacement = 0;
            }

            //Считываем спектральные данные для разных затуханий
            try
            {
                
                string buffer = rawData;
                List<int> dampsStartIndexes = new List<int>();
                int lastI = 0;
                while (buffer.Contains("data of sd,sv,sa,pssv,ttsd,ttsv,ttsa :"))
                {
                    dampsStartIndexes.Add(buffer.IndexOf("data of sd,sv,sa,pssv,ttsd,ttsv,ttsa :") + 38 + lastI);
                    lastI = buffer.IndexOf("data of sd,sv,sa,pssv,ttsd,ttsv,ttsa :") + 38 + lastI;
                    buffer = buffer[(buffer.IndexOf("data of sd,sv,sa,pssv,ttsd,ttsv,ttsa :") + 38)..];
                }
                List<double> dampings = new List<double>();
                SerializableDictionary<double, SerializableDictionary<string, List<double>>> dampingsData = new SerializableDictionary<double, SerializableDictionary<string, List<double>>>();
                for (int i = 0; i < dampsStartIndexes.Count; i++)
                {
                    int dampSI = dampsStartIndexes[i];
                    int dampEI = 0;
                    if (i != dampsStartIndexes.Count - 1)
                    {
                        dampEI = dampsStartIndexes[i + 1] - 54;
                    }
                    else
                    {
                        dampEI = rawData.IndexOf("----------  end of spectral data for channel") - 5;
                    }
                    string strd1 = rawData[rawData[..dampSI].LastIndexOf('=')..dampSI];
                    //string strDamp = "0," + strd1.Split('.')[1];
                    string strDamp = "0." + strd1.Split('.')[1];
                    double damp = 0;
                    double.TryParse(strDamp, out damp);
                    dampings.Add(damp);
                    string[] splitted = rawData[dampSI..dampEI].Split('.');
                    int measuresCount = splitted[1..].Length / 7;
                    List<List<double>> dataList = new List<List<double>>();
                    for (int j = 0; j < 7; j++)
                    {
                        string[] splPart;
                        if (j == 0)
                        {
                            splPart = splitted[(j * measuresCount)..((j + 1) * measuresCount+1)];
                        }
                        else
                        {
                            splPart = splitted[(j * measuresCount)..((j + 1) * measuresCount+1)];
                        }
                        string strData = string.Join('.', splPart);
                        List<double> doubleData = ParseStringToDoubleList(strData);
                        dataList.Add(doubleData);
                    }
                    SerializableDictionary<string, List<double>> dampData = new SerializableDictionary<string, List<double>>();
                    dampData.Add("Sd", dataList[0]);
                    dampData.Add("Sv", dataList[1]);
                    dampData.Add("Sa", dataList[2]);
                    dampData.Add("Pssv", dataList[3]);
                    dampData.Add("ttSd", dataList[4]);
                    dampData.Add("ttSv", dataList[5]);
                    dampData.Add("ttSa", dataList[6]);
                    dampingsData.Add(damp, dampData);
                }
                chan.DampingsData = dampingsData;
                chan.Dampings = dampings;
            }
            catch (Exception)
            {

                chan.DampingsData = new SerializableDictionary<double, SerializableDictionary<string, List<double>>>();
            }

            //Считываем амплитуду Фурье
            try
            {
                
                int FSI = rawData.IndexOf("fourier amplitude spectra in in/sec.") + 37;
                if (rawData.IndexOf("fourier amplitude spectra in in/sec.") != -1)
                {
                    int FEI = rawData[FSI..].IndexOf("damping") + FSI - 1;
                    string strF = rawData[FSI..FEI];
                    chan.FourierAmplitudeArray = ParseStringToDoubleList(strF);
                }
                else
                {
                    chan.FourierAmplitudeArray = new List<double>();
                }
            }
            catch (Exception)
            {
                chan.FourierAmplitudeArray = new List<double>();
            }

            //Считываем период
            try
            {
                int periodSI = rawData.IndexOf("instr period =") + 15;
                periodSI = rawData[periodSI..].IndexOf('.') + periodSI;
                int periodEI = rawData[periodSI..].IndexOf(' ') + periodSI;
                //string strPeriod = ("0" + rawData[periodSI..periodEI]).Replace('.', ',');
                string strPeriod = ("0" + rawData[periodSI..periodEI]);
                double period = 0;
                double.TryParse(strPeriod, out period);
                chan.Period = period;
            }
            catch (Exception)
            {
                chan.Period = 0;
            }

            //Считываем начальную скорость
            try
            {
                
                int ivSI = rawData.IndexOf("initial velocity");
                while (rawData[ivSI] != '.')
                {
                    ivSI++;
                }
                int ivEI = rawData[ivSI..].IndexOf("cm") - 1 + ivSI;
                while (ivEI == ' ')
                {
                    ivEI--;
                }
                ivEI = ivEI - 1;
                string strIV = rawData[ivSI..ivEI];
                double iv = 0;
                if (double.TryParse(strIV, out iv))
                {
                    chan.InitialVelocity = iv;
                }
            }
            catch (Exception)
            {
                chan.InitialVelocity = 0;
            }

            //Считываем начальное смещение
            try
            {
                int idSI = rawData.IndexOf("initial displacement");
                while (rawData[idSI] != '.')
                {
                    idSI++;
                }
                int idEI = rawData[idSI..].IndexOf("cm") - 1 + idSI;
                while (idEI == ' ')
                {
                    idEI--;
                }
                idEI = idEI - 1;
                string strID = rawData[idSI..idEI];
                double id = 0;
                if (double.TryParse(strID, out id))
                {
                    chan.InitialDisplacement = id;
                }
            }
            catch (Exception)
            {
                chan.InitialDisplacement = 0;
            }

            //Считываем чувствительность
            try
            {
                int sensSI = rawData.IndexOf("sensitivity");
                while (rawData[sensSI] != '.')
                {
                    sensSI++;
                }
                while (rawData[sensSI] != ' ')
                {
                    sensSI--;
                }
                int sensEI = rawData[sensSI..].IndexOf("v/g") - 1 + sensSI;
                while (sensEI == ' ')
                {
                    sensEI--;
                }
                string strSens = rawData[sensSI..sensEI];
                double sens = 0;
                if (double.TryParse(strSens, out sens))
                {
                    chan.Sensitivity= sens;
                }
            }
            catch (Exception)
            {
                chan.Sensitivity = 0;
            }
            
            //Считываем положение
            try
            {
                int locSI = rawData.IndexOf("location") + 9;
                while (rawData[locSI] != ' ')
                {
                    locSI++;
                }
                locSI = locSI + 1;
                int locEI = rawData[locSI..].IndexOf("  ") + locSI;
                string location = rawData[locSI..locEI];
                chan.Location = location;
            }
            catch (Exception)
            {
                chan.Location = "";
            }

            //Считываем координаты станции
            try
            {
                int latitudeSI = rawData.IndexOf("station no.") + 11;
                while (rawData[latitudeSI] != '.')
                {
                    latitudeSI++;
                }
                while (rawData[latitudeSI] != ' ')
                {
                    latitudeSI--;
                }
                latitudeSI++;
                int latitudeEI = rawData[latitudeSI..].IndexOf(',') + latitudeSI;
                string latitude = rawData[latitudeSI..latitudeEI];
                int longitudeSI = latitudeEI + 2;
                while (rawData[longitudeSI] == ' ')
                {
                    longitudeSI++;
                }
                int longitudeEI = rawData[longitudeSI..].IndexOf(' ') + longitudeSI;
                string longitude = rawData[longitudeSI..longitudeEI];
                chan.StationLatitude = latitude;
                chan.StationLongitude = longitude;
            }
            catch (Exception)
            {
                chan.StationLatitude = "";
                chan.StationLongitude = "";
            }

            //Считываем длину записи
            try
            {
                int recSI = rawData.IndexOf("record length");
                while (rawData[recSI] != '.')
                {
                    recSI++;
                }
                while (rawData[recSI] != ' ')
                {
                    recSI--;
                }
                recSI = recSI + 1;
                int recEI = rawData[recSI..].IndexOf("sec") + recSI;
                while (recEI == ' ')
                {
                    recEI--;
                }
                recEI = recEI - 1;
                //string strRec = rawData[recSI..recEI].Replace('.', ',');
                string strRec = rawData[recSI..recEI];
                double rec = 0;
                if (double.TryParse(strRec, out rec))
                {
                    chan.RecordLength = rec;
                }
            }
            catch (Exception)
            {
                chan.RecordLength = 0;
            }

            //Считываем затухание
            try
            {
                int dampSI = rawData.IndexOf("damping =") + 9;
                dampSI = rawData[dampSI..].IndexOf('.') + dampSI;
                int dampEI = rawData[dampSI..].IndexOf(',') + dampSI;
                //string strDamp = ("0" + rawData[dampSI..dampEI]).Replace('.', ',');
                string strDamp = ("0" + rawData[dampSI..dampEI]);
                double damp = 0;
                double.TryParse(strDamp, out damp);
                chan.Damping = damp;
            }
            catch (Exception)
            {
                chan.Damping = 0;
            }

            //Считываем UncorMax
            try
            {
                int uncSI = rawData.IndexOf("uncor max") + 9;
                uncSI = rawData[uncSI..].IndexOf('.') + uncSI;
                while (rawData[uncSI] != ' ')
                {
                    uncSI--;
                }
                uncSI++;
                int uncEI = rawData[uncSI..].IndexOf(' ') + uncSI;
                string strUnc = rawData[uncSI..uncEI];
                double unc = ParseStringToDoubleList(strUnc)[0];
                chan.UncorMax = unc;
            }
            catch (Exception)
            {
                chan.UncorMax = 0;
            }

            //Считываем направление
            try
            {
                int orSI1 = rawData.IndexOf("chan");
                int orSI = rawData[orSI1..].IndexOf(":") + orSI1 + 1;
                while (rawData[orSI] == ' ')
                {
                    orSI++;
                }
                int orEI = rawData[orSI..].IndexOf(' ') + orSI;
                string or = rawData[orSI..orEI];
                chan.Orientation = or;
            }
            catch (Exception)
            {

                chan.Orientation = "";
            }

            return chan;
        }

        public static ObservableCollection<FileData> TransformRawData(List<string> rawData)
        {
            ObservableCollection<FileData> filesData = new ObservableCollection<FileData>();
            for (int i = 0; i < rawData.Count; i++)
            {
                string splittingConstant = "";
                if (rawData[i].Contains("end of data for channel"))
                {
                    splittingConstant = "end of data for channel";
                }
                else if (rawData[i].Contains("end of spectral data for channel"))
                {
                    splittingConstant = "end of spectral data for channel";
                }

                List<string> splittedData = rawData[i].Split(splittingConstant).ToList();
                for (int j = 0; j < splittedData.Count-1; j++)
                {
                    int charsCountAfterEndOfData = 35;
                    splittedData[j] = splittedData[j] + splittingConstant + splittedData[j + 1][..charsCountAfterEndOfData];
                }

                foreach (string part in splittedData)
                {
                    FileData channelData = DataTransformUtils.ParseRawData(part);
                    filesData.Add(channelData);
                }
            }
            return filesData;
        }

        public static bool ClassCollectionContainsPropertyValue<T, V>(ICollection<T> collection, PropertyInfo property, V value)
        {
            bool contains = false;
            foreach (T item in collection)
            {
                if (EqualityComparer<V>.Default.Equals((V)(property.GetValue(item)), value))
                {
                    contains = true;
                    break;
                }
            }
            return contains;
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

        public static ObservableCollection<ChannelData> FileCollectionToChannelCollection(ObservableCollection<FileData> transformedData)
        {
            List<int> chanNums = GetUnicPropertyValues<FileData, int>(transformedData.ToList(), typeof(FileData).GetProperty("ChannelNumber"));
            List<string> chanDates = GetUnicPropertyValues<FileData, string>(transformedData.ToList(), typeof(FileData).GetProperty("FullDate"));
            List<string> chanBldgs = GetUnicPropertyValues<FileData, string>(transformedData.ToList(), typeof(FileData).GetProperty("BuildingName"));

            ObservableCollection<ChannelData> channels = new ObservableCollection<ChannelData>();
            foreach (string bldgName in chanBldgs)
            {
                List<FileData> buildingChannelFiles = transformedData.Where(x => x.BuildingName == bldgName).ToList();
                foreach (string date in chanDates)
                {
                    List<FileData> buildingDateChannelFiles = buildingChannelFiles.Where(x => x.FullDate == date).ToList();
                    foreach (int chanNum in chanNums)
                    {
                        List<FileData> channelFiles = buildingDateChannelFiles.Where(x => x.ChannelNumber == chanNum).ToList();
                        if (channelFiles.Count != 0)
                        {
                            ChannelData channel = new ChannelData();
                            channel.ChannelNumber = channelFiles[0].ChannelNumber;
                            foreach (FileData file in channelFiles)
                            {
                                foreach (PropertyInfo prop in typeof(FileData).GetProperties())
                                {
                                    if (prop.PropertyType == typeof(List<double>))
                                    {
                                        List<double> fileListValue = (List<double>)prop.GetValue(file);
                                        if (fileListValue.Count != 0)
                                        {
                                            PropertyInfo chanProperty = typeof(FileData).GetProperty(prop.Name);
                                            chanProperty.SetValue(channelFiles, fileListValue);
                                        }
                                    }
                                    if (prop.PropertyType == typeof(double))
                                    {
                                        double fileDoubleValue = (double)prop.GetValue(file);
                                        if (fileDoubleValue != 0)
                                        {
                                            PropertyInfo chanProperty = typeof(FileData).GetProperty(prop.Name);
                                            chanProperty.SetValue(channelFiles, fileDoubleValue);
                                        }
                                    }
                                    if (prop.PropertyType == typeof(int))
                                    {
                                        int fileIntValue = (int)prop.GetValue(file);
                                        if (fileIntValue != 0)
                                        {
                                            PropertyInfo chanProperty = typeof(FileData).GetProperty(prop.Name);
                                            chanProperty.SetValue(channelFiles, fileIntValue);
                                        }
                                    }
                                    if (prop.PropertyType == typeof(string))
                                    {
                                        string fileStringValue = (string)prop.GetValue(file);
                                        if (fileStringValue != "")
                                        {
                                            PropertyInfo chanProperty = typeof(FileData).GetProperty(prop.Name);
                                            chanProperty.SetValue(channelFiles, fileStringValue);
                                        }
                                    }
                                    if (prop.PropertyType == typeof(SerializableDictionary<double, SerializableDictionary<string, List<double>>>))
                                    {
                                        SerializableDictionary<double, SerializableDictionary<string, List<double>>> fileDictValue =
                                            (SerializableDictionary<double, SerializableDictionary<string, List<double>>>)prop.GetValue(file);
                                        if (fileDictValue.Count != 0)
                                        {
                                            PropertyInfo chanProperty = typeof(FileData).GetProperty(prop.Name);
                                            chanProperty.SetValue(channelFiles, fileDictValue);
                                        }
                                    }
                                }
                            }
                            channels.Add(channel);
                        }
                    }
                }
            }

            return channels;
        }

        public static ObservableCollection<BuildingData> CreateBuildingCollectionFromChannelCollection(ObservableCollection<ChannelData> channels)
        {
            List<int> chanNums = GetUnicPropertyValues<ChannelData, int>(channels.ToList(), typeof(FileData).GetProperty("ChannelNumber"));
            List<string> chanDates = GetUnicPropertyValues<ChannelData, string>(channels.ToList(), typeof(FileData).GetProperty("FullDate"));
            List<string> chanBldgs = GetUnicPropertyValues<ChannelData, string>(channels.ToList(), typeof(FileData).GetProperty("BuildingName"));

            ObservableCollection<BuildingData> buildings = new ObservableCollection<BuildingData>();
            foreach (string buildingName in chanBldgs)
            {
                BuildingData building = new BuildingData();
                building.BuildingName = buildingName;
                List<EarthquakeData> earthquakes = new List<EarthquakeData>();
                foreach (string date in chanDates)
                {
                    EarthquakeData earthquake = new EarthquakeData();
                    earthquake.Date = date;
                    List<ChannelData> buildingDateChannels = channels.Where(x => x.BuildingName == buildingName && x.FullDate == date).ToList();
                    if (buildingDateChannels.Count != 0)
                    {
                        earthquake.ChannelData = new ObservableCollection<ChannelData>(buildingDateChannels);
                        earthquakes.Add(earthquake);
                    }
                }
                building.EarthquakeData = new ObservableCollection<EarthquakeData>(earthquakes);
            }
            return buildings;
        }

        public static List<double> ParseStringToDoubleList(string strAccelerations)
        {
            List<double> result = new List<double>();   
            string[] strAArray = strAccelerations.Split('.');
            for (int i = 1; i < strAArray.Length; i++)
            {
                int degree = 0;
                string strA3 = "";
                for (int j = 0; j < strAArray[i].Length; j++)
                {
                    if (strAArray[i][j] == ' ')
                    {
                        strA3 = strAArray[i][0..strAArray[i].IndexOf(' ')];
                        break;
                    }
                    else if (strAArray[i][j] == 'e')
                    {
                        strA3 = strAArray[i][0..strAArray[i].IndexOf('e')];
                        if (i != strAArray.Length-1)
                        {
                            int.TryParse(strAArray[i][(strAArray[i].IndexOf('e') + 1)..strAArray[i].IndexOf(' ')], out degree);
                        }
                        else
                        {
                            int.TryParse(strAArray[i][(strAArray[i].IndexOf('e') + 1)..], out degree);
                        }
                        break;
                    }
                    else if (strAArray[i][j] == '-')
                    {
                        strA3 = strAArray[i][0..strAArray[i].IndexOf('-')];
                        break;
                    }
                    else if (j == strAArray[i].Length-1)
                    {
                        strA3 = strAArray[i];
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
                strA = strA.Replace(',', '.');
                double A;
                if (double.TryParse(strA, out A))
                {
                    result.Add(A*Math.Pow(10, degree));
                }
            }
            return result;
        }

        public static void Serialize(BuildingData result, string file)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(BuildingData));
            string xml;
            System.Xml.XmlWriterSettings writerSets = new System.Xml.XmlWriterSettings();
            writerSets.NewLineOnAttributes = true;
            writerSets.Indent = true;
            writerSets.ConformanceLevel = ConformanceLevel.Auto;
            writerSets.OmitXmlDeclaration = true;
            using (XmlWriter writer = XmlWriter.Create(file, writerSets))
            {
                xmlSerializer.Serialize(writer, result);
            }
        }

        public static BuildingData Deserialize(string file)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(BuildingData));
            BuildingData result = new BuildingData();
            if (File.Exists(file))
            {
                try
                {
                    using (XmlReader reader = XmlReader.Create(file))
                    {
                        result = (BuildingData)xmlSerializer.Deserialize(reader);
                    }
                }
                catch (Exception)
                {

                }
            }
            return result;
        }

        public static ObservableCollection<SpectrumObject> CreateSpectrumObjects(DataBase Model, bool CompareNearestSensors)
        {
            ObservableCollection<SpectrumObject> objects = new ObservableCollection<SpectrumObject>();
            foreach (BuildingData building in Model.Buildings)
            {
                foreach (EarthquakeData earthquake in building.EarthquakeData)
                {
                    foreach (ChannelData channel in earthquake.ChannelData)
                    {
                        double height = 0;
                        double.TryParse(channel.LocationHeight, out height);
                        if (height != 0)
                        {
                            ChannelData minChan = new ChannelData();
                            if (CompareNearestSensors)
                            {
                                //Подбираем минимальный по высоте и ближайший в плоскости сенсор с таким же направлением измерения 
                                double minHeight = 1000000000;
                                double minX = 100000000;
                                double minY = 100000000;
                                foreach (ChannelData chan in earthquake.ChannelData)
                                {
                                    double buf = 0;
                                    double.TryParse(chan.LocationHeight, out buf);
                                    double buf11 = 0;
                                    double.TryParse(chan.LocationX, out buf11);
                                    double buf12 = 0;
                                    double.TryParse(channel.LocationX, out buf12);
                                    double buf1 = Math.Abs(buf11 - buf12);
                                    double buf21 = 0;
                                    double.TryParse(chan.LocationX, out buf21);
                                    double buf22 = 0;
                                    double.TryParse(channel.LocationX, out buf22);
                                    double buf2 = Math.Abs(buf21 - buf22);
                                    if (buf != 0 && buf <= minHeight && buf1 <= minX && buf2 <= minY && chan.Orientation == channel.Orientation)
                                    {
                                        minHeight = buf;
                                        minX = buf1;
                                        minY = buf2;
                                        minChan = chan;
                                    }
                                }
                            }
                            else
                            {
                                //Подбираем минимальный по высоте сенсор с таким же направлением измерения
                                double minHeight = 1000000000;
                                foreach (ChannelData chan in earthquake.ChannelData)
                                {
                                    double buf = 0;
                                    double.TryParse(chan.LocationHeight, out buf);
                                    if (buf != 0 && buf < minHeight && chan.Orientation == channel.Orientation)
                                    {
                                        minHeight = buf;
                                        minChan = chan;
                                    }
                                }
                            }
                            foreach (double damp in channel.Dampings)
                            {
                                for (int i = 0; i < channel.DampingsData[damp]["Sa"].Count; i++)
                                {
                                    SpectrumObject obj = new SpectrumObject();
                                    string name = building.BuildingName + "_" + earthquake.Date + "_" + channel.ChannelNumber.ToString();
                                    obj.Name = name;
                                    obj.Period = channel.Period * i;
                                    obj.Damping = damp;
                                    obj.Acceleration = channel.DampingsData[damp]["Sa"][i];
                                    double buf = 0;
                                    double.TryParse(channel.LocationHeight, out buf);
                                    double bufX = 0;
                                    double.TryParse(channel.LocationX, out bufX);
                                    double bufY = 0;
                                    double.TryParse(channel.LocationX, out bufY);
                                    double buf2 = 0;
                                    double.TryParse(minChan.LocationHeight, out buf2);
                                    double buf3 = 0;
                                    double.TryParse(minChan.LocationX, out buf3);
                                    double buf4 = 0;
                                    double.TryParse(minChan.LocationY, out buf4);
                                    obj.dX = bufX - buf3;
                                    obj.dY = bufY - buf4;
                                    obj.dZ = buf - buf2;
                                    obj.Z2divZ1 = buf2 / buf;
                                    obj.BaseAcceleration = minChan.DampingsData[damp]["Sa"][i];
                                    double bufLength = 0;
                                    double.TryParse(channel.BuildingLength, out bufLength);
                                    obj.BldgLength = bufLength;
                                    double bufWidth = 0;
                                    double.TryParse(channel.BuildingWidth, out bufWidth);
                                    obj.BldgWidth = bufWidth;
                                    double orientation = 0;
                                    if (double.TryParse(channel.Orientation, out orientation))
                                    {
                                        double strRef = 0;
                                        if (orientation == 360)
                                        {
                                            orientation = 0;
                                        }
                                        if (double.TryParse(building.StructuralReference, out strRef))
                                        {
                                            double dRef = orientation + strRef;
                                            if (dRef > 180)
                                            {
                                                dRef = Math.Abs(360 - dRef);
                                                obj.dRef = dRef;
                                            }
                                            else
                                            {
                                                obj.dRef = dRef;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        obj.dRef = 0;
                                    }
                                    double bufMag = 0;
                                    double.TryParse(earthquake.Magnitude, out bufMag);
                                    obj.Magnitude = bufMag;
                                    double bufDepth = 0;
                                    double.TryParse(earthquake.HypocenterDepth, out bufDepth);
                                    obj.HypocenterDepth = bufDepth;
                                    objects.Add(obj);
                                }
                            }
                        }
                    }
                }
            }
            return objects;
        }

        public static List<double[]> AmplitudeHeightComparison(List<SpectrumObject> objects)
        {
            List<double> comparisons = new List<double>();
            List<double> periods = new List<double>();
            foreach (SpectrumObject obj in objects)
            {
                if (obj.Z2divZ1 != 1 && obj.BaseAcceleration != 0)
                {
                    double comparison = obj.Acceleration / obj.BaseAcceleration / obj.Z2divZ1;
                    comparisons.Add(comparison);
                    periods.Add(obj.Period);
                }
            }
            List<double[]> result = new List<double[]>();
            result.Add(comparisons.ToArray());
            result.Add(periods.ToArray());
            return result;
        }

        public static double CalculateMean(List<double> data, double accuracy = 0.95)
        {
            double mean = 0.0;
            List<double> sortedData = data;
            Sorts.InsertSort(sortedData);
            int newLength = (int)Math.Round(sortedData.Count * accuracy, 0);
            sortedData = sortedData.GetRange(0, newLength);
            foreach (double item in sortedData)
            {
                mean = mean + item;
            }
            mean = mean / sortedData.Count;
            return mean;
        }

        public static double[] CreateConstantArray(double value, int count)
        {
            List<double> data = new List<double>();
            for (int i = 0; i < count; i++)
            {
                data.Add(value);
            }
            return data.ToArray();
        }

        public static double[] CreateArrayFromMaxValue(double span, double maxValue)
        {
            List<double> result = new List<double>();
            int count = (int)Math.Round(maxValue / span);
            for (int i = 0; i < count; i++)
            {
                result.Add(span * i);
            }
            return result.ToArray();
        }

        public static List<P> GetUnicPropertyValues<T, P>(List<T> list, PropertyInfo property)
        {
            if (list[0].GetType().GetProperty(property.Name) == null)
            {
                return null;
            }
            List<P> propertyValues = new List<P>();
            foreach (T item in list)
            {
                if (!propertyValues.Contains((P)(property.GetValue(item))))
                {
                    propertyValues.Add((P)(property.GetValue(item)));
                }
            }
            return propertyValues;
        }
    }
}
