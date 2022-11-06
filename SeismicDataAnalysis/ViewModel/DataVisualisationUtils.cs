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
        public static void AddLineGraph(ChartPlotter plotter, double[] yArray, double[] xArray)
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
            plotter.Viewport.FitToView();
        }

        /// <summary>
        /// Добавляет к графику описание из файла
        /// </summary>
        /// <param name="plotter">Двумерное пространство графика</param>
        /// <param name="chan">Данные из файла</param>
        public static void AddChannelDescription(ChartPlotter plotter, ChannelData chan)
        {
            ClearTopPanel(plotter);
            //Контейнер для строк
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Vertical;
            //Название и номер станции
            TextBlock staInfo = new TextBlock();
            string staInfoText = chan.StationName + "     CGS Sta " + chan.StationNumber.ToString();
            staInfo.Text = staInfoText;
            staInfo.HorizontalAlignment = HorizontalAlignment.Center;
            staInfo.VerticalAlignment = VerticalAlignment.Center;
            staInfo.FontSize = 20;
            panel.Children.Add(staInfo);
            //Дата и время записи
            TextBlock time = new TextBlock();
            string timeText = "Rcrd of " + chan.WeekDay + " " + chan.Month + " " + chan.Day.ToString() + ", " + chan.Year.ToString() + " "
                + chan.Hour.ToString() + ":" + chan.Minute.ToString() + ":" + chan.Second.ToString();
            time.Text = timeText;
            time.HorizontalAlignment = HorizontalAlignment.Center;
            time.VerticalAlignment = VerticalAlignment.Center;
            time.FontSize = 20;
            panel.Children.Add(time);
            //Измеряемый параметр
            TextBlock parameter = new TextBlock();
            string parameterText = chan.PhysicalParameter;
            parameter.Text = parameterText;
            parameter.HorizontalAlignment = HorizontalAlignment.Center;
            parameter.VerticalAlignment = VerticalAlignment.Center;
            parameter.FontSize = 20;
            panel.Children.Add(parameter);
            plotter.TopPanel.Children.Add(panel);
        }
    }
}
