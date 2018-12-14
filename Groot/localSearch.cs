using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Groot
{
    class LocalSearch
    {
        int kmax = 10000000;
        double temp = 100d;
        Random rng = new Random();
        Solution bestSolution = new Solution(new Truck(1), new Truck(2));
        public static AfstandRijtijd[,] afstandenMatrix = null;
        public static Dictionary<int, OrderDescription> ordersDict = null;
        public static OrderDescription[] orders = null;
        private double MaxPenalty;

        public LocalSearch()
        {
            inladen();
            orders = ordersDict.Values.ToArray();
            MaxPenalty = solutionValue(new Solution(new Truck(1), new Truck(2)));
        }

        public Solution FindSolution(Solution trucks)
        {
            Solution currentSolution = new Solution(trucks.Item1.Copy(), trucks.Item2.Copy());

            bestSolution = currentSolution;
            orders = ordersDict.Values.ToArray();
            for (int i = 0; i < 1000; i++)
            {
                int index = rng.Next(0, orders.Length);
                currentSolution.Item1.AddBedrijf(orders[index].Order, i % 200, i / 200);

                index = rng.Next(0, orders.Length);
                currentSolution.Item2.AddBedrijf(orders[index].Order, i % 200, i / 200);
            }

            for (int i = 0; i < kmax; i++)
            {
                if(i % 10000 == 0)
                {
                    temp *= 0.99d;
                }

                Solution randomNeighbor = newNeighbor(currentSolution);

                if (acceptanceChance(currentSolution, randomNeighbor, temp) >= rng.NextDouble() || randomNeighbor.Value < currentSolution.Value)
                {
                    currentSolution = randomNeighbor;
                    if (currentSolution.Value < bestSolution.Value)
                    {
                        bestSolution = randomNeighbor;
                    }
                }

                
            }
            return bestSolution;
        }

        double acceptanceChance(Solution currentSolution, Solution randomNeighbor, double temp)
        {
            return Math.Exp((currentSolution.Value - randomNeighbor.Value) / temp );
        }

        // TODO !!!
        Solution newNeighbor(Solution trucks)
        {
            Solution result = new Solution(trucks.Item1.Copy(), trucks.Item2.Copy());
            int choice = rng.Next(7);
            switch(choice)
            {
                case 0:
                    addBedrijf(result);
                    break;
                case 1:
                    removeBedrijf(result);
                    break;
                case 2:
                    swapOrder(result);
                    break;
                case 3:
                    addDumpen(result);
                    break;
                case 4:
                    removeDumpen(result);
                    break;
                case 5:
                    changeOrderDay(result);
                    break;
                case 6:
                    changeOrderTruck(result);
                    break;
            }
            return result;
        }

        Solution addBedrijf(Solution trucks)
        {
            int truck = rng.Next(2);
            switch(truck)
            {
                case 0:
                    trucks.Item1.AddRandomBedrijf();
                    break;
                case 1:
                    trucks.Item2.AddRandomBedrijf();
                    break;
            }
            return trucks;
        }

        Solution removeBedrijf(Solution trucks)
        {
            int truck = rng.Next(2);
            switch(truck)
            {
                case 0:
                    trucks.Item1.RemoveRandomBedrijf();
                    break;
                case 1:
                    trucks.Item2.RemoveRandomBedrijf();
                    break;
            }
            return trucks;
        }

        Solution swapOrder(Solution trucks)
        {
            int truck = rng.Next(2);

            switch (truck)
            {
                case 0:
                    trucks.Item1.SwapRandomOrderInDay();
                    break;
                case 1:
                    trucks.Item2.SwapRandomOrderInDay();
                    break;
            }
            return trucks;
        }

        Solution addDumpen(Solution trucks)
        {
            int truck = rng.Next(2);

            switch (truck)
            {
                case 0:
                    trucks.Item1.AddRandomDump();
                    break;
                case 1:
                    trucks.Item2.AddRandomDump();
                    break;
            }
            return trucks;
        }

        Solution removeDumpen(Solution trucks)
        {
            int truck = rng.Next(2);

            switch (truck)
            {
                case 0:
                    trucks.Item1.RemoveRandomDump();
                    break;
                case 1:
                    trucks.Item2.RemoveRandomDump();
                    break;
            }
            return trucks;
        }

        Solution changeOrderDay(Solution trucks)
        {
            int truck = rng.Next(2);
            
            switch (truck)
            {
                case 0:
                    trucks.Item1.swapRandomOrderDiffDay();
                    break;
                case 1:
                    trucks.Item2.swapRandomOrderDiffDay();
                    break;
            }
            return trucks;
        }

        Solution changeOrderTruck(Solution trucks)
        {
            int truck = rng.Next(2);

            switch (truck)
            {
                case 0:
                    trucks.Item1.swapOrderDiffTruck(trucks.Item2);
                    break;
                case 1:
                    trucks.Item2.swapOrderDiffTruck(trucks.Item1);
                    break;
            }
            return trucks;
        }

        public static double solutionValue(Solution trucks)
        {
            double solution = 0;
            foreach(KeyValuePair<int, OrderDescription> kvp in ordersDict)
            {
                OrderDescription order = kvp.Value;
                if(!orderVoldaan(trucks, order))
                    solution += order.LedigingDuurMinuten * order.Frequentie * 3 * 60;
            }
            solution += addTijden(trucks.Item1);
            solution += addTijden(trucks.Item2);
            return solution;
        }

        static double addTijden(Truck truck)
        {
            double solution = 0;
            foreach (List<int> dagen in truck.Dagen)
            {
                double vandaag = 0;
                double lading = 0;
                for (int i = 1; i < dagen.Count; i++)
                {
                    int a = dagen[i-1], b = dagen[i];
                    if (b != 0)
                    {
                        lading += ordersDict[b].AantContainers * ordersDict[b].VolumePerContainer;
                        if (lading > 100000)
                            solution += (100000 - lading) * 25;
                        else
                            vandaag += ordersDict[b].LedigingDuurMinuten * 60;
                    }
                    else
                    {
                        vandaag += 30 * 60;
                        lading = 0;
                    }
                    
                    if (a == 0)
                        a = 287;
                    else
                        a = ordersDict[a].MatrixID;
                    if (b == 0)
                        b = 287;
                    else
                        b = ordersDict[b].MatrixID;
                    vandaag += afstandenMatrix[a, b].Rijtijd;
                }
                if(vandaag > 60 * 11.5 * 60)
                {
                    solution += 150 + ((vandaag - 60 * 60 * 11.5) / 30) * 150 * 60;
                }
                solution += vandaag;
            }
            return solution;
        }

        // TODO
        static bool orderVoldaan(Solution trucks, OrderDescription order)
        {
            bool result = true;
            Predicate<int> p = i => i == order.Order;
            List<int> a = new List<int>(), b = new List<int>(), c = new List<int>(), d = new List<int>(), e = new List<int>();
            for (int i = 0; i < trucks.Item1.Dagen[0].Count; i++)
                a.Add(trucks.Item1.Dagen[0][i]);
            a.AddRange(trucks.Item2.Dagen[0]);
            for (int i = 0; i < trucks.Item1.Dagen[1].Count; i++)
                b.Add(trucks.Item1.Dagen[1][i]);
            b.AddRange(trucks.Item2.Dagen[1]);
            for (int i = 0; i < trucks.Item1.Dagen[2].Count; i++)
                c.Add(trucks.Item1.Dagen[2][i]);
            c.AddRange(trucks.Item2.Dagen[2]);
            for (int i = 0; i < trucks.Item1.Dagen[3].Count; i++)
                d.Add(trucks.Item1.Dagen[3][i]);
            d.AddRange(trucks.Item2.Dagen[3]);
            for (int i = 0; i < trucks.Item1.Dagen[4].Count; i++)
                e.Add(trucks.Item1.Dagen[4][i]);
            e.AddRange(trucks.Item2.Dagen[4]);

            switch (order.Frequentie)
            {
                case 1:
                    result = a.Exists(p) || b.Exists(p) || c.Exists(p) || d.Exists(p) || e.Exists(p);
                    break;
                case 2:
                    result = (a.Exists(p) && c.Exists(p)) || (b.Exists(p) && d.Exists(p));
                    break;
                case 3:
                    result = a.Exists(p) && b.Exists(p) && c.Exists(p);
                    break;
                case 4:
                    result = (a.Exists(p) ? 1 : 0) + (b.Exists(p) ? 1 : 0) + (c.Exists(p) ? 1 : 0) + (d.Exists(p) ? 1 : 0) + (e.Exists(p) ? 1 : 0) >= 4;
                    break;
                case 5:
                    result = a.Exists(p) && b.Exists(p) && c.Exists(p) && d.Exists(p) && e.Exists(p);
                    break;
            }
            return result;
        }

        void inladen()
        {
            string[] afstanden = File.ReadAllLines(@"..\..\afstanden.txt");

            int maxLenght = int.Parse(afstanden[afstanden.Length - 1].Split(';')[0]);

            afstandenMatrix = new AfstandRijtijd[maxLenght + 1, maxLenght + 1];

            for (int i = 1; i < afstanden.Length - 1; i++)
            {
                string rij = afstanden[i];
                string[] info = rij.Split(';');
                int id1 = int.Parse(info[0]);
                int id2 = int.Parse(info[1]);
                int afstand = int.Parse(info[2]);
                int rijtijd = int.Parse(info[3]);
                afstandenMatrix[id1, id2] = new AfstandRijtijd(afstand, rijtijd);
            }
            
            string[] orderbestand = File.ReadAllLines(@"..\..\orderbestand.txt");

            ordersDict = new Dictionary<int, OrderDescription>();

            for (int i = 1; i < orderbestand.Length - 1; i++)
            {
                string rij = orderbestand[i];

                string[] info = rij.Split(';');
                int a = int.Parse(info[0]);
                string b = info[1];
                int c = int.Parse(info[2][0].ToString());
                int d = int.Parse(info[3]);
                int e = int.Parse(info[4]);
                double f = double.Parse(info[5]);
                int g = int.Parse(info[6]);
                int h = int.Parse(info[7]);
                int ib = int.Parse(info[8]);

                ordersDict[a] = new OrderDescription(a, b, c, d, e, f, g, h, ib);
            }
        }
    }
}

