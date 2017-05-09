using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CGraph.Window
{
    /// <summary>
    /// Interaction logic for Plotter.xaml
    /// </summary>
    public partial class Graph
    {
        public Graph()
        {
            InitializeComponent();
        }

        private static double Evaluate(string exp)
        {
            var loDataTable = new DataTable();
            loDataTable.Columns.Add(new DataColumn("Eval", typeof (double), exp));
            loDataTable.Rows.Add(0);
            return (double) (loDataTable.Rows[0]["Eval"]);
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            Paint();
        }

        private void SetError(string erro = "")
        {
            if (ErrorLabel == null)
                return;
            ErrorLabel.Content = erro;
        }

        private List<Tuple<double, double>> GetDataSet(double from, double to, double def)
        {
            var expression = ExpressionTextBox.Text;

            if(string.IsNullOrWhiteSpace(expression))
                SetError("Empty expression");
            else
            {
                if (!expression.Contains("x"))
                    SetError("x not found");
                else
                {
                    var list = new List<Tuple<double, double>>();

                    try
                    {
                        for (var i = from; i <= to; i+= def)
                        {
                            var replaced = expression.Replace("x", i.ToString(CultureInfo.InvariantCulture));
                            var result = Evaluate(replaced);
                            list.Add(new Tuple<double, double>(i, result));
                        }
                    }
                    catch (Exception e)
                    {
                        SetError(e.Message);
                        return null;
                    }
                    return list;
                }
            }
            return null;
        }

        private void Paint()
        {
           
            SetError();
            double from = -10;
            double to = 10;
            double def = 1;
            if (From != null && !double.TryParse(From.Text, out from))
            {
                SetError("Cannot parse from");
                return;
            }
            if (To != null && !double.TryParse(To.Text, out to))
            {
                SetError("Cannot parse to");
                return;
            }
            if (Def != null && !double.TryParse(Def.Text, out def))
            {
                SetError("Cannot parse def");
                return;
            }

            if (from >= to)
            {
                SetError("from must be < to");
                return;
            }

            if (def <= 0)
            {
                SetError("def must be > 0");
                return;
            }

            var dataSet = GetDataSet(from, to, def);

            CanGraph.Children.Clear();

            var yaxisGeo = new GeometryGroup();
            var w = CanGraph.ActualWidth;
            var h = CanGraph.ActualHeight;
            if(w < 1 || h < 1)
                return;
            
            var maxX = (double)10;
            var minX = (double) -10;

            var maxY = (double)10;
            var minY = (double)-10;

            if (dataSet != null)
            {
                maxX = dataSet.Max(i => i.Item1);
                minX = dataSet.Min(i => i.Item1);
                maxY = dataSet.Max(i => i.Item2);
                minY = dataSet.Min(i => i.Item2);
            }

            //var h5P = (h * (4.5 / 100));
            var yh5P = ((h - 50) / (maxY + Math.Abs(minY)));
            var yh2P5P = yh5P/2;

            var xh5P = ((w - 50) / (maxX + Math.Abs(minX)));
            var xh2P5P = xh5P / 2;


            var zeroPoint = new Point(((Math.Abs(minX) * xh5P) + 20), ((Math.Abs(maxY) * yh5P) + 20));

            yaxisGeo.Children.Add(new LineGeometry(new Point(zeroPoint.X, 0), new Point(zeroPoint.X, h)));
            var yaxisPath = new Path { StrokeThickness = 1, Stroke = Brushes.Black, Data = yaxisGeo };

            for (var i = 1; i <= maxY; i ++)
            {
                var textBlock = new TextBlock { Text = i.ToString(), Foreground = new SolidColorBrush(Colors.Black), FontSize = yh2P5P };
                Canvas.SetTop(textBlock, (zeroPoint.Y - (i * yh5P)) - (yh2P5P / 2));
                Canvas.SetLeft(textBlock, zeroPoint.X + yh2P5P);
                CanGraph.Children.Add(textBlock);

                yaxisGeo.Children.Add(new LineGeometry(
                    new Point(zeroPoint.X - 5, zeroPoint.Y - (i * yh5P)),
                    new Point(zeroPoint.X + 5, zeroPoint.Y - (i * yh5P))));
            }


            for (var i = 1; i <= Math.Abs(minY); i ++)
            {
                var textBlock = new TextBlock {Text = (i * -1).ToString(), Foreground = new SolidColorBrush(Colors.Black), FontSize = yh2P5P };
                Canvas.SetTop(textBlock, (zeroPoint.Y + (Math.Abs(i) * yh5P)) - (yh2P5P / 2));
                Canvas.SetLeft(textBlock, zeroPoint.X + yh2P5P);
                CanGraph.Children.Add(textBlock);
                   
                yaxisGeo.Children.Add(new LineGeometry(
                    new Point(zeroPoint.X - 5, zeroPoint.Y + (Math.Abs(i) * yh5P)),
                    new Point(zeroPoint.X + 5, zeroPoint.Y +  (Math.Abs(i) * yh5P))));
            }


            var xaxisGeo = new GeometryGroup();
            xaxisGeo.Children.Add(new LineGeometry(new Point(0, zeroPoint.Y), new Point(w, zeroPoint.Y)));

            for (var i = 1; i <= Math.Abs(minX); i++)
            {
                var ibyh5P = i * xh5P;
                var textBlock = new TextBlock { Text = (i * -1).ToString(), Foreground = new SolidColorBrush(Colors.Black), FontSize = xh2P5P };
                Canvas.SetTop(textBlock, zeroPoint.Y - xh5P);
                Canvas.SetLeft(textBlock, zeroPoint.X - ibyh5P -(xh2P5P / 2));
                CanGraph.Children.Add(textBlock);

                xaxisGeo.Children.Add(new LineGeometry(
                    new Point(zeroPoint.X - ibyh5P, zeroPoint.Y - 5),
                    new Point(zeroPoint.X - ibyh5P, zeroPoint.Y + 5)));
            }

            for (var i = 1; i <= maxX; i++)
            {
                var ibyh5P = i * xh5P;
                var textBlock = new TextBlock { Text = i.ToString(), Foreground = new SolidColorBrush(Colors.Black), FontSize = xh2P5P };
                Canvas.SetTop(textBlock, zeroPoint.Y - xh5P);
                Canvas.SetLeft(textBlock, zeroPoint.X + ibyh5P - (xh2P5P / 2));
                CanGraph.Children.Add(textBlock);

                xaxisGeo.Children.Add(new LineGeometry(
                    new Point(zeroPoint.X + ibyh5P, zeroPoint.Y - 5),
                    new Point(zeroPoint.X + ibyh5P, zeroPoint.Y + 5)));
            }

            var xaxisPath = new Path { StrokeThickness = 1, Stroke = Brushes.Black, Data = xaxisGeo };


            CanGraph.Children.Add(yaxisPath);
            CanGraph.Children.Add(xaxisPath);

            var ylit = new TextBlock {Text = "y",Foreground = Brushes.CornflowerBlue, FontSize = 15};
            Canvas.SetTop(ylit, 10);
            Canvas.SetLeft(ylit, zeroPoint.X - 20);
            CanGraph.Children.Add(ylit);


            var xlit = new TextBlock { Text = "x", Foreground = Brushes.CornflowerBlue, FontSize = 15 };
            Canvas.SetTop(xlit, zeroPoint.Y);
            Canvas.SetLeft(xlit, 10);
            CanGraph.Children.Add(xlit);

            // x 
            for (var i = 1; i <= Math.Abs(minX); i++)
            {
                var x = zeroPoint.X + (-1*i*xh5P);
                var lineGeo = new LineGeometry(new Point(x, 0), new Point(x, w));
                var path = new Path {Stroke = Brushes.LightGray, StrokeThickness = 0.5, Data = lineGeo, ToolTip = x};
                CanGraph.Children.Add(path);
            }

            // x plus

            for (var i = 1; i <= maxX; i++)
            {
                var x = zeroPoint.X + (i * xh5P);
                var lineGeo = new LineGeometry(new Point(x, 0), new Point(x, w));
                var path = new Path { Stroke = Brushes.LightGray, StrokeThickness = 0.5, Data = lineGeo, ToolTip = x };
                CanGraph.Children.Add(path);
            }

            // y 
            for (var i = 1; i <= maxY; i++)
            {
                var y = zeroPoint.Y + (-1 * i * yh5P);
                var lineGeo = new LineGeometry(new Point(0, y), new Point(w, y));
                var path = new Path { Stroke = Brushes.LightGray, StrokeThickness = 0.5, Data = lineGeo, ToolTip = y };
                CanGraph.Children.Add(path);
            }

            // y minus

            for (var i = 1; i <= Math.Abs(minY); i++)
            {
                var y = zeroPoint.Y + ( i * yh5P);
                var lineGeo = new LineGeometry(new Point(0, y), new Point(w, y));
                var path = new Path { Stroke = Brushes.LightGray, StrokeThickness = 0.5, Data = lineGeo, ToolTip = y };
                CanGraph.Children.Add(path);
            }

            if(dataSet == null)
                return;

            Table.ItemsSource = new ObservableCollection<Tuple<double, double>>(dataSet);
            PaintLine(dataSet, yh5P,xh5P, zeroPoint);
            CalculateSlope(dataSet);
        }

        private Point TransformPoint(double x, double y, double yh5P, double xh5P, Point zeroPoint)
        {
            return new Point(zeroPoint.X + (xh5P * x), zeroPoint.Y + (yh5P * y * -1));
        }

        private Point PaintPoint(double x, double y, double yh5P, double xh5P, Point zeroPoint)
        {
            var point = TransformPoint(x, y, yh5P, xh5P, zeroPoint);
            var ec = new EllipseGeometry(point, 2.5, 2.5);
            var path = new Path {Fill = Brushes.Red, Data = ec, ToolTip = string.Format("{0}, {1}", x, y)};
            CanGraph.Children.Add(path);
            return point;
        }

        private void PaintLine(List<Tuple<double, double>> dataSet, double yh5P, double xh5P, Point zeroPoint )
        {
            var list = new PointCollection();
            foreach (var tuple in dataSet)
            {
                var point = PaintPoint(tuple.Item1, tuple.Item2, yh5P, xh5P, zeroPoint);
                list.Add(point);
            }

            var polyline = new Polyline { StrokeThickness = 1, Stroke = Brushes.Red, Points = list };
            CanGraph.Children.Add(polyline);
        }

        private void CalculateSlope(List<Tuple<double, double>> data)
        {
           if(data == null || data.Count < 2)
                return;

            var first = data[0];
            var second = data[1];

            var slope = (first.Item2 - second.Item2)/(first.Item1 - second.Item1);
            Slope.Content = string.Format("∆y/∆x = {0}", slope);

        }

        private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Paint();
        }

        private void OnExpressionChanged(object sender, TextChangedEventArgs e)
        {
            Paint();
        }

        private void OnCanvasMouseWheel(object sender, MouseWheelEventArgs e)
        {
            const double s = 1.1;
            if (e.Delta > 0)
            {
                CanScale.ScaleX *= s;
                CanScale.ScaleY *= s;
            }
            else
            {
                CanScale.ScaleX /= s;
                CanScale.ScaleY /= s;
            }
        }
        

        private void OnValueChange(object sender, TextChangedEventArgs e)
        {
            Paint();
        }
    }
}
