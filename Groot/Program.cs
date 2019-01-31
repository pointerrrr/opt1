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
            int MaxIterations = 1000, Q = 100;
            double T = 30d;
            try
            {
                MaxIterations = int.Parse(args[0]);
                T = double.Parse(args[1]);
                Q = int.Parse(args[2]);
            }
            catch
            {

            }

            LocalSearch search = new LocalSearch(MaxIterations, T, Q);

            Solution solution = search.FindSolution();

            PrintSolution(solution);
            Console.ReadLine();
        }
        

        public static void PrintSolution(Solution solution)
        {
            Truck truck1 = solution.Truck1;
            Truck truck2 = solution.Truck2;
            for (int i = 0; i < 5; i++)
            {
                int n = 1;
                for (int j = 0; j < truck1.Dagen[i].Count; j++)
                {
                    
                    for (int k = 0; k < truck1.Dagen[i][j].Item1.Count; k++)
                    {
                        
                        Console.WriteLine("1; " + (i + 1) + "; " + (n++) + "; " + truck1.Dagen[i][j].Item1[k]);
                    }
                        
                    Console.WriteLine("1; " + (i + 1) + "; " + (n++) + "; " + 0);
                    
                }
                n = 1;
                for (int j = 0; j < truck2.Dagen[i].Count; j++)
                {
                    for (int k = 0; k < truck2.Dagen[i][j].Item1.Count; k++)
                    {

                        Console.WriteLine("2; " + (i + 1) + "; " + (n++) + "; " + truck2.Dagen[i][j].Item1[k]);
                    }

                    Console.WriteLine("2; " + (i + 1) + "; " + (n++) + "; " + 0);
                }
            }
        }
    }
}
