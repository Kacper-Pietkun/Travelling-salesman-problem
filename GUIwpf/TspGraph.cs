using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GUIwpf
{
    public class TspGraph
    {
        public List<PointF> Nodes { get; set; }

        public TspGraph(string pathToFile)
        {
            TspFileReader tspFileReader = new TspFileReader();
            Nodes = tspFileReader.ReadFile(pathToFile);
        }

        public List<PointF> GetRandomPermutation()
        {
            List<PointF> nodesPermutation = new List<PointF>(Nodes);

            for (int i = 0; i < nodesPermutation.Count; i++)
            {
                Random random = new Random();
                int index = random.Next(nodesPermutation.Count);
                PointF temp = nodesPermutation[index];
                nodesPermutation[index] = nodesPermutation[i];
                nodesPermutation[i] = temp;
            }

            return Nodes;
        }

        public void Draw(Canvas canvas)
        {
            canvas.Children.Clear();
            float delta = 20;
            float maxWidth = (float) canvas.ActualWidth - delta;
            float maxHeight = (float) canvas.ActualHeight - delta;
            float maxPointX = float.MinValue;
            float minPointX = float.MaxValue;
            float maxPointY = float.MinValue;
            float minPointY = float.MaxValue;
            foreach (PointF point in Nodes)
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
            List<PointF> scaledNodes = new List<PointF>(Nodes);
            for (int i = 0; i < scaledNodes.Count; i++)
                scaledNodes[i] = new PointF((scaledNodes[i].X - minPointX) / (maxPointX - minPointX) * maxWidth + delta / 2,
                    (scaledNodes[i].Y - minPointY) / (maxPointY - minPointY) * maxHeight + delta / 2);

            DrawEdges(canvas, scaledNodes);
            DrawNodes(canvas, scaledNodes);
        }

        private void DrawNodes(Canvas canvas, List<PointF> scaledNodes)
        {
            foreach (PointF point in scaledNodes)
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

        private void DrawEdges(Canvas canvas, List<PointF> scaledNodes)
        {
            for (int i = 0; i < scaledNodes.Count; i++)
            {
                Line edge = new Line
                {
                    X1 = scaledNodes[i].X,
                    Y1 = scaledNodes[i].Y,
                    X2 = scaledNodes[(i + 1) % scaledNodes.Count].X,
                    Y2 = scaledNodes[(i + 1) % scaledNodes.Count].Y,
                    Stroke = Brushes.Black,
                };
                canvas.Children.Add(edge);
            }
        }

    }
}
