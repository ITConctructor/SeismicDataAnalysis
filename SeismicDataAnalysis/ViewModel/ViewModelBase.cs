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
        public ObservableCollection<ChannelData> transformedData
        {
            get { return Model.TransformedData; }
            set
            {
                Model.TransformedData = value;
                OnPropertyChanged();
            }
        }
        
        private ChannelData _selectedFile;
        public ChannelData SelectedFile 
        {
            get { return _selectedFile; } 
            set 
            { 
                _selectedFile = value;
                OnPropertyChanged();
                DataVisualisationUtils.AddLineGraph(_win.AccelerationChart, 
                    value.AccelerationsArray.ToArray(), 
                    DataTransformUtils.CreateArrayFromSpan(value.SpaceOfRecord, value.AccelerationsArray.Count));
                DataVisualisationUtils.AddChannelDescription(_win.AccelerationChart, value);
            } 
        }

        public ObservableCollection<Point> AccelerationData = new ObservableCollection<Point>();

        #endregion

        #region Методы
        /// <summary>
        /// Скачивает и трансформирует данные в класс
        /// </summary>
        public void LoadData()
        {
            //Показываем диалоговое окно, которое позволяет выбрать несколько файлов типа v2 или v3
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            //Считываем данные из файла в виде одной строки и записываем эти данные в класс DataBase
            List<string> data = new List<string>(); 
            if(openFileDialog.ShowDialog() == true)
            {
                string[] FileNames = openFileDialog.FileNames;
                foreach (string path in FileNames)
                {
                    using (StreamReader reader = new StreamReader(path))
                    {
                        string text = reader.ReadToEnd();
                        Model.LoadedData.Add(text);
                    }
                }
                //Преобразуем строковые данные из списка LoadedData в класс ChannelData и записываем их в список TransformedData
                for (int i = 0; i < Model.LoadedData.Count; i++)
                {
                    ChannelData channelData = DataTransformUtils.TransformRawData(Model.LoadedData[i]);
                    channelData.FileName = FileNames[i].Split("\\", StringSplitOptions.None).Last();
                    Model.TransformedData.Add(channelData);
                }
            }
        }

        /// <summary>
        /// Сохраняет выбранный график
        /// </summary>
        public void SaveChart()
        {
            _win.AccelerationChart.SaveScreenshot(@"C:\Users\Евгений\Desktop\Chart.png");
        }
        #endregion

        #region Команды
        private RelayCommand _loadDataCommand;
        public RelayCommand LoadDataCommand
        {
            get { return _loadDataCommand ?? (_loadDataCommand = new Model.RelayCommand(LoadData)); }
        }

        private RelayCommand _saveChartCommand;
        public RelayCommand SaveChartCommand
        {
            get { return _saveChartCommand ?? (_loadDataCommand = new Model.RelayCommand(SaveChart)); }
        }
        #endregion
    }
}
