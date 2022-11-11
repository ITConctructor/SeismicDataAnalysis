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

        /// <summary>
        /// Добавляет линейный график в двумерное пространство
        /// </summary>
        /// <param name="plotter">Двумерное пространство</param>
        /// <param name="yArray">Массив значений по y</param>
        /// <param name="xArray">Массив значений по x</param>
        public static void AddLineGraph(ChartPlotter plotter, double[] yArray, double[] xArray, string yTitle="Y", string xTitle="X", string header="Chart")
        {
            List<Point> points = new List<Point>();
            for (int i = 0; i < xArray.Length; i++)
            {
                Point point = new Point(xArray[i], yArray[i]);
                points.Add(point);
            }
            ClearLines(plotter);
            IPointDataSource _eds = null;
            EnumerableDataSource<Point> _edsSPP = new EnumerableDataSource<Point>(points);
            _edsSPP.SetXMapping(p => p.X);
            _edsSPP.SetYMapping(p => p.Y);
            _eds = _edsSPP;
            LineGraph line = new LineGraph(_eds);
            line.LinePen = new Pen(Brushes.Black, 2);
            plotter.Children.Add(line);
            VerticalAxisTitle YTitle = new VerticalAxisTitle();
            YTitle.Content = yTitle;
            HorizontalAxisTitle XTitle = new HorizontalAxisTitle();
            XTitle.Content = xTitle;
            Header _header = new Header();
            _header.Content = header;
            plotter.Children.Add(YTitle);
            plotter.Children.Add(XTitle);
            plotter.Children.Add(_header);
            plotter.Viewport.FitToView();
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

        /// <summary>
        /// На указанной StackPanel построить графики для выбранного файла
        /// </summary>
        /// <param name="panel">Панель для построения</param>
        public static void UpdateCharts(StackPanel panel, ChannelData chan)
        {
            //Контейнер для графиков
            panel.Children.Clear();
            //Контейнер для описания канала
            StackPanel textPanel = new StackPanel();
            textPanel.Orientation = Orientation.Vertical;
            //Название и номер станции
            TextBlock staInfo = new TextBlock();
            string staInfoText = chan.StationName + "     CGS Sta " + chan.StationNumber.ToString();
            staInfo.Text = staInfoText;
            staInfo.HorizontalAlignment = HorizontalAlignment.Center;
            staInfo.VerticalAlignment = VerticalAlignment.Center;
            staInfo.FontSize = 20;
            textPanel.Children.Add(staInfo);
            //Дата и время записи
            TextBlock time = new TextBlock();
            string timeText = "Rcrd of " + chan.WeekDay + " " + chan.Month + " " + chan.Day.ToString() + ", " + chan.Year.ToString() + " "
                + chan.Hour.ToString() + ":" + chan.Minute.ToString() + ":" + chan.Second.ToString();
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
            ChartPlotter plotterA = new ChartPlotter();
            panel.Children.Add(plotterA);
            string maxAText = "Peak acceleration = " + chan.PeakAcceleration.ToString() + " cm/sec2";
            AddChartDescription(plotterA, chan.AccelerationsArray, chan.NumberOfAccelerationsPoints, maxAText);
            //List<double> yAH = new List<double>();
            //foreach (double y in yA)
            //{
            //    yAH.Add(y * Math.Sqrt(Math.Pow(chan.PeakAcceleration, 2)));
            //}
            AddLineGraph(plotterA, yA.ToArray(), xA.ToArray(), "Accelerations, cm/sec2", "Time, sec", "Accelerations");

            //График скорости
            double[] yV = chan.VelocitysArray.ToArray();
            double[] xV = DataTransformUtils.CreateArrayFromSpan(chan.SpaceOfRecord, yV.Length);
            ChartPlotter plotterV = new ChartPlotter();
            panel.Children.Add(plotterV);
            string maxVText = "Peak velocity = " + chan.PeakVelocity.ToString() + " cm/sec";
            AddChartDescription(plotterV, chan.VelocitysArray, chan.NumberOfVelocitysPoints, maxVText);
            //List<double> yVH = new List<double>();
            //foreach (double y in yV)
            //{
            //    yVH.Add(y * Math.Sqrt(Math.Pow(chan.PeakVelocity, 2)));
            //}
            AddLineGraph(plotterV, yV.ToArray(), xV.ToArray(), "Velocitys, cm/sec", "Time, sec", "Velocitys");

            //График распределения
            double[] yD = chan.VelocitysArray.ToArray();
            double[] xD = DataTransformUtils.CreateArrayFromSpan(chan.SpaceOfRecord, yD.Length);
            ChartPlotter plotterD = new ChartPlotter();
            panel.Children.Add(plotterD);
            string maxDText = "Peak displacement = " + chan.PeakVelocity.ToString() + " cm";
            AddChartDescription(plotterD, chan.DisplacementsArray, chan.NumberOfDisplacementsPoints, maxDText);
            //List<double> yDH = new List<double>();
            //foreach (double y in yD)
            //{
            //    yDH.Add(y * Math.Sqrt(Math.Pow(chan.PeakDisplacement, 2)));
            //}
            AddLineGraph(plotterD, yD.ToArray(), xD.ToArray(), "Displacements, cm/sec", "Time, sec", "Displacements");
        }
    }
}
