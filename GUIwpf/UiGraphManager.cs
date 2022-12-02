using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;
using TspAlgorithms;

namespace GUIwpf
{
    public class UiGraphManager
    {
        private Canvas canvas;
        public UiGraphManager(Canvas _canvas)
        {
            canvas = _canvas;
        }

        public void Draw(TspGraph graph)
        {
            if (graph == null)
                return;
            canvas.Children.Clear();
            float delta = 20;
            float maxWidth = (float)canvas.ActualWidth - delta;
            float maxHeight = (float)canvas.ActualHeight - delta;
            float maxPointX = float.MinValue;
            float minPointX = float.MaxValue;
            float maxPointY = float.MinValue;
            float minPointY = float.MaxValue;
            foreach ((int index, PointF point) in graph.Nodes)
            {
                if (point.X > maxPointX)
                    maxPointX = point.X;
                if (point.X < minPointX)
                    minPointX = point.X;
                if (point.Y > maxPointY)
                    maxPointY = point.Y;
                if (point.Y < minPointY)
                    minPointY = point.Y;
            }
            List<(int, PointF)> scaledNodes = new List<(int, PointF)>(graph.Nodes);
            for (int i = 0; i < scaledNodes.Count; i++)
                scaledNodes[i] = (scaledNodes[i].Item1, new PointF((scaledNodes[i].Item2.X - minPointX) / (maxPointX - minPointX) * maxWidth + delta / 2,
                    (scaledNodes[i].Item2.Y - minPointY) / (maxPointY - minPointY) * maxHeight + delta / 2));

            DrawEdges(canvas, scaledNodes);
            DrawNodes(canvas, scaledNodes);
        }

        private void DrawNodes(Canvas canvas, List<(int, PointF)> scaledNodes)
        {
            foreach ((int index, PointF point) in scaledNodes)
            {
                Ellipse node = new Ellipse
                {
                    Width = 7,
                    Height = 7,
                    Fill = Brushes.Red,
                    StrokeThickness = 1,
                    Stroke = Brushes.Black
                };
                Canvas.SetLeft(node, point.X - node.Width / 2);
                Canvas.SetTop(node, point.Y - node.Height / 2);
                canvas.Children.Add(node);
            }
        }

        private void DrawEdges(Canvas canvas, List<(int, PointF)> scaledNodes)
        {
            for (int i = 0; i < scaledNodes.Count; i++)
            {
                Line edge = new Line
                {
                    X1 = scaledNodes[i].Item2.X,
                    Y1 = scaledNodes[i].Item2.Y,
                    X2 = scaledNodes[(i + 1) % scaledNodes.Count].Item2.X,
                    Y2 = scaledNodes[(i + 1) % scaledNodes.Count].Item2.Y,
                    Stroke = Brushes.Black,
                };
                canvas.Children.Add(edge);
            }
        }
    }
}
