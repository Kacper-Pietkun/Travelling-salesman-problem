using System.Diagnostics;
using System.Drawing;
using TspAlgorithms;

namespace ConsoleCalculation2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Process.Start("C:\\Program Files\\Microsoft Visual Studio\\2022\\Community\\Common7\\IDE\\devenv.exe", "debugexe \"ConsoleCalculation2.exe\" \"Witaj\"");
            //Console.WriteLine("Aby zakończyć główny proces naciśnij dowolny przycisk.");
            //Console.ReadKey();

            List<(int, PointF)> firstPermutation = new List<(int, PointF)>();
            List<(int, PointF)> secondPermutation = new List<(int, PointF)>();

            firstPermutation.Add((3, new PointF(0, 0)));
            firstPermutation.Add((5, new PointF(0, 0)));
            firstPermutation.Add((1, new PointF(0, 0)));
            firstPermutation.Add((8, new PointF(0, 0)));
            firstPermutation.Add((2, new PointF(0, 0)));
            firstPermutation.Add((6, new PointF(0, 0)));
            firstPermutation.Add((7, new PointF(0, 0)));
            firstPermutation.Add((4, new PointF(0, 0)));
            firstPermutation.Add((9, new PointF(0, 0)));

            secondPermutation.Add((1, new PointF(0, 0)));
            secondPermutation.Add((6, new PointF(0, 0)));
            secondPermutation.Add((2, new PointF(0, 0)));
            secondPermutation.Add((7, new PointF(0, 0)));
            secondPermutation.Add((4, new PointF(0, 0)));
            secondPermutation.Add((8, new PointF(0, 0)));
            secondPermutation.Add((5, new PointF(0, 0)));
            secondPermutation.Add((9, new PointF(0, 0)));
            secondPermutation.Add((3, new PointF(0, 0)));


            printPermutation(firstPermutation);
            printPermutation(secondPermutation);

            Pmx pmx = new Pmx(firstPermutation, secondPermutation, 4);
            pmx.Start();
            List<(int, PointF)> result = pmx.GetResultingPermutation();
            printPermutation(result);


        }
        private static void printPermutation(List<(int, PointF)> permutation)
        {
            string msg = "";
            for (int i = 0; i < permutation.Count; i++)
                msg += permutation[i].Item1.ToString() + " ";
            Console.WriteLine(msg);
        }
    }
}