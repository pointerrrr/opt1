﻿using System;
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
        public int[] ValidCheck;

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
            ValidCheck = new int[5];
        }
    }

    public class Truck
    {
        public int Id;
        public List<int>[] Dagen;
        public int[] RijTijden;
        public Solution Parent;
        public double Value;
        Random rng = new Random();
        

        public Truck(int id)
        {
            Id = id;
            Dagen = new List<int>[5];
            RijTijden = new int[5];

            for(int i = 0; i < 5; i++)
            {
                Dagen[i] = new List<int>();
                RijTijden[i] = 0;
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
            if (Parent != null)
                result.Parent = Parent;
            return result;
        }

        public void ChangeRijTijd(int dag, int a, int b, int c)
        {
            int previousRijTijd = RijTijden[dag];

            RijTijden[dag] -= LocalSearch.afstandenMatrix[a, c].Rijtijd;
            RijTijden[dag] += LocalSearch.afstandenMatrix[a, b].Rijtijd;
            RijTijden[dag] += LocalSearch.afstandenMatrix[b, c].Rijtijd;

            double newRijTijd = RijTijden[dag];

            double maxRijTijd = 3600 * 11.5;

            if (previousRijTijd < maxRijTijd && newRijTijd > maxRijTijd)
                Value += 150 + ((newRijTijd - maxRijTijd) / 30) * 150 * 60;
            else if (previousRijTijd > maxRijTijd && newRijTijd < maxRijTijd)
                Value -= 150 + ((previousRijTijd - maxRijTijd) / 30) * 150 * 60;
            else if (previousRijTijd > maxRijTijd && newRijTijd > maxRijTijd)
                Value += ((newRijTijd - maxRijTijd) / 30) * 150 * 60 - ((previousRijTijd - maxRijTijd) / 30) * 150 * 60;
        }

        public void ChangeCapaciteit(int voor, int na)
        {
            int max = 100000;

            if (voor < max && na > max)
                Value += 25 * (na - max);
            else if (voor > max && na < max)
                Value -= 25 * (voor - max);
            else if (voor > max && na > max)
                Value += 25 * (na - max) - 25 * (voor - max);
        }

        public int CheckCapacity(int dag, int index)
        {
            int result = 0;
            int i = index;
            int j = index + 1;

            while(i > 0 && Dagen[dag][i] != 0)
            {
                result += LocalSearch.ordersDict[Dagen[dag][i]].AantContainers * LocalSearch.ordersDict[Dagen[dag][i]].VolumePerContainer;
                i--;
            }
            while(j < Dagen[dag].Count && Dagen[dag][j] != 0 )
            {
                result += LocalSearch.ordersDict[Dagen[dag][j]].AantContainers * LocalSearch.ordersDict[Dagen[dag][j]].VolumePerContainer;
                j++;
            }

            return result;
        }


        // Add order at specific location and calculate new cost value
        public void AddBedrijf(int bedrijf, int index, int dag)
        {
            int a, b, c;

            // Set correct orderIds
            if (index == 0)
                a = 287;
            else if (Dagen[dag][index - 1] == 0)
                a = 287;
            else
                a = LocalSearch.ordersDict[Dagen[dag][index - 1]].MatrixID;

            if (bedrijf == 0)
                b = 287;
            else
                b = LocalSearch.ordersDict[bedrijf].MatrixID;

            if (index == Dagen[dag].Count - 1 || Dagen[dag][index + 1] == 0)
                c = 287;
            else
                c = LocalSearch.ordersDict[Dagen[dag][index + 1]].MatrixID;

            // Calculate new Cost Value
            Value += LocalSearch.afstandenMatrix[a, b].Rijtijd;
            Value += LocalSearch.afstandenMatrix[b, c].Rijtijd;

            if (bedrijf == 0)
                Value += 30 * 60;
            else
                Value += LocalSearch.ordersDict[bedrijf].LedigingDuurMinuten * 60;

            Value -= LocalSearch.afstandenMatrix[a, c].Rijtijd;

            ChangeRijTijd(dag, a, b, c);


            int capaciteitVoor = CheckCapacity(dag, index);

            // Add order to list
            if (bedrijf == 0)
                Dagen[dag].Insert(index, 0);
            else
                Dagen[dag].Insert(index, LocalSearch.ordersDict[bedrijf].Order);

            int capaciteitNa = CheckCapacity(dag, index);

            ChangeCapaciteit(capaciteitVoor, capaciteitNa);

            if (bedrijf != 0)
            {

                bool validVoor = Parent.ValidCheck[bedrijf].Valid;

                Parent.ValidCheck[bedrijf][dag]++;

                bool validNa = Parent.ValidCheck[bedrijf].Valid;

                if (validVoor)
                    Value += LocalSearch.ordersDict[bedrijf].Frequentie * LocalSearch.ordersDict[bedrijf].LedigingDuurMinuten * 3 * 60;
                else if (validNa)
                    Value -= LocalSearch.ordersDict[bedrijf].Frequentie * LocalSearch.ordersDict[bedrijf].LedigingDuurMinuten * 3 * 60;
            }
        }


        public void RemoveBedrijf(int index, int dag)
        {
            if (Dagen[dag].Count == 1)
                return;
            int a, b, c;

            // Set correct orderIds
            if (index == 0)
                a = 287;
            else if (Dagen[dag][index - 1] == 0)
                a = 287;
            else
                a = LocalSearch.ordersDict[Dagen[dag][index - 1]].MatrixID;

            if (Dagen[dag][index] == 0)
                b = 287;
            else
                b = LocalSearch.ordersDict[Dagen[dag][index]].MatrixID;

            if (Dagen[dag][index + 1] == 0)
                c = 287;
            else
                c = LocalSearch.ordersDict[Dagen[dag][index + 1]].MatrixID;

            // Calculate new Cost Value
            Value -= LocalSearch.afstandenMatrix[a,b].Rijtijd;
            Value -= LocalSearch.afstandenMatrix[b,c].Rijtijd;

            if (Dagen[dag][index] == 0)
                Value -= 30 * 60;
            else
                Value -= LocalSearch.ordersDict[Dagen[dag][index]].LedigingDuurMinuten * 60;

            Value += LocalSearch.afstandenMatrix[a,c].Rijtijd;

            ChangeRijTijd(dag, a, b, c);

            int capaciteitVoor = CheckCapacity(dag, index);

            if (Dagen[dag][index] != 0)
            {

                bool validVoor = Parent.ValidCheck[Dagen[dag][index]].Valid;

                Parent.ValidCheck[LocalSearch.ordersDict[Dagen[dag][index]].Order][dag]--;

                bool validNa = Parent.ValidCheck[Dagen[dag][index]].Valid;

                if (validVoor)
                    Value += LocalSearch.ordersDict[Dagen[dag][index]].Frequentie * LocalSearch.ordersDict[Dagen[dag][index]].LedigingDuurMinuten * 3 * 60;
                else if (validNa)
                    Value -= LocalSearch.ordersDict[Dagen[dag][index]].Frequentie * LocalSearch.ordersDict[Dagen[dag][index]].LedigingDuurMinuten * 3 * 60;
            }

            // Remove from list
            Dagen[dag].RemoveAt(index);

            

            int capaciteitNa = CheckCapacity(dag, index);

            ChangeCapaciteit(capaciteitVoor, capaciteitNa);            
        }
        
        // Add random order and calculate new cost value
        public void RemoveRandomBedrijf()
        {
            // Init random values
            int dag = rng.Next(5);
            int index = rng.Next(0, Dagen[dag].Count - 1);
            int count = 0;

            while (Dagen[dag][index] == 0 && count++ < 10)
                index = rng.Next(0, Dagen[dag].Count - 1);

            // Only remove if not dump
            if(count < 10)
                RemoveBedrijf(index, dag);
        }

        // Add dunp to random place
        public void RemoveRandomDump()
        {
            // Init random values
            int dag = rng.Next(5);

            int count = Dagen[dag].Count(a => { return a == 0; });

            if (count > 1)
            {
                int index = rng.Next(0, count - 1);
                count = 0;

                for (int i = 0; i < Dagen[dag].Count; i++)
                {
                    if (Dagen[dag][i] == 0)
                    {
                        if (count == index)
                        {
                            RemoveBedrijf(i, dag);
                            break;
                        }
                        count++;
                    }   
                }
            }
        }
        // Add random order and calculate new cost value
        public void AddRandomBedrijf()
        {
            // Init random values
            int dag = rng.Next(5);
            int index = rng.Next(0, Dagen[dag].Count - 1);
            int bedrijf = LocalSearch.orders[rng.Next(LocalSearch.orders.Length)].Order;

            AddBedrijf(bedrijf, index, dag);
        }

        // Add dunp to random place
        public void AddRandomDump()
        {
            int dump = 0;

            // Init random values
            int dag = rng.Next(5);
            int index = rng.Next(0, Dagen[dag].Count - 1);

            AddBedrijf(dump, index, dag);
        }

        // Swap random orders and calculate new cost value
        public void SwapRandomOrderInDay()
        {
            int dag = rng.Next(5);
            int plek1;
            int plek2;
            int plek1res;
            int plek2res;

            plek1 = rng.Next(0, Dagen[dag].Count - 1);
            plek1res = Dagen[dag][plek1];
            RemoveBedrijf(plek1, dag);

            plek2 = rng.Next(0, Dagen[dag].Count - 1);
            plek2res = Dagen[dag][plek2];
            RemoveBedrijf(plek2, dag);

            AddBedrijf(plek2res, plek2, dag);
            AddBedrijf(plek1res, plek1, dag);
        }

        // Swap random orders and calculate new cost value
        public void swapRandomOrderDiffDay()
        {
            int plek1;
            int plek2;
            int plek1res;
            int plek2res;
            int count = 0;

            // Init random values
            int dag1 = rng.Next(5);
            int dag2 = rng.Next(5);

            while (dag1 == dag2)
                dag2 = rng.Next(5);

            plek1 = rng.Next(0, Dagen[dag1].Count - 1);
            plek2 = rng.Next(0, Dagen[dag2].Count - 1);

            while ((Dagen[dag1][plek1] == 0 || Dagen[dag2][plek2] == 0) && count++ < 10)
            {
                plek1 = rng.Next(0, Dagen[dag1].Count - 1);
                plek2 = rng.Next(0, Dagen[dag2].Count - 1);
            }
            
            // Swap orders if not dump
            if (count < 10)
            {
                plek1res = Dagen[dag1][plek1];
                plek2res = Dagen[dag2][plek2];
                RemoveBedrijf(plek1, dag1);
                RemoveBedrijf(plek2, dag2);
                AddBedrijf(plek2res, plek1, dag1);
                AddBedrijf(plek1res, plek2, dag2);
            }
        }

        public void swapOrderDiffTruck(Truck t)
        {
            int dag1 = rng.Next(5);
            int dag2 = rng.Next(5);

            int plek1;
            int plek2;
            int plek1res;
            int counter = 0;

            plek1 = rng.Next(0, Dagen[dag1].Count - 1);
            plek2 = rng.Next(0, t.Dagen[dag2].Count - 1);

            while ((Dagen[dag1][plek1] == 0 || t.Dagen[dag2][plek2] == 0) && counter++ < 10)
            {
                plek1 = rng.Next(0, Dagen[dag1].Count - 1);
                plek2 = rng.Next(0, t.Dagen[dag2].Count - 1);
            }
                
            if (counter < 10)
            {
                plek1res = Dagen[dag1][plek1];
                RemoveBedrijf(plek1, dag1);
                t.AddBedrijf(plek1res, plek2, dag2);
            }
        }
    }

    public class Solution
    {
        public Truck Item1, Item2;

        public Dictionary<int, ValidArray> ValidCheck;

        public double Value
        {
            get
            {
                return Item1.Value + Item2.Value;
            }
        }

        public Solution(Truck truck1, Truck truck2)
        {
            ValidCheck = new Dictionary<int, ValidArray>();
            Item1 = truck1.Copy();
            Item1.Parent = this;
            Item2 = truck2.Copy();
            Item2.Parent = this;
        }

        public Solution Copy()
        {
            Solution result = new Solution(Item1,Item2);
            result.ValidCheck = new Dictionary<int, ValidArray>(ValidCheck);
            return result;
        }
    }

    public class ValidArray
    {
        public int Dag1, Dag2, Dag3, Dag4, Dag5, Frequentie;

        public bool Valid
        {
            get
            {
                switch(Frequentie)
                {
                    case 1:
                        int count = 0;
                        for (int i = 0; i < 0; i++)
                            if (this[i] == 1)
                                count++;
                            else if (this[i] > 1)
                                return false;      
                        return count == 1;
                    case 2:
                        return (this[0] == 1 && this[1] == 0 && this[2] == 0 && this[3] == 1 && this[4] == 0) || (this[0] == 0 && this[1] == 1 && this[2] == 0 && this[3] == 0 && this[4] == 1);
                    case 3:
                        return this[0] == 1 && this[1] == 0 && this[2] == 1 && this[3] == 0 && this[4] == 1;
                    case 4:
                        count = 0;
                        for (int i = 0; i < 0; i++)
                            if (this[i] == 1)
                                count++;
                            else if (this[i] > 1)
                                return false;
                        return count == 4;
                    case 5:
                        return this[0] == 1 && this[1] == 1 && this[2] == 1 && this[3] == 1 && this[4] == 1;
                    default:
                        return false;
                }
            }
        }

        public ValidArray(int a = 0, int b = 0, int c = 0, int d = 0, int e = 0, int f = 1)
        {
            Dag1 = a; Dag2 = b; Dag3 = c; Dag4 = d; Dag5 = e;
        }

        public ValidArray Copy()
        {
            return new ValidArray(Dag1, Dag2, Dag3, Dag4, Dag5, Frequentie);
        }

        public int this[int key]
        {
            get
            {
                switch(key)
                {
                    case 0:
                        return Dag1;
                    case 1:
                        return Dag2;
                    case 2:
                        return Dag3;
                    case 3:
                        return Dag4;
                    case 4:
                        return Dag5;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (key)
                {
                    case 0:
                        Dag1 = value;
                        break;
                    case 1:
                        Dag2 = value;
                        break;
                    case 2:
                        Dag3 = value ;
                        break;
                    case 3:
                        Dag4 = value;
                        break;
                    case 4:
                        Dag5 = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }
    }
}
