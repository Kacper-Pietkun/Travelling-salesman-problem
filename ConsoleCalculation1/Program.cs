using System.Diagnostics;

namespace ConsoleCalculation2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Process.Start("C:\\Program Files\\Microsoft Visual Studio\\2022\\Community\\Common7\\IDE\\devenv.exe", "debugexe \"ConsoleCalculation2.exe\" \"Witaj\"");
            Console.WriteLine("Aby zakończyć główny proces naciśnij dowolny przycisk.");
            Console.ReadKey();
        }
    }
}