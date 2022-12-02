using System.Diagnostics;
using System.Drawing;
using TspAlgorithms;

namespace ConsoleCalculation2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Process.Start("TasksCalculations.exe", "Witaj");
            Console.WriteLine("WASSSUUUUUUP");
            Console.ReadKey();

            //List<(int, PointF)> firstPermutation = new List<(int, PointF)>();
            //List<(int, PointF)> secondPermutation = new List<(int, PointF)>();
            //Random random = new Random();
            //firstPermutation.Add((3, new PointF(random.Next(100), random.Next(100))));
            //firstPermutation.Add((5, new PointF(random.Next(100), random.Next(100))));
            //firstPermutation.Add((1, new PointF(random.Next(100), random.Next(100))));
            //firstPermutation.Add((8, new PointF(random.Next(100), random.Next(100))));
            //firstPermutation.Add((2, new PointF(random.Next(100), random.Next(100))));
            //firstPermutation.Add((6, new PointF(random.Next(100), random.Next(100))));
            //firstPermutation.Add((7, new PointF(random.Next(100), random.Next(100))));
            //firstPermutation.Add((4, new PointF(random.Next(100), random.Next(100))));
            //firstPermutation.Add((9, new PointF(random.Next(100), random.Next(100))));

            //secondPermutation.Add((1, new PointF(random.Next(100), random.Next(100))));
            //secondPermutation.Add((6, new PointF(random.Next(100), random.Next(100))));
            //secondPermutation.Add((2, new PointF(random.Next(100), random.Next(100))));
            //secondPermutation.Add((7, new PointF(random.Next(100), random.Next(100))));
            //secondPermutation.Add((4, new PointF(random.Next(100), random.Next(100))));
            //secondPermutation.Add((8, new PointF(random.Next(100), random.Next(100))));
            //secondPermutation.Add((5, new PointF(random.Next(100), random.Next(100))));
            //secondPermutation.Add((9, new PointF(random.Next(100), random.Next(100))));
            //secondPermutation.Add((3, new PointF(random.Next(100), random.Next(100))));

            //TspGraph firstGraph = new TspGraph(firstPermutation);
            //TspGraph secondGraph = new TspGraph(secondPermutation);
            //printPermutation(firstPermutation);
            //printPermutation(secondPermutation);

            //Pmx pmx = new Pmx(firstGraph, secondGraph, 4);
            //pmx.Start();
            //TspGraph result = pmx.ResultingGraph;
            //printPermutation(result.Nodes);


            //TspGraph g = new TspGraph(firstPermutation);
            //printPermutation(firstPermutation);
            //Console.WriteLine(g.GetPathLength());
            //ThreeOpt threeOpt = new ThreeOpt(g);
            //threeOpt.Start();
            //secondPermutation = threeOpt.BestGraph.Nodes;
            //printPermutation(secondPermutation);
            //Console.WriteLine(threeOpt.BestGraph.GetPathLength());


        }
        //private static void printPermutation(List<(int, PointF)> permutation)
        //{
        //    string msg = "";
        //    for (int i = 0; i < permutation.Count; i++)
        //        msg += permutation[i].Item1.ToString() + " ";
        //    Console.WriteLine(msg);
        //}
    }
}