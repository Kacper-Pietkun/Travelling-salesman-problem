using System.Diagnostics;
using System.Drawing;
using ThreadsCalculations;
using TspAlgorithms;

namespace ConsoleCalculation2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Calculator calculator = new Calculator();
            calculator.Calculate();
        }
    }
}