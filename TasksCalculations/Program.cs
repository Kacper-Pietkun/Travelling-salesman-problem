using System.Diagnostics;
using System.IO.Pipes;
using System.Security.Principal;
using TasksCalculations;
using TspAlgorithms;

namespace ConsoleCalculation1
{
    public class Program
    {
        static void Main(string[] args)
        {
            Calculator calculator = new Calculator();
            calculator.Calculate();
        }
    }
}
