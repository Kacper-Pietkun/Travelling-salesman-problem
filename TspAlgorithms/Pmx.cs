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
        private List<(int, PointF)> firstPermutation;
        private List<(int, PointF)> secondPermutation;
        private List<(int, PointF)> resultingPermutation;
        private int subnodesCount;
        private int permutationLength;
        private Random random;

        public Pmx(List<(int, PointF)> _firstPermutation, List<(int, PointF)> _secondPermutation, int _subnodesCount)
        {
            firstPermutation = _firstPermutation;
            secondPermutation = _secondPermutation;
            resultingPermutation = new List<(int, PointF)>();
            for (int i = 0; i < firstPermutation.Count; i++)
                resultingPermutation.Add((-1, new PointF(0, 0)));
            subnodesCount = _subnodesCount;
            permutationLength = firstPermutation.Count;
            random = new Random();
        }

        public void Start()
        {
            int startIndex = 3;// random.Next(0, permutationLength - subnodesCount + 1);
            int endIndex = startIndex + subnodesCount - 1;

            CopySubNodes(firstPermutation, resultingPermutation, startIndex, endIndex);
            ReassignNodes(firstPermutation, secondPermutation, resultingPermutation, startIndex, endIndex);
            CopyRemainingnodes(secondPermutation, resultingPermutation);
        }

        public List<(int, PointF)> GetResultingPermutation()
        {
            return resultingPermutation;
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
