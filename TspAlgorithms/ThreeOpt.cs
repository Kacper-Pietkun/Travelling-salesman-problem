using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TspAlgorithms
{
    public class ThreeOpt
    {
        private TspGraph originalGraph;
        public TspGraph BestGraph { get; set; }
        private float bestLength;

        public ThreeOpt(TspGraph _originalPermutation)
        {
            originalGraph = _originalPermutation;
            BestGraph = originalGraph;
            bestLength = BestGraph.GetPathLength();
        }

        public IEnumerable<int> Start()
        {
            for (int i = 0; i < originalGraph.Nodes.Count; i++)
            {
                for (int j = i; j < originalGraph.Nodes.Count; j++)
                {
                    for (int k = j; k < originalGraph.Nodes.Count - 1; k++)
                    {
                        if (i == j || i + 1 == j || j == k || j + 1 == k)
                            continue;
                        TspGraph firstGraph = CreateFirstPossibility(i, j, k);
                        float firstLength = firstGraph.GetPathLength();
                        if (firstLength < bestLength)
                        {
                            BestGraph = firstGraph;
                            bestLength = firstLength;
                            BestGraph.PathLength = bestLength;
                            yield return 1;
                        }
                        TspGraph secondGraph = CreateSecondPossibility(i, j, k);
                        float secondLength = secondGraph.GetPathLength();
                        if (secondLength < bestLength)
                        {
                            BestGraph = secondGraph;
                            bestLength = secondLength;
                            BestGraph.PathLength = bestLength;
                            yield return 1;
                        }
                    }
                }
            }
        }

        private TspGraph CreateFirstPossibility(int firstIndex, int secondIndex, int thirdIndex)
        {
            TspGraph newGraph = new TspGraph();
            int distance = getDistanceBetweenIndices(firstIndex + 1, secondIndex);
            int cycleLength = originalGraph.Nodes.Count;
            int i = firstIndex + 1;
            while (distance >= 0)
            {
                newGraph.Nodes.Add((originalGraph.Nodes[(i + cycleLength) % cycleLength].Item1,
                    new PointF(originalGraph.Nodes[(i + cycleLength) % cycleLength].Item2.X, originalGraph.Nodes[(i + cycleLength) % cycleLength].Item2.Y)));
                i++;
                distance--;
            }
            distance = getDistanceBetweenIndices(secondIndex + 1, thirdIndex);
            i = thirdIndex;
            while (distance >= 0)
            {
                newGraph.Nodes.Add((originalGraph.Nodes[(i + cycleLength) % cycleLength].Item1,
                    new PointF(originalGraph.Nodes[(i + cycleLength) % cycleLength].Item2.X, originalGraph.Nodes[(i + cycleLength) % cycleLength].Item2.Y)));
                i--;
                distance--;
            }
            distance = getDistanceBetweenIndices(thirdIndex + 1, firstIndex);
            i = firstIndex;
            while (distance >= 0)
            {
                newGraph.Nodes.Add((originalGraph.Nodes[(i + cycleLength) % cycleLength].Item1,
                    new PointF(originalGraph.Nodes[(i + cycleLength) % cycleLength].Item2.X, originalGraph.Nodes[(i + cycleLength) % cycleLength].Item2.Y)));
                i--;
                distance--;
            }
            return newGraph;
        }

        private TspGraph CreateSecondPossibility(int firstIndex, int secondIndex, int thirdIndex)
        {
            TspGraph newGraph = new TspGraph();
            int distance = getDistanceBetweenIndices(firstIndex + 1, secondIndex);
            int cycleLength = originalGraph.Nodes.Count;
            int i = firstIndex + 1;
            while (distance >= 0)
            {
                newGraph.Nodes.Add((originalGraph.Nodes[(i + cycleLength) % cycleLength].Item1,
                    new PointF(originalGraph.Nodes[(i + cycleLength) % cycleLength].Item2.X, originalGraph.Nodes[(i + cycleLength) % cycleLength].Item2.Y)));
                i++;
                distance--;
            }

            distance = getDistanceBetweenIndices(thirdIndex + 1, firstIndex);
            i = firstIndex;
            while (distance >= 0)
            {
                newGraph.Nodes.Add((originalGraph.Nodes[(i + cycleLength) % cycleLength].Item1,
                    new PointF(originalGraph.Nodes[(i + cycleLength) % cycleLength].Item2.X, originalGraph.Nodes[(i + cycleLength) % cycleLength].Item2.Y)));
                i--;
                distance--;
            }

            distance = getDistanceBetweenIndices(secondIndex + 1, thirdIndex);
            i = secondIndex + 1;
            while (distance >= 0)
            {
                newGraph.Nodes.Add((originalGraph.Nodes[(i + cycleLength) % cycleLength].Item1,
                    new PointF(originalGraph.Nodes[(i + cycleLength) % cycleLength].Item2.X, originalGraph.Nodes[(i + cycleLength) % cycleLength].Item2.Y)));
                i++;
                distance--;
            }
            return newGraph;
        }

        private int getDistanceBetweenIndices(int firstIndex, int secondIndex)
        {
            int distance = 0;
            while (firstIndex != secondIndex)
            {
                firstIndex = (firstIndex + 1) % originalGraph.Nodes.Count;
                distance++;
            }
            return distance;
        }

    }
}
