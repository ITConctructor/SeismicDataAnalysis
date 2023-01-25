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
using System.Windows.Media;
using System.Windows.Controls;
using System.Reflection;
using Microsoft.Research.DynamicDataDisplay.Charts;

namespace SeismicDataAnalysis.ViewModel
{
    public static class DataVisualisationUtils
    {
        /// <summary>
        /// Очищает двумерное пространство от линейных графиков
        /// </summary>
        /// <param name="plotter">Двумерное пространство</param>
        public static void ClearLines(ChartPlotter plotter)
        {
            var lgc = new Collection<IPlotterElement>();
            foreach (var x in plotter.Children)
            {
                if (x is LineGraph || x is ElementMarkerPointsGraph)
                    lgc.Add(x);
            }

            foreach (var x in lgc)
            {
                plotter.Children.Remove(x);
            }
        }

        /// <summary>
        /// Очищает шапку двумерного пространства
        /// </summary>
        /// <param name="plotter">Двумерное пространство</param>
        public static void ClearTopPanel(ChartPlotter plotter)
        {
            List<UIElement> children = new List<UIElement>();
            foreach (UIElement x in plotter.TopPanel.Children)
            {
                children.Add(x);
            }

            foreach (UIElement x in children)
            {
                plotter.TopPanel.Children.Remove(x);
            }
        }

        public static ChartPlotter CreateChartPlotter(string yTitle, string xTitle, string header)
        {
            ChartPlotter plotter = new ChartPlotter();
            VerticalAxisTitle YTitle = new VerticalAxisTitle();
            YTitle.Content = yTitle;
            HorizontalAxisTitle XTitle = new HorizontalAxisTitle();
            XTitle.Content = xTitle;
            Header _header = new Header();
            _header.Content = header;
            plotter.Children.Add(YTitle);
            plotter.Children.Add(XTitle);
            plotter.Children.Add(_header);
            return plotter;
        }

        public static LineGraph CreateLineGraph(double[] xArray, double[] yArray, SolidColorBrush color, double thickness = 1, string legend = "Data")
        {
            List<Point> points = new List<Point>();
            for (int i = 0; i < xArray.Length; i++)
            {
                Point point = new Point(xArray[i], yArray[i]);
                points.Add(point);
            }
            IPointDataSource _eds = null;
            EnumerableDataSource<Point> _edsSPP = new EnumerableDataSource<Point>(points);
            _edsSPP.SetXMapping(p => p.X);
            _edsSPP.SetYMapping(p => p.Y);
            _eds = _edsSPP;
            LineGraph line = new LineGraph(_eds);
            line.LinePen = new Pen(color, thickness);
            Legend.SetDescription(line, legend);
            return line;
        }

        public static MarkerPointsGraph CreateMarkerGraph(double[] xArray, double[] yArray, SolidColorBrush color, double thickness = 1, string legend = "Data")
        {
            List<Point> points = new List<Point>();
            for (int i = 0; i < xArray.Length; i++)
            {
                Point point = new Point(xArray[i], yArray[i]);
                points.Add(point);
            }
            IPointDataSource _eds = null;
            EnumerableDataSource<Point> _edsSPP = new EnumerableDataSource<Point>(points);
            _edsSPP.SetXMapping(p => p.X);
            _edsSPP.SetYMapping(p => p.Y);
            _eds = _edsSPP;
            MarkerPointsGraph graph = new MarkerPointsGraph(_eds);
            Microsoft.Research.DynamicDataDisplay.PointMarkers.CirclePointMarker marker = new Microsoft.Research.DynamicDataDisplay.PointMarkers.CirclePointMarker();
            marker.Pen = new Pen(color, thickness);
            marker.Fill = color;
            marker.Size = thickness;
            graph.Marker = marker;
            Legend.SetDescription(graph, legend);
            return graph;
        }

        /// <summary>
        /// Добавляет к графику описание из файла
        /// </summary>
        /// <param name="plotter">Двумерное пространство графика</param>
        /// <param name="chan">Данные из файла</param>
        public static void AddChartDescription(ChartPlotter plotter, List<double> yArray, int xNum, string maxText)
        {
            ClearTopPanel(plotter);
            //Контейнер для строк
            StackPanel textPanel = new StackPanel();
            textPanel.Orientation = Orientation.Vertical;
            //Количество считанных точек
            TextBlock quality = new TextBlock();
            string qualityText = "Points read: " + yArray.Count.ToString() + " of " + xNum;
            quality.Text = qualityText;
            quality.HorizontalAlignment = HorizontalAlignment.Left;
            quality.VerticalAlignment = VerticalAlignment.Center;
            quality.FontSize = 10;
            textPanel.Children.Add(quality);
            //Максимальное значение
            TextBlock max = new TextBlock();
            max.Text = maxText;
            max.HorizontalAlignment = HorizontalAlignment.Left;
            max.VerticalAlignment = VerticalAlignment.Center;
            max.FontSize = 10;
            textPanel.Children.Add(max);
            plotter.TopPanel.Children.Add(textPanel);
        }

        public static StackPanel CreateChartHeaderPanel(List<string> textList)
        {
            StackPanel textPanel = new StackPanel();
            textPanel.Name = "Description";
            textPanel.Orientation = Orientation.Vertical;
            foreach (string text in textList)
            {
                TextBlock textBlock = new TextBlock();
                textBlock.Text = text;
                textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                textBlock.VerticalAlignment = VerticalAlignment.Center;
                textBlock.FontSize = 20;
                textPanel.Children.Add(textBlock);
            }
            return textPanel;
        }

        /// <summary>
        /// На указанной StackPanel построить графики для выбранного файла
        /// </summary>
        /// <param name="panel">Панель для построения</param>
        public static void CreateCharts(StackPanel panel, ChannelData chan, double damp)
        {
            panel.Children.Clear();
            //Контейнер для описания канала
            StackPanel textPanel = new StackPanel();
            textPanel.Name = "Description";
            textPanel.Orientation = Orientation.Vertical;
            //Название и номер станции
            TextBlock staInfo = new TextBlock();
            string staInfoText = chan.BuildingName + "     CGS Sta " + chan.StationNumber.ToString();
            staInfo.Text = staInfoText;
            staInfo.HorizontalAlignment = HorizontalAlignment.Center;
            staInfo.VerticalAlignment = VerticalAlignment.Center;
            staInfo.FontSize = 20;
            textPanel.Children.Add(staInfo);
            //Дата и время записи
            TextBlock time = new TextBlock();
            string timeText = "Rcrd of " + chan.Month.ToString() + "/" + chan.Day.ToString() + "/" + chan.Year.ToString();
            time.Text = timeText;
            time.HorizontalAlignment = HorizontalAlignment.Center;
            time.VerticalAlignment = VerticalAlignment.Center;
            time.FontSize = 20;
            textPanel.Children.Add(time);

            //Добавляем текстовое описание канала
            panel.Children.Add(textPanel);
            //Добавляем графики

            //График ускорения
            double[] yA = chan.AccelerationsArray.ToArray();
            double[] xA = DataTransformUtils.CreateArrayFromSpan(chan.SpaceOfRecord, yA.Length);
            if (yA.Length == 0)
            {
                TextBlock error = new TextBlock();
                error.Name = "AccError";
                error.Text = "No acceleration data";
                error.HorizontalAlignment = HorizontalAlignment.Center;
                error.VerticalAlignment = VerticalAlignment.Center;
                error.FontSize = 20;
                error.Margin = new Thickness(10);
                panel.Children.Add(error);
            }
            else
            {
                LineGraph lineGraph = DataVisualisationUtils.CreateLineGraph(xA, yA, Brushes.Black, 1, "Acceleration");
                ChartPlotter plotterA = DataVisualisationUtils.CreateChartPlotter("Accelerations, cm/sec2", "Time, sec", "Accelerations");
                plotterA.Children.Add(lineGraph);
                plotterA.Name = "AccPlotter";
                panel.Children.Add(plotterA);
                string maxAText = "Peak acceleration = " + chan.PeakAcceleration.ToString() + " cm/sec2";
                AddChartDescription(plotterA, chan.AccelerationsArray, chan.NumberOfAccelerationsPoints, maxAText);
            }

            //График скорости
            double[] yV = chan.VelocitysArray.ToArray();
            double[] xV = DataTransformUtils.CreateArrayFromSpan(chan.SpaceOfRecord, yV.Length);
            if (yV.Length == 0)
            {
                TextBlock error = new TextBlock();
                error.Name = "VelocError";
                error.Text = "No velocity data";
                error.HorizontalAlignment = HorizontalAlignment.Center;
                error.VerticalAlignment = VerticalAlignment.Center;
                error.FontSize = 20;
                error.Margin = new Thickness(10);
                panel.Children.Add(error);

            }
            else
            {
                LineGraph lineGraph = DataVisualisationUtils.CreateLineGraph(xV, yV, Brushes.Black, 1, "Velocity");
                ChartPlotter plotterV = DataVisualisationUtils.CreateChartPlotter("Velocitys, cm/sec", "Time, sec", "Velocitys");
                plotterV.Children.Add(lineGraph);
                plotterV.Name = "VelocPlotter";
                panel.Children.Add(plotterV);
                string maxVText = "Peak velocity = " + chan.PeakVelocity.ToString() + " cm/sec";
                AddChartDescription(plotterV, chan.VelocitysArray, chan.NumberOfVelocitysPoints, maxVText);
            }

            //График распределения
            double[] yD = chan.DisplacementsArray.ToArray();
            double[] xD = DataTransformUtils.CreateArrayFromSpan(chan.SpaceOfRecord, yD.Length);
            if (yD.Length == 0)
            {
                TextBlock error = new TextBlock();
                error.Name = "DispError";
                error.Text = "No displacement data";
                error.HorizontalAlignment = HorizontalAlignment.Center;
                error.VerticalAlignment = VerticalAlignment.Center;
                error.FontSize = 20;
                error.Margin = new Thickness(10);
                panel.Children.Add(error);
            }
            else
            {
                LineGraph lineGraph = DataVisualisationUtils.CreateLineGraph(xD, yD, Brushes.Black, 1, "Displacement");
                ChartPlotter plotterD = DataVisualisationUtils.CreateChartPlotter("Displacements, cm/sec", "Time, sec", "Displacements");
                plotterD.Children.Add(lineGraph);
                plotterD.Name = "DispPlotter";
                panel.Children.Add(plotterD);
                string maxDText = "Peak displacement = " + chan.PeakDisplacement.ToString() + " cm";
                AddChartDescription(plotterD, chan.DisplacementsArray, chan.NumberOfDisplacementsPoints, maxDText);
            }

            //Добавляем график амплитуды Фурье
            double[] yF = chan.FourierAmplitudeArray.ToArray();
            double[] xF = DataTransformUtils.CreateArrayFromSpan(chan.Period, yF.Length);
            if (yF.Length == 0)
            {
                TextBlock error = new TextBlock();
                error.Name = "FourierError";
                error.Text = "No Fourier amplitude data";
                error.HorizontalAlignment = HorizontalAlignment.Center;
                error.VerticalAlignment = VerticalAlignment.Center;
                error.FontSize = 20;
                error.Margin = new Thickness(10);
                panel.Children.Add(error);
            }
            else
            {
                LineGraph lineGraph = DataVisualisationUtils.CreateLineGraph(xF, yF, Brushes.Black, 1, "Amplitude");
                ChartPlotter plotterF = DataVisualisationUtils.CreateChartPlotter("Amplitude, in", "Period, sec", "Fourier amplitude spectra");
                plotterF.Children.Add(lineGraph);
                plotterF.Name = "FourierPlotter";
                panel.Children.Add(plotterF);
            }

            try
            {
                //Добавляем графики для указанного значения демфирования
                StackPanel specPanel = new StackPanel();
                //График Sd
                double[] ySd = chan.DampingsData[damp]["Sd"].ToArray();
                double[] xSd = DataTransformUtils.CreateArrayFromSpan(chan.Period, ySd.Length);
                LineGraph lineGraphSd = DataVisualisationUtils.CreateLineGraph(ySd, xSd, Brushes.Black, 1, "Sd");
                ChartPlotter plotterSd = DataVisualisationUtils.CreateChartPlotter("Sd (in)", "Period (sec)", "Sd");
                plotterSd.Children.Add(lineGraphSd);
                specPanel.Children.Add(plotterSd);
                //График Sv
                double[] ySv = chan.DampingsData[damp]["Sv"].ToArray();
                double[] xSv = DataTransformUtils.CreateArrayFromSpan(chan.Period, ySv.Length);
                LineGraph lineGraphSv = DataVisualisationUtils.CreateLineGraph(ySv, xSv, Brushes.Black, 1, "Sv");
                ChartPlotter plotterSv = DataVisualisationUtils.CreateChartPlotter("Sv (in/sec)", "Period (sec)", "Sv");
                plotterSv.Children.Add(lineGraphSv);
                specPanel.Children.Add(plotterSv);
                //График Sa
                double[] ySa = chan.DampingsData[damp]["Sa"].ToArray();
                double[] xSa = DataTransformUtils.CreateArrayFromSpan(chan.Period, ySa.Length);
                LineGraph lineGraphSa = DataVisualisationUtils.CreateLineGraph(ySa, xSa, Brushes.Black, 1, "Sa");
                ChartPlotter plotterSa = DataVisualisationUtils.CreateChartPlotter("Sa (g)", "Period (sec)", "Sa");
                plotterSa.Children.Add(lineGraphSa);
                specPanel.Children.Add(plotterSa);
                //График Pssv
                double[] yPssv = chan.DampingsData[damp]["Pssv"].ToArray();
                double[] xPssv = DataTransformUtils.CreateArrayFromSpan(chan.Period, yPssv.Length);
                LineGraph lineGraphPssv = DataVisualisationUtils.CreateLineGraph(yPssv, xPssv, Brushes.Black, 1, "Pssv");
                ChartPlotter plotterPssv = DataVisualisationUtils.CreateChartPlotter("Pssv (in)", "Period (sec)", "Pssv");
                plotterPssv.Children.Add(lineGraphPssv);
                specPanel.Children.Add(plotterPssv);
                //График ttSd
                double[] yttSd = chan.DampingsData[damp]["ttSd"].ToArray();
                double[] xttSd = DataTransformUtils.CreateArrayFromSpan(chan.Period, yttSd.Length);
                LineGraph lineGraphttSd = DataVisualisationUtils.CreateLineGraph(yttSd, xttSd, Brushes.Black, 1, "ttSd");
                ChartPlotter plotterttSd = DataVisualisationUtils.CreateChartPlotter("ttSd (in)", "Period (sec)", "ttSd");
                plotterttSd.Children.Add(lineGraphttSd);
                specPanel.Children.Add(plotterttSd);
                //График ttSv
                double[] yttSv = chan.DampingsData[damp]["ttSv"].ToArray();
                double[] xttSv = DataTransformUtils.CreateArrayFromSpan(chan.Period, yttSv.Length);
                LineGraph lineGraphttSv = DataVisualisationUtils.CreateLineGraph(yttSv, xttSv, Brushes.Black, 1, "ttSv");
                ChartPlotter plotterttSv = DataVisualisationUtils.CreateChartPlotter("ttSv (in/sec)", "Period (sec)", "ttSv");
                plotterttSv.Children.Add(lineGraphttSv);
                specPanel.Children.Add(plotterttSv);
                //График ttSa
                double[] yttSa = chan.DampingsData[damp]["ttSa"].ToArray();
                double[] xttSa = DataTransformUtils.CreateArrayFromSpan(chan.Period, yttSa.Length);
                LineGraph lineGraphttSa = DataVisualisationUtils.CreateLineGraph(yttSa, xttSa, Brushes.Black, 1, "ttSa");
                ChartPlotter plotterttSa = DataVisualisationUtils.CreateChartPlotter("ttSa (g)", "Period (sec)", "ttSa");
                plotterttSa.Children.Add(lineGraphttSa);
                specPanel.Children.Add(plotterttSa);
                panel.Children.Add(specPanel);
            }
            catch (Exception)
            {
                TextBlock error = new TextBlock();
                error.Name = "SpecError";
                error.Text = "No spectral data";
                error.HorizontalAlignment = HorizontalAlignment.Center;
                error.VerticalAlignment = VerticalAlignment.Center;
                error.FontSize = 20;
                error.Margin = new Thickness(10);
                panel.Children.Add(error);
            }
        }

        /// <summary>
        /// Создаем список данных канала на панели
        /// </summary>
        /// <param name="panel">Панель</param>
        /// <param name="chan">Канал</param>
        public static void CreateDataList(StackPanel panel, ChannelData chan)
        {
            panel.Children.Clear();
            foreach (PropertyInfo property in typeof(ChannelData).GetProperties())
            {
                if (property.Name != "BuildingName" && property.Name != "Location" && property.Name != "LocationHeight" && 
                    property.Name != "LocationX" && property.Name != "LocationY" && property.Name != "BuildingLength" &&
                    property.Name != "BuildingWidth")
                {
                    TextBlock textBlock = new TextBlock();
                    string name = property.Name;
                    string value = "";
                    if (property.PropertyType == typeof(double) ||
                        property.PropertyType == typeof(int) ||
                        property.PropertyType == typeof(string))
                    {
                        value = property.GetValue(chan).ToString();
                    }
                    else if (property.PropertyType == typeof(List<double>))
                    {
                        value = ((List<double>)property.GetValue(chan)).Count.ToString();
                    }
                    else if (property.Name == "DampingsData")
                    {
                        value = ((SerializableDictionary<double, SerializableDictionary<string, List<double>>>)property.GetValue(chan)).Count.ToString();
                    }
                    textBlock.Text = name + ": " + value;
                    panel.Children.Add(textBlock);
                }
                
            }
        }

    }
}
