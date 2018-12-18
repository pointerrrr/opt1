using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Groot.Data;

namespace Groot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            int maxIterations= 1000, Q = 100;
            double T = 100;
            try
            {
                maxIterations = int.Parse(args[0]);
                T = double.Parse(args[1]);
                Q = int.Parse(args[2]);
            }
            catch
            {

            }

            Init();

            LocalSearch search = new LocalSearch(maxIterations, T, Q);
            
            Solution solution = search.FindSolution();

            printSolution(solution);

            Console.ReadLine();
        }
        

        public static void printSolution(Solution solution)
        {
            Truck truck1 = solution.Truck1;
            Truck truck2 = solution.Truck2;
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < truck1.Dagen[i].Count; j++)
                {
                    Console.WriteLine("1; " + (i + 1) + "; " + (j + 1) + "; " + truck1.Dagen[i][j]);
                }
                for (int j = 0; j < truck2.Dagen[i].Count; j++)
                {
                    Console.WriteLine("2; " + (i + 1) + "; " + (j + 1) + "; " + truck2.Dagen[i][j]);
                }
            }
        }
    }
}
