using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TspAlgorithms
{
    public class TspGraph
    {
        public List<(int, PointF)> Nodes { get; set; }

        public TspGraph(List<(int, PointF)> Nodes)
        {
            this.Nodes = Nodes;
        }

        public List<(int, PointF)> GetRandomPermutation()
        {
            List<(int, PointF)> nodesPermutation = new List<(int, PointF)>(Nodes);

            for (int i = 0; i < nodesPermutation.Count; i++)
            {
                Random random = new Random();
                int index = random.Next(nodesPermutation.Count);
                (int, PointF) temp = nodesPermutation[index];
                nodesPermutation[index] = nodesPermutation[i];
                nodesPermutation[i] = temp;
            }

            return Nodes;
        }

        public float GetPathLength()
        {
            float length = 0;
            for (int i = 0; i < Nodes.Count - 1; i++)
                length += MathF.Sqrt( MathF.Pow(Nodes[i].Item2.X - Nodes[i + 1].Item2.X, 2) + MathF.Pow(Nodes[i].Item2.Y - Nodes[i + 1].Item2.Y, 2));
            length += MathF.Sqrt(MathF.Pow(Nodes[0].Item2.X - Nodes[Nodes.Count-1].Item2.X, 2) + MathF.Pow(Nodes[0].Item2.Y - Nodes[Nodes.Count - 1].Item2.Y, 2));
            return length;
        }
    }
}
