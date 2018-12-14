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
        int kmax = 10000;
        double temp = 100d;
        Random rng = new Random();
        Solution bestSolution = new Solution(new Truck(1), new Truck(2));
        AfstandRijtijd[,] afstandenMatrix = null;
        Dictionary<int, OrderDescription> ordersDict = null;
        OrderDescription[] orders = null;

        public LocalSearch()
        {
            inladen();
            orders = ordersDict.Values.ToArray();
        }

        public Solution FindSolution(Solution trucks)
        {
            Solution currentSolution = new Solution(trucks.Item1.Copy(), trucks.Item2.Copy());
            bestSolution = currentSolution;
            orders = ordersDict.Values.ToArray();
            for (int i = 0; i < 50; i++)
            {
                currentSolution.Item1.Dagen[i / 10].Insert(0, orders[rng.Next(0, orders.Length)].Order);

                currentSolution.Item2.Dagen[i / 10].Insert(0, orders[rng.Next(0, orders.Length)].Order);
            }

            for (int i = 0; i < kmax; i++)
            {
                if(i % 10000 == 0)
                {
                    temp *= 0.99d;
                }

                Solution randomNeighbor = newNeighbor(currentSolution);

                if (acceptanceChance(currentSolution, randomNeighbor, temp) >= rng.NextDouble() || solutionValue(randomNeighbor) < solutionValue(currentSolution))
                {
                    currentSolution = randomNeighbor;
                    if (solutionValue(currentSolution) > solutionValue(bestSolution))
                    {
                        bestSolution = randomNeighbor;
                    }
                }

                
            }
            return bestSolution;
        }

        double acceptanceChance(Solution currentSolution, Solution randomNeighbor, double temp)
        {
            return Math.Exp(((solutionValue(currentSolution) - solutionValue(randomNeighbor) ) / temp ) );
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
            int bedrijf = rng.Next(orders.Length);
            int dag = rng.Next(5);
            int plek;
            switch(truck)
            {
                case 0:
                    plek = rng.Next(1, trucks.Item1.Dagen[dag].Count - 1);
                    trucks.Item1.Dagen[dag].Insert(plek, orders[bedrijf].Order);
                    break;
                case 1:
                    plek = rng.Next(1, trucks.Item2.Dagen[dag].Count - 1);
                    trucks.Item2.Dagen[dag].Insert(plek, orders[bedrijf].Order);
                    break;
            }
            return trucks;
        }

        Solution removeBedrijf(Solution trucks)
        {
            int truck = rng.Next(2);
            int dag = rng.Next(5);
            int plek;
            int count = 0;
            switch(truck)
            {
                case 0:
                    plek = rng.Next(1, trucks.Item1.Dagen[dag].Count - 1);
                    if(trucks.Item1.Dagen[dag][plek] == 0 && count++ < 10)
                        goto case 0;
                    if (trucks.Item1.Dagen[dag][plek] != 0)
                        trucks.Item1.Dagen[dag].RemoveAt(plek);
                    break;
                case 1:
                    plek = rng.Next(1, trucks.Item2.Dagen[dag].Count - 1);
                    if (trucks.Item2.Dagen[dag][plek] == 0 && count++ < 10)
                        goto case 1;
                    if(trucks.Item2.Dagen[dag][plek] != 0)
                        trucks.Item2.Dagen[dag].RemoveAt(plek);
                    break;
            }
            return trucks;
        }

        Solution swapOrder(Solution trucks)
        {
            int truck = rng.Next(2);
            int dag = rng.Next(5);
            int plek1;
            int plek2;
            int plek1res;
            switch (truck)
            {
                case 0:
                    plek1 = rng.Next(1, trucks.Item1.Dagen[dag].Count - 1);
                    plek2 = rng.Next(1, trucks.Item1.Dagen[dag].Count - 1);
                    plek1res = trucks.Item1.Dagen[dag][plek1];
                    trucks.Item1.Dagen[dag][plek1] = trucks.Item1.Dagen[dag][plek2];
                    trucks.Item1.Dagen[dag][plek2] = plek1res;
                    break;
                case 1:
                    plek1 = rng.Next(1, trucks.Item2.Dagen[dag].Count - 1);
                    plek2 = rng.Next(1, trucks.Item2.Dagen[dag].Count - 1);
                    plek1res = trucks.Item2.Dagen[dag][plek1];
                    trucks.Item2.Dagen[dag][plek1] = trucks.Item2.Dagen[dag][plek2];
                    trucks.Item2.Dagen[dag][plek2] = plek1res;
                    break;
            }
            return trucks;
        }

        Solution addDumpen(Solution trucks)
        {
            int truck = rng.Next(2);
            int dag = rng.Next(5);
            int plek;
            switch (truck)
            {
                case 0:
                    plek = rng.Next(1, trucks.Item1.Dagen[dag].Count);
                    trucks.Item1.Dagen[dag].Insert(plek, 0);
                    break;
                case 1:
                    plek = rng.Next(1, trucks.Item2.Dagen[dag].Count);
                    trucks.Item2.Dagen[dag].Insert(plek, 0);
                    break;
            }
            return trucks;
        }

        Solution removeDumpen(Solution trucks)
        {
            int truck = rng.Next(2);
            int dag = rng.Next(5);
            
            int count;
            int plek;
            switch (truck)
            {
                case 0:
                    count = trucks.Item1.Dagen[dag].Count(a => { return a == 0; });
                    if (count < 2)
                        goto default;
                    plek = rng.Next(0, count - 1);
                    count = 0;
                    for (int i = 0; i < trucks.Item1.Dagen[dag].Count; i++)
                    {
                        if (trucks.Item1.Dagen[dag][i] == 0)
                        {
                            if (count == plek)
                            {
                                trucks.Item1.Dagen[dag].RemoveAt(i);
                                break;
                            }
                            count++;
                        }
                    }
                    break;
                case 1:
                    count = trucks.Item2.Dagen[dag].Count(a => { return a == 0; });
                    if (count < 2)
                        goto default;
                    plek = rng.Next(0, count - 1);
                    count = 0;
                    for(int i = 0; i < trucks.Item2.Dagen[dag].Count; i++)
                    {
                        if (trucks.Item2.Dagen[dag][i] == 0)
                        {
                            if (count == plek)
                            {
                                trucks.Item2.Dagen[dag].RemoveAt(i);
                                break;
                            }
                            count++;
                        }
                    }
                    break;
                default:
                    break;
            }
            return trucks;
        }

        Solution changeOrderDay(Solution trucks)
        {
            int truck = rng.Next(2);
            int dag1 = rng.Next(5);
            int dag2 = rng.Next(5);

            while (dag1 == dag2)
                dag2 = rng.Next(5);

            int plek1;
            int plek2;
            int plek1res;
            int count = 0;
            switch (truck)
            {
                case 0:
                    plek1 = rng.Next(1, trucks.Item1.Dagen[dag1].Count - 1);
                    plek2 = rng.Next(1, trucks.Item1.Dagen[dag2].Count - 1);
                    if ((trucks.Item1.Dagen[dag1][plek1] == 0 || trucks.Item1.Dagen[dag2][plek2] == 0) && count++ < 10)
                        goto case 0;
                    if (count < 10)
                    {
                        plek1res = trucks.Item1.Dagen[dag1][plek1];
                        trucks.Item1.Dagen[dag1][plek1] = trucks.Item1.Dagen[dag2][plek2];
                        trucks.Item1.Dagen[dag2][plek2] = plek1res;
                    }
                    break;
                case 1:
                    plek1 = rng.Next(1, trucks.Item2.Dagen[dag1].Count - 1);
                    plek2 = rng.Next(1, trucks.Item2.Dagen[dag2].Count - 1);
                    if ((trucks.Item2.Dagen[dag1][plek1] == 0 || trucks.Item2.Dagen[dag2][plek2] == 0) && count++ < 10)
                        goto case 1;
                    if (count < 10)
                    {
                        plek1res = trucks.Item2.Dagen[dag1][plek1];
                        trucks.Item2.Dagen[dag1][plek1] = trucks.Item2.Dagen[dag2][plek2];
                        trucks.Item2.Dagen[dag2][plek2] = plek1res;
                    }
                    break;
            }
            return trucks;
        }

        Solution changeOrderTruck(Solution trucks)
        {
            int truck = rng.Next(2);
            int dag1 = rng.Next(5);
            int dag2 = rng.Next(5);

            int plek1;
            int plek2;
            int plek1res;
            int counter = 0;
            switch (truck)
            {
                case 0:
                    plek1 = rng.Next(1, trucks.Item1.Dagen[dag1].Count - 1);
                    plek2 = rng.Next(1, trucks.Item2.Dagen[dag2].Count - 1);
                    if ((trucks.Item1.Dagen[dag1][plek1] == 0 || trucks.Item2.Dagen[dag2][plek2] == 0) && counter++ < 10)
                        goto case 0;
                    if (counter < 10)
                    {
                        plek1res = trucks.Item1.Dagen[dag1][plek1];
                        trucks.Item1.Dagen[dag1].RemoveAt(plek1);
                        trucks.Item2.Dagen[dag2].Insert(plek2, plek1res);
                    }
                    break;
                case 1:
                    plek1 = rng.Next(1, trucks.Item2.Dagen[dag1].Count - 1);
                    plek2 = rng.Next(1, trucks.Item1.Dagen[dag2].Count - 1);
                    if ((trucks.Item2.Dagen[dag1][plek1] == 0 || trucks.Item1.Dagen[dag2][plek2] == 0) && counter++ < 10)
                        goto case 1;
                    if (counter < 10)
                    {
                        plek1res = trucks.Item2.Dagen[dag1][plek1];
                        trucks.Item2.Dagen[dag1].RemoveAt(plek1);
                        trucks.Item1.Dagen[dag2].Insert(plek2, plek1res);
                    }
                    break;
            }
            return trucks;
        }

        double solutionValue(Solution trucks)
        {
            double solution = 0;
            foreach(KeyValuePair<int, OrderDescription> kvp in ordersDict)
            {
                OrderDescription order = kvp.Value;
                if(!orderVoldaan(trucks, order))
                    solution += order.LedigingDuurMinuten * order.Frequentie * 3;
            }
            solution += addTijden(trucks.Item1);
            solution += addTijden(trucks.Item2);
            if (solution > 690 * 2 * 5)
                return solution * 10;
            return solution;
        }

        double addTijden(Truck truck)
        {
            double solution = 0;
            foreach (List<int> dagen in truck.Dagen)
            {
                for (int i = 1; i < dagen.Count; i++)
                {
                    int a = dagen[i-1], b = dagen[i];
                    if (a == 0)
                        a = 287;
                    else
                        a = ordersDict[a].MatrixID;
                    if (b == 0)
                        b = 287;
                    else
                        b = ordersDict[b].MatrixID;
                    solution += afstandenMatrix[a,b].Rijtijd;
                }
            }
            return solution;
        }

        // TODO
        bool orderVoldaan(Solution trucks, OrderDescription order)
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

        // TODO!!!
        double temperature(int i, int kmax)
        {
            return (double)i / kmax;
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

