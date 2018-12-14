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
            Solution trucks = new Solution(new Truck(1), new Truck(2));

            LocalSearch search = new LocalSearch();

            search.FindSolution(trucks);

            printSolution(trucks);

            Console.ReadLine();
        }



        public static void findSolution(Solution trucks)
        {

        }

        public static void printSolution(Solution trucks)
        {
            Truck truck1 = trucks.Item1;
            Truck truck2 = trucks.Item2;
            for (int i = 0; i < 5; i++)
            {
                for(int j = 0; j < truck1.Dagen[i].Count; j++)
                {
                    Console.WriteLine(truck1.Id + "; " + (i+1) + "; " + (j+1) + "; " + truck1.Dagen[i][j]);
                }
                for (int j = 0; j < truck2.Dagen[i].Count; j++)
                {
                    Console.WriteLine(truck2.Id + "; " + (i+1) + "; " + (j+1) + "; " + truck2.Dagen[i][j]);
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
        public double Value;

        public Truck(int id)
        {
            Id = id;
            Dagen = new List<int>[5];

            for(int i = 0; i < 5; i++)
            {
                Dagen[i] = new List<int>();
            }

            foreach(List<int> list in Dagen)
            {
                list.Add(0);
            }
        }

        public Truck Copy()
        {
            Truck result = new Truck(Id);
            result.Dagen = Dagen.Take(Dagen.Length).ToArray();
            return result;
        }

        public void RemoveBedrijf(int index, int dag)
        {
            int a, b, c;
            if (index == 0)
            {
                a = 287;

            }
            else
            {
                a = LocalSearch.ordersDict[Dagen[dag][index - 1]].MatrixID;
            }
            if (Dagen[dag][index] == 0)
                b = 287;
            else
                b = LocalSearch.ordersDict[Dagen[dag][index]].MatrixID;
            if (Dagen[dag][index + 1] == 0)
                c = 287;
            else
                c = LocalSearch.ordersDict[Dagen[dag][index + 1]].MatrixID;
            Value -= LocalSearch.afstandenMatrix[a,b].Rijtijd;
            Value -= LocalSearch.afstandenMatrix[b,c].Rijtijd;
            if (Dagen[dag][index] == 0)
                Value -= 30 * 60;
            else
                Value -= LocalSearch.ordersDict[Dagen[dag][index]].LedigingDuurMinuten * 60;
            Value += LocalSearch.afstandenMatrix[a,c].Rijtijd;
        }

        public void AddBedrijf(int bedrijf, int index, int dag)
        {
            int a, b, c;
            if(index == 0)
            {
                a = 287;
                
            }
            else
            {
                a = LocalSearch.ordersDict[Dagen[dag][index - 1]].MatrixID;
            }

            if(bedrijf == 0)
            {
                b= 287;
            }
            else
            {
                b= LocalSearch.ordersDict[bedrijf].MatrixID;
            }

            if (Dagen[dag][index + 1] == 0)
                c = 287;
            else
                c = LocalSearch.ordersDict[Dagen[dag][index + 1]].MatrixID;
            Value += LocalSearch.afstandenMatrix[a, b].Rijtijd;
            Value += LocalSearch.afstandenMatrix[b, c].Rijtijd;
            if (bedrijf == 0)
                Value += 30 * 60;
            else
                Value += LocalSearch.ordersDict[bedrijf].LedigingDuurMinuten * 60;
            Value -= LocalSearch.afstandenMatrix[a,c].Rijtijd;
        }
    }

    public class Solution
    {
        public Truck Item1, Item2;
        public double Value;

        public Solution(Truck truck1, Truck truck2)
        {
            Item1 = truck1.Copy();
            Item2 = truck2.Copy();
        }

        public Solution Copy()
        {
            Solution result = new Solution(Item1,Item2);
            result.Value = Value;
            return result;
        }
    }
}
