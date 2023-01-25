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
using System.Windows.Forms;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Excel = Microsoft.Office.Interop.Excel;
using System.Xml.Serialization;
using System.Xml;
using CsvHelper;
using System.Globalization;
using System.Windows.Media;
using System.Reflection;
using Microsoft.Research.DynamicDataDisplay.Charts;

namespace SeismicDataAnalysis.ViewModel
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public ViewModelBase(MainWindow win)
        {
            _win = win;
            Model = new DataBase();
        }
        
        #region Реализация событийной модели для связи с Model
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyNane = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyNane));
        }
        #endregion

        #region Вспомогательные поля
        DataBase Model;
        private MainWindow _win;
        #endregion

        #region Связь с интерфейсом
        public ObservableCollection<FileData> transformedData
        {
            get { return Model.TransformedData; }
            set
            {
                Model.TransformedData = value;
                OnPropertyChanged();
            }
        }

        private FileData _selectedFile;
        public FileData SelectedFile 
        {
            get { return _selectedFile; } 
            set 
            { 
                _selectedFile = value;
                OnPropertyChanged();
            } 
        }

        public ObservableCollection<BuildingData> buildings
        {
            get { return Model.Buildings; }
            set
            {
                Model.Buildings = value;
                OnPropertyChanged();
            }
        }

        private BuildingData _selectedBuilding;
        public BuildingData SelectedBuilding
        {
            get { return _selectedBuilding; }
            set
            {
                _selectedBuilding = value;
                OnPropertyChanged();

            }
        }

        private EarthquakeData _selectedEarthquake;
        public EarthquakeData SelectedEarthquake
        {
            get { return _selectedEarthquake; }
            set
            {
                _selectedEarthquake = value;
                OnPropertyChanged();
                UpdateAnalysForSelected();
            }
        }

        private ChannelData _selectedChannel;
        public ChannelData SelectedChannel
        {
            get { return _selectedChannel; }
            set
            {
                _selectedChannel = value;
                OnPropertyChanged();
                if (value != null)
                {
                    DataVisualisationUtils.CreateCharts(_win.ChartPanel, value, SelectedDamping);
                    DataVisualisationUtils.CreateDataList(_win.DataPanel, value);
                }
            }
        }

        private double _selectedDamping;
        public double SelectedDamping
        {
            get { return _selectedDamping; }
            set
            {
                _selectedDamping = value;
                OnPropertyChanged();
                if (value != null)
                {
                    DataVisualisationUtils.CreateCharts(_win.ChartPanel, SelectedChannel, value);
                }
            }
        }

        private bool _compareNearestSensors;
        public bool CompareNearestSensors
        {
            get { return _compareNearestSensors; }
            set
            {
                _compareNearestSensors = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Методы
        /// <summary>
        /// Загружает и преобразует XML файлы
        /// </summary>
        public void LoadXML()
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedpath = folderDialog.SelectedPath;
                string[] fileNames = Directory.GetFiles(selectedpath, "*.xml*", SearchOption.AllDirectories);
                foreach (string file in fileNames)
                {
                    BuildingData bldg = DataTransformUtils.Deserialize(file);
                    Model.Buildings.Add(bldg);
                }
            }
        }

        /// <summary>
        /// Загружает и преобразует файлы Cosmos
        /// </summary>
        public void LoadCosmos()
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedpath = folderDialog.SelectedPath;
                string[] fileNames = Directory.GetFiles(selectedpath, "*.*", SearchOption.AllDirectories);

                List<string> newData = new List<string>();
                foreach (string path in fileNames)
                {
                    if (path.ToLower().EndsWith(".v2") || path.ToLower().EndsWith(".v3"))
                    {
                        using (StreamReader reader = new StreamReader(path))
                        {
                            string text = reader.ReadToEnd();
                            text = path.Split("\\", StringSplitOptions.None).Last() + ": File name__" + text;
                            newData.Add(text.ToLower());
                            Model.LoadedData.Add(text);
                        }
                    }
                }
                ObservableCollection<FileData> filesData = DataTransformUtils.TransformRawData(newData);
                foreach (FileData file in filesData)
                {
                    if (!DataTransformUtils.ClassCollectionContainsPropertyValue<FileData, string>(Model.TransformedData,
                    typeof(FileData).GetProperty("FileName"), file.FileName))
                    {
                        Model.TransformedData.Add(file);
                    }
                }

                ObservableCollection<ChannelData> channels = DataTransformUtils.FileCollectionToChannelCollection(Model.TransformedData);
                ObservableCollection<BuildingData> buffer = DataTransformUtils.CreateBuildingCollectionFromChannelCollection(channels);
                Model.Buildings.Clear();
                foreach (BuildingData bldg in buffer)
                {
                    Model.Buildings.Add(bldg);
                }
            }
        }

        /// <summary>
        /// Сохраняет выбранный график
        /// </summary>
        public void SaveChart()
        {
            //_win.AccelerationChart.SaveScreenshot(@"C:\Users\Евгений\Desktop\Chart.png");
        }

        /// <summary>
        /// Очищает список скачанных данных
        /// </summary>
        public void ClearData()
        {
            Model.LoadedData.Clear();
            Model.TransformedData.Clear();
            Model.Buildings.Clear();
        }

        /// <summary>
        /// Вспомогательный метод для проверки данных, может меняться в зависимости от нужной проверки
        /// </summary>
        public void CheckData()
        {
            
        }

        /// <summary>
        /// Перегруппировывает данные после корректировки имени здания
        /// </summary>
        public void ResortData()
        {
            ObservableCollection<ChannelData> newChannelData = new ObservableCollection<ChannelData>();
            foreach (BuildingData building in Model.Buildings)
            {
                foreach (EarthquakeData earthquake in building.EarthquakeData)
                {
                    foreach (ChannelData channel in earthquake.ChannelData)
                    {
                        channel.BuildingName = building.BuildingName;
                        channel.BuildingLength = building.BuildingLength;
                        channel.BuildingWidth = building.BuildingWidth;
                        newChannelData.Add(channel);
                    }
                }
            }
            //Исследуем все содержащиеся в файлах названия зданий
            List<string> chanBldgs = DataTransformUtils.GetUnicPropertyValues<ChannelData, string>(newChannelData.ToList(), typeof(FileData).GetProperty("BuildingName"));

            //Создаем список пустых файлов для данных по зданиям
            ObservableCollection<BuildingData> newbuildings = new ObservableCollection<BuildingData>();
            foreach (string name in chanBldgs)
            {
                BuildingData bldg = new BuildingData();
                bldg.BuildingName = name;
                newbuildings.Add(bldg);
            }
            //Заполняем классы по зданиям данными о землятресениях по датам
            foreach (BuildingData bldg in newbuildings)
            {
                List<ChannelData> bldgChans = newChannelData.Where(x => x.BuildingName == bldg.BuildingName).ToList();
                if (bldgChans.Count > 0)
                {
                    bldg.BuildingLength = bldgChans[0].BuildingLength;
                    bldg.BuildingWidth = bldgChans[0].BuildingWidth;
                }
                List<string> bldgDates = new List<string>();
                foreach (ChannelData chan in bldgChans)
                {
                    string chandate = chan.Month.ToString() + "/" + chan.Day.ToString() + "/" + chan.Year.ToString();
                    if (!bldgDates.Contains(chandate))
                    {
                        bldgDates.Add(chandate);
                    }
                }
                List<EarthquakeData> earthquakes = new List<EarthquakeData>();
                foreach (string date in bldgDates)
                {
                    EarthquakeData earthquake = new EarthquakeData();
                    earthquake.Date = date;
                    List<ChannelData> dateChans = bldgChans.Where(chan => chan.Month.ToString() + "/" + chan.Day.ToString() + "/" + chan.Year.ToString() == date).ToList();
                    ObservableCollection<ChannelData> earthquakeChans = new ObservableCollection<ChannelData>(dateChans);
                    earthquake.ChannelData = earthquakeChans;
                    earthquakes.Add(earthquake);
                }
                ObservableCollection<EarthquakeData> earthquakesData = new ObservableCollection<EarthquakeData>(earthquakes);
                bldg.EarthquakeData = earthquakesData;
            }
            //Заполняем координаты сенсоров везде где возможно
            foreach (BuildingData bldg in newbuildings)
            {
                foreach (EarthquakeData edata in bldg.EarthquakeData)
                {   
                    foreach(ChannelData chan in edata.ChannelData)
                    {
                        foreach (EarthquakeData edata2 in bldg.EarthquakeData)
                        {
                            foreach (ChannelData chan2 in edata2.ChannelData)
                            {
                                if (chan.ChannelNumber == chan2.ChannelNumber)
                                {
                                    if (chan.LocationHeight == "" && chan2.LocationHeight != "")
                                    {
                                        chan.LocationHeight = chan2.LocationHeight;
                                    }
                                    if (chan.LocationX == "" && chan2.LocationX != "")
                                    {
                                        chan.LocationX = chan2.LocationX;
                                    }
                                    if (chan.LocationY == "" && chan2.LocationY != "")
                                    {
                                        chan.LocationY = chan2.LocationY;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            buildings = newbuildings;
        }

        /// <summary>
        /// Преобразует данные землятресений в набор объектов из отклика спектра и высоты и экспортирует их в эксель
        /// </summary>
        public void ExportCSV()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Выберите папку для сохранения отчетов";
            folderBrowserDialog.ShowDialog();
            string path = folderBrowserDialog.SelectedPath;
            string filename = "Earthquake data.csv";
            //Создаем объекты для выгрузки в csv
            ObservableCollection<SpectrumObject> objects = new ObservableCollection<SpectrumObject>();
            objects = DataTransformUtils.CreateSpectrumObjects(Model, CompareNearestSensors);
            //Выгружаем в csv
            
            string fullpath = path + "\\" + filename;
            using (StreamWriter writer = new StreamWriter(fullpath))
            using (CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(objects);
            }
        }

        /// <summary>
        /// Сохраняет отредактированные данные для продолжения редактирования в другое время и более быстрой загрузки
        /// </summary>
        public void SaveData()
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (BuildingData bldg in Model.Buildings)
                {
                    string path = folderDialog.SelectedPath;
                    string fullpath = path + "\\" + "ED_" + bldg.BuildingName.Replace("\r\n", " ") + ".xml";
                    DataTransformUtils.Serialize(bldg, fullpath);
                }
            }
        }

        /// <summary>
        /// Выявляет зависимость амплитуды колебаний от высоты
        /// </summary>
        public void Analys()
        {
            _win.CommonAnalysPanel.Children.Clear();
            //Создание общего графика для всех зданий
            ChartPlotter plotter = DataVisualisationUtils.CreateChartPlotter("Spectrum acceleration, g", "Period", "Summary building corelation");
            _win.CommonAnalysPanel.Children.Add(plotter);
            List<SolidColorBrush> colors = new List<SolidColorBrush>();
            foreach (PropertyInfo prop in typeof(Brushes).GetProperties())
            {
                SolidColorBrush value = prop.GetValue(prop) as SolidColorBrush;
                colors.Add(value);
            }
            ObservableCollection<SpectrumObject> objects = DataTransformUtils.CreateSpectrumObjects(Model, CompareNearestSensors);
            foreach (BuildingData bldg in Model.Buildings)
            {
                List<SpectrumObject> objectsForBuilding = objects.Where(x => x.Name.Contains(bldg.BuildingName)).ToList();
                List<double[]> comparisons = DataTransformUtils.AmplitudeHeightComparison(objectsForBuilding);
                Random random = new Random();
                int colorNum = random.Next(0, colors.Count);
                MarkerPointsGraph graph = DataVisualisationUtils.CreateMarkerGraph(comparisons[1], comparisons[0], colors[colorNum], 1, bldg.BuildingName);
                plotter.Children.Add(graph);
            }
            //Создание графика со средним значением
            ChartPlotter meanPlotter = DataVisualisationUtils.CreateChartPlotter("Spectrum acceleration, g", "Period", "Mean corelation");
            _win.CommonAnalysPanel.Children.Add(meanPlotter);
            List<List<double>> data = new List<List<double>>();
            List<double[]> meanComparisons = DataTransformUtils.AmplitudeHeightComparison(objects.ToList());
            MarkerPointsGraph meanGraph = DataVisualisationUtils.CreateMarkerGraph(meanComparisons[1], meanComparisons[0], Brushes.Gray, 1, "Buildings");
            meanPlotter.Children.Add(meanGraph);
            double mean = DataTransformUtils.CalculateMean(meanComparisons[0].ToList());
            double[] meansPeriods = DataTransformUtils.CreateArrayFromMaxValue(0.01, meanComparisons[1].Max());
            double[] means = DataTransformUtils.CreateConstantArray(mean, meansPeriods.Length);
            LineGraph line = DataVisualisationUtils.CreateLineGraph(meansPeriods, means, Brushes.Red, 2, "Mean: " + mean.ToString());
            meanPlotter.Children.Add(line);
        }

        /// <summary>
        /// Обновляет график анализа для выбранных здания и землятресения
        /// </summary>
        private void UpdateAnalysForSelected()
        {
            _win.BuildingAnalysPanel.Children.Clear();
            ObservableCollection<SpectrumObject> objects = DataTransformUtils.CreateSpectrumObjects(Model, CompareNearestSensors);
            if (SelectedEarthquake != null)
            {
                foreach (double damp in SelectedEarthquake.ChannelData[0].Dampings)
                {
                    ChartPlotter meanPlotter = DataVisualisationUtils.CreateChartPlotter("Spectrum acceleration, g", "Period", "Data for " + damp + " damping");
                    List<double> data = new List<double>();
                    double maxPeriod = 0;
                    foreach (ChannelData chan in SelectedEarthquake.ChannelData)
                    {
                        if (chan.DampingsData.ContainsKey(damp))
                        {
                            double[] periodArray = DataTransformUtils.CreateArrayFromSpan(chan.Period, chan.DampingsData[damp]["Sa"].Count);
                            List<SpectrumObject> correctObjects = objects.Where(x =>
                                x.Name.Contains(SelectedBuilding.BuildingName) && x.Name.Contains(SelectedEarthquake.Date) &&
                                x.Name.Split('_')[2] == chan.ChannelNumber.ToString() && x.Damping == damp).ToList();
                            if (periodArray.Length > 0 && periodArray.Max() > maxPeriod)
                            {
                                maxPeriod = periodArray.Max();
                            }
                            List<double[]> comparisons = DataTransformUtils.AmplitudeHeightComparison(correctObjects);
                            foreach (double comp in comparisons[0])
                            {
                                data.Add(comp);
                            }
                            MarkerPointsGraph graph = DataVisualisationUtils.CreateMarkerGraph(comparisons[1], comparisons[0], Brushes.Gray, 1, "Spectrum acceleration");
                            meanPlotter.Children.Add(graph);
                        }
                    }
                    _win.BuildingAnalysPanel.Children.Add(meanPlotter);
                    double mean = DataTransformUtils.CalculateMean(data);
                    double[] meansPeriods = DataTransformUtils.CreateArrayFromMaxValue(0.01, maxPeriod);
                    double[] means = DataTransformUtils.CreateConstantArray(mean, meansPeriods.Length);
                    LineGraph line = DataVisualisationUtils.CreateLineGraph(meansPeriods, means, Brushes.Red, 2, "Mean: " + mean.ToString());
                    meanPlotter.Children.Add(line);
                }
            }
        }

        #endregion

        #region Команды
        private RelayCommand _loadXMLCommand;
        public RelayCommand LoadXMLCommand
        {
            get { return _loadXMLCommand ?? (_loadXMLCommand = new Model.RelayCommand(LoadXML)); }
        }

        private RelayCommand _loadCosmosCommand;
        public RelayCommand LoadCosmosCommand
        {
            get { return _loadCosmosCommand ?? (_loadCosmosCommand = new Model.RelayCommand(LoadCosmos)); }
        }

        private RelayCommand _saveChartCommand;
        public RelayCommand SaveChartCommand
        {
            get { return _saveChartCommand ?? (_saveChartCommand = new Model.RelayCommand(SaveChart)); }
        }

        private RelayCommand _clearDataCommand;
        public RelayCommand ClearDataCommand
        {
            get { return _clearDataCommand ?? (_clearDataCommand = new Model.RelayCommand(ClearData)); }
        }

        private RelayCommand _checkDataCommand;
        public RelayCommand CheckDataCommand
        {
            get { return _checkDataCommand ?? (_checkDataCommand = new Model.RelayCommand(CheckData)); }
        }

        private RelayCommand _exportCSVCommand;
        public RelayCommand ExportCSVCommand
        {
            get { return _exportCSVCommand ?? (_exportCSVCommand = new Model.RelayCommand(ExportCSV)); }
        }

        private RelayCommand _resortDataCommand;
        public RelayCommand ResortDataCommand
        {
            get { return _resortDataCommand ?? (_resortDataCommand = new Model.RelayCommand(ResortData)); }
        }

        private RelayCommand _saveDataCommand;
        public RelayCommand SaveDataCommand
        {
            get { return _saveDataCommand ?? (_saveDataCommand = new Model.RelayCommand(SaveData)); }
        }

        private RelayCommand _analysCommand;
        public RelayCommand AnalysCommand
        {
            get { return _analysCommand ?? (_analysCommand = new Model.RelayCommand(Analys)); }
        }
        #endregion
    }
}
