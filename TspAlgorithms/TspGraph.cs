﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TspAlgorithms
{
    [Serializable]
    public class TspGraph : ISerializable
    {
        public List<(int, PointF)> Nodes { get; set; }

        public TspGraph()
        {
            this.Nodes = new List<(int, PointF)>();
        }

        public TspGraph(List<(int, PointF)> Nodes)
        {
            this.Nodes = Nodes;
        }

        public TspGraph(SerializationInfo info, StreamingContext context)
        {
            Nodes = new List<(int, PointF)>();
            int? count = (int?) info.GetValue("Count", typeof(int));
            if (count == null)
                return;
            for (int i = 0; i < count; i++)
            {
                int? nodeId = (int?) info.GetValue($"NodeId{i}", typeof(int));
                float? nodePointX = (float?) info.GetValue($"NodePointX{i}", typeof(float));
                float? nodePointY = (float?) info.GetValue($"NodePointY{i}", typeof(float));
                if (nodeId != null && nodePointX != null && nodePointY != null)
                Nodes.Add((nodeId.Value, new PointF(nodePointX.Value, nodePointY.Value)));
            }
        }

        public TspGraph(string pathToFile)
        {
            TspFileReader tspFileReader = new TspFileReader();
            Nodes = tspFileReader.ReadFile(pathToFile);
        }

        public void PermutateNodes()
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
            Nodes = nodesPermutation;
        }

        public float GetPathLength()
        {
            float length = 0;
            for (int i = 0; i < Nodes.Count - 1; i++)
                length += MathF.Sqrt( MathF.Pow(Nodes[i].Item2.X - Nodes[i + 1].Item2.X, 2) + MathF.Pow(Nodes[i].Item2.Y - Nodes[i + 1].Item2.Y, 2));
            length += MathF.Sqrt(MathF.Pow(Nodes[0].Item2.X - Nodes[Nodes.Count-1].Item2.X, 2) + MathF.Pow(Nodes[0].Item2.Y - Nodes[Nodes.Count - 1].Item2.Y, 2));
            return length;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Count", Nodes.Count);
            for (int i = 0; i < Nodes.Count; i++)
            {
                info.AddValue($"NodeId{i}", Nodes[i].Item1);
                info.AddValue($"NodePointX{i}", Nodes[i].Item2.X);
                info.AddValue($"NodePointY{i}", Nodes[i].Item2.Y);
            }
        }
    }
}
