using System.Diagnostics;

namespace ConsoleCalculation1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Czekanie na podczepienie debbugera do procesu");
            Console.WriteLine("Po podłączeniu wciśnij dowolny klawisz");
            Console.ReadKey();
            Console.WriteLine("Tutaj postaw pułapkę");
            if (args.Length > 0)
            {
                Console.WriteLine("Pierwszy parametr przekazany do procesu to: " + args[0]);
            }
            else
            {
                Console.WriteLine("Nie przekazano żadnego parametru do procesu");
            }
        }
    }
}
