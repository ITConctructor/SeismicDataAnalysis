using Microsoft.Win32;
using SeismicDataAnalysis.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeismicDataAnalysis.ViewModel
{
    public class ViewModelBase
    {
        public ViewModelBase(MainWindow win)
        {
            _win = win;
        }

        private MainWindow _win;

        #region Методы
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
                        DataBase.LoadedData.Add(text);
                    }
                }
            }

            //Преобразуем строковые данные из списка LoadedData в класс ChannelData и записываем их в список TransformedData
            foreach (string rawStr in DataBase.LoadedData)
            {
                ChannelData channelData = DataTransformUtils.TransformRawData(rawStr);
                DataBase.TransformedData.Add(channelData);
            }
        }
        #endregion

        #region Команды
        private Model.RelayCommand _loadDataCommand;
        public Model.RelayCommand LoadDataCommand
        {
            get { return _loadDataCommand ?? (_loadDataCommand = new Model.RelayCommand(LoadData)); }
        }
        #endregion
    }
}
