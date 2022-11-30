using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TspAlgorithms
{
    public class Pmx
    {
        private TspGraph firstGraph;
        private TspGraph secondGraph;
        public TspGraph ResultingGraph { get; set; }
        private int subnodesCount;
        private int permutationLength;
        private Random random;

        public Pmx(TspGraph _firstGraph, TspGraph _secondGraph, int _subnodesCount)
        {
            firstGraph = _firstGraph;
            secondGraph = _secondGraph;
            ResultingGraph = new TspGraph();
            for (int i = 0; i < firstGraph.Nodes.Count; i++)
                ResultingGraph.Nodes.Add((-1, new PointF(0, 0)));
            subnodesCount = _subnodesCount;
            permutationLength = firstGraph.Nodes.Count;
            random = new Random();
        }

        public void Start()
        {
            int startIndex = 3;// random.Next(0, permutationLength - subnodesCount + 1);
            int endIndex = startIndex + subnodesCount - 1;

            CopySubNodes(firstGraph.Nodes, ResultingGraph.Nodes, startIndex, endIndex);
            ReassignNodes(firstGraph.Nodes, secondGraph.Nodes, ResultingGraph.Nodes, startIndex, endIndex);
            CopyRemainingnodes(secondGraph.Nodes, ResultingGraph.Nodes);
        }

        private void CopyRemainingnodes(List<(int, PointF)> source, List<(int, PointF)> destination)
        {
            for (int i = 0; i < destination.Count; i++)
            {
                if (destination[i].Item1 == -1)
                    destination[i] = (source[i].Item1, new PointF(source[i].Item2.X, source[i].Item2.Y));
            }
        }

        private void CopySubNodes(List<(int, PointF)> source, List<(int, PointF)> destination, int startIndex, int endIndex)
        {
            for (int i = startIndex; i <= endIndex; i++)
                destination[i] = (source[i].Item1, new PointF(source[i].Item2.X, source[i].Item2.Y));
        }


        private void ReassignNodes(List<(int, PointF)> first, List<(int, PointF)> second, List<(int, PointF)> result, int startIndex, int endIndex)
        {
            for (int i = startIndex; i <= endIndex; i++)
            {
                int newIndex = FindPlaceForNode(first, second, startIndex, endIndex, i);
                if (newIndex != -1)
                    result[newIndex] = (second[i].Item1, new PointF(second[i].Item2.X, second[i].Item2.Y));
            }
        }


        private int FindPlaceForNode(List<(int, PointF)> first, List<(int, PointF)> second, int startIndex, int endIndex, int i)
        {
            if (FindNode(first, startIndex, endIndex, second[i].Item1))
                return -1;
            int placeOfParent = i;
            while (placeOfParent >= startIndex && placeOfParent <= endIndex)
            {
                i = placeOfParent;
                placeOfParent = FindNode(second, first[i].Item1);
            }
            return placeOfParent;
        }

        private int FindNode(List<(int, PointF)> permutation, int nodeId)
        {
            for (int i = 0; i < permutation.Count; i++)
                if (permutation[i].Item1 == nodeId)
                    return i;
            return -1;
        }

        private bool FindNode(List<(int, PointF)> permutation, int startIndex, int endIndex, int nodeId)
        {
            for (int i = startIndex; i <= endIndex; i++)
                if (permutation[i].Item1 == nodeId)
                    return true;
            return false;
        }
    }
}
