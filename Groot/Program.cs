using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Groot
{
    class Program
    {
        public static void Main(string[] args)
        {
            Tuple<Truck, Truck> trucks = new Tuple<Truck, Truck>(new Truck(0), new Truck(1));

            LocalSearch search = new LocalSearch();

            search.FindSolution(trucks);

            printSolution(trucks);

            Console.ReadLine();
        }



        public static void findSolution(Tuple<Truck, Truck> trucks)
        {

        }

        public static void printSolution(Tuple<Truck, Truck> trucks)
        {
            Truck truck1 = trucks.Item1;
            Truck truck2 = trucks.Item2;
            for (int i = 0; i < 5; i++)
            {
                for(int j = 0; j < truck1.Dagen[i].Count; j++)
                {
                    Console.WriteLine(truck1.Id + "; " + i + "; " + j + "; " + truck1.Dagen[i][j]);
                }
                for (int j = 0; j < truck2.Dagen[i].Count; j++)
                {
                    Console.WriteLine(truck2.Id + "; " + i + "; " + j + "; " + truck2.Dagen[i][j]);
                }
            }
        }
    }

    public struct AfstandRijtijd
    {
        public int Afstand, Rijtijd;

        public AfstandRijtijd(int a, int b)
        {
            Afstand = a;
            Rijtijd = b;
        }
    }

    public struct OrderDescription
    {
        public int Order, Frequentie, AantContainers, VolumePerContainer, MatrixID, XCoordinaat, YCoordinaat;
        public double LedigingDuurMinuten;
        public string Plaats;

        public OrderDescription(int a, string b, int c, int d, int e, double f, int g, int h, int i)
        {
            Order = a;
            Plaats = b;
            Frequentie = c;
            AantContainers = d;
            VolumePerContainer = e;
            LedigingDuurMinuten = f;
            MatrixID = g;
            XCoordinaat = h;
            YCoordinaat = i;
        }
    }

    public class Truck
    {
        public int Id;
        public List<int>[] Dagen;

        public Truck(int id)
        {
            Id = id;
            Dagen = new List<int>[5];

            foreach(List<int> list in Dagen)
            {
                list.Add(0);
            }
        }
    }
}
