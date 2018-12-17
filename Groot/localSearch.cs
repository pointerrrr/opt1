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
        public int kmax = 1000000;
        public double temp = 500000000;
        double startTemp = 0;
        public int tempDecrease = 100;
        public int aantalBedrijvenStart = 0;
        public string startOplossing = null;
        //public string startOplossing = @"out.txt";

        public static AfstandRijtijd[,] afstandenMatrix = null;
        public static Dictionary<int, OrderDescription> ordersDict = null;
        public static OrderDescription[] orders = null;

        Random rng = new Random();
        Solution bestSolution;
        private bool emptyStart = false;
        private double MaxPenalty;

        public LocalSearch()
        {
            inladen();
            orders = ordersDict.Values.ToArray();
            MaxPenalty = solutionValue(new Solution(new Truck(1), new Truck(2)));
        }

        int[] closestBedrijven(int aantal, int matrixID)
        {
            int[] res = new int[aantal];
            List<Tuple<int,AfstandRijtijd>> pres = new List<Tuple<int, AfstandRijtijd>>();
            for (int i = 0; i < 1099; i++)
                pres.Add (new Tuple<int, AfstandRijtijd>(i, afstandenMatrix[matrixID, i]));

            Comparison<Tuple<int, AfstandRijtijd>> comp = (a,b) => { return a.Item2.Rijtijd - b.Item2.Rijtijd; };

            pres.Sort(comp);
            pres.RemoveAt(0);

            Tuple<int, AfstandRijtijd>[] array = null;
            array = pres.Take(aantal).ToArray();
            for(int i = 0; i < array.Length; i++)
            {
                res[i] = orders.First((o) => { return o.MatrixID == array[i].Item1; }).Order;
            }

            return res;
        }

        void addCloud(Solution currentSolution)
        {
            for (int i = 0; i < 5; i++)
            {
                int index = rng.Next(0, orders.Length);
                currentSolution.Item1.AddBedrijf(orders[index].Order, 0, i);

                index = rng.Next(0, orders.Length);
                currentSolution.Item2.AddBedrijf(orders[index].Order, 0, i);


                int[] closest1 = closestBedrijven(1177, ordersDict[currentSolution.Item1.Dagen[i][0]].MatrixID);
                int[] closest2 = closestBedrijven(1177, ordersDict[currentSolution.Item2.Dagen[i][0]].MatrixID);
                int j = 0;
                while(currentSolution.Item1.RijTijden[i] < 36000-1800)
                {
                    currentSolution.Item1.AddBedrijf(closest1[j], j + 1, i);
                    j++;
                }
                j = 0;
                while (currentSolution.Item2.RijTijden[i] < 36000-1800)
                {
                    currentSolution.Item2.AddBedrijf(closest2[j], j + 1, i);
                    j++;
                }
                currentSolution.Item1.AddBedrijf(0, aantalBedrijvenStart + 1, i);
                currentSolution.Item2.AddBedrijf(0, aantalBedrijvenStart + 1, i);
            }
        }

        public Solution FindSolution(Solution trucks)
        {
            startTemp = temp;
            Solution currentSolution = trucks.Copy();
            foreach(KeyValuePair<int,OrderDescription> kvp in ordersDict)
            {
                currentSolution.ValidCheck[kvp.Key] = new ValidArray(f: kvp.Value.Frequentie);
            }

            currentSolution.Strafpunten = MaxPenalty;
            
            orders = ordersDict.Values.ToArray();

            if (startOplossing != null)
            {
                startOplossingInladen(startOplossing, currentSolution);
                addCloud(currentSolution);
            }
            else if (!emptyStart)
            /*for (int i = 0; i < aantalBedrijvenStart; i++)
            {
                int index = rng.Next(0, orders.Length);
                currentSolution.Item1.AddBedrijf(orders[index].Order, i % (aantalBedrijvenStart / 5), i / (aantalBedrijvenStart / 5));

                index = rng.Next(0, orders.Length);
                currentSolution.Item2.AddBedrijf(orders[index].Order, i % (aantalBedrijvenStart / 5), i / (aantalBedrijvenStart / 5));
            }*/
            {
                addCloud(currentSolution);
            }

            return currentSolution;

            int count = 0;
            int untilStopped = 0;
            bestSolution = currentSolution.Copy();
            setTemp:
            temp = startTemp;            
            for (int i = 0; i < kmax; i++)
            {
                /*if (count++ < 1)
                {
                    addCloud(bestSolution);
                    currentSolution = bestSolution.Copy();
                        
                    untilStopped = 0;
                    goto setTemp;
                }*/
                if(i % tempDecrease == 0)
                {
                    temp *= 0.99d;
                }

                Solution randomNeighbor = newNeighbor(currentSolution);

                if (randomNeighbor.Value <= currentSolution.Value || acceptanceChance(currentSolution, randomNeighbor, temp) >= rng.NextDouble())
                {
                    currentSolution = randomNeighbor.Copy();
                    if (currentSolution.Value < bestSolution.Value)
                    {
                        bestSolution = randomNeighbor.Copy();
                        untilStopped = 0;
                    }
                }
                
            }
            if (count++ < 0)
                goto setTemp;
            return bestSolution;
        }

        int countChance = 0;
        double[] test = new double[1000];
        double acceptanceChance(Solution currentSolution, Solution randomNeighbor, double temp)
        {
            
            
            double res = Math.Exp((currentSolution.Value - randomNeighbor.Value) / temp);
            if (countChance < 1000)
            {
                test[countChance++] = res;
                
            }
            else
            {
                double testres = test.Sum() / 1000d;
            }
            return res;
        }

        Solution newNeighbor(Solution trucks)
        {
            Solution result = trucks.Copy();
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
                if (!orderVoldaan(trucks, order))
                    solution += order.LedigingDuurMinuten * order.Frequentie * 3d * 60d;
            }

            solution += 36000d * 150000d * 10;

            return solution;
        }
        

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

            for (int i = 1; i < orderbestand.Length; i++)
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

        public void startOplossingInladen(string path, Solution s)
        {
            string[] visits = File.ReadAllLines(path);
            Truck t1 = s.Item1;
            Truck t2 = s.Item2;

            List<int[]> orders = new List<int[]>();

            for (int i = 0; i < visits.Length; i++)
            {
                int[] order = new int[4];
                string[] visit = visits[i].Split(';');

                order[0] = int.Parse(visit[0]);
                order[1] = int.Parse(visit[1]);
                order[2] = int.Parse(visit[2]);
                order[3] = int.Parse(visit[3]);

                orders.Add(order);
            }

            Comparison<int[]> comp = (a, b) => { return compareOrder(a, b); } ;
            orders.Sort(comp);

            for (int i = 0; i < orders.Count; i++)
            {
                switch (orders[i][0])
                {
                    case 1:
                        t1.AddBedrijf(orders[i][3], orders[i][2] - 1, orders[i][1] - 1);
                        break;
                    case 2:
                        t2.AddBedrijf(orders[i][3], orders[i][2] - 1, orders[i][1] - 1);
                        break;
                }
            }

            for(int i = 0; i < 5; i++)
            {
                if (t1.Dagen[i].Count != 1)
                    t1.RemoveBedrijf(t1.Dagen[i].Count - 2, i);
                if (t2.Dagen[i].Count != 1)
                    t2.RemoveBedrijf(t2.Dagen[i].Count - 2, i);
            }
        }

        int compareOrder(int[] a, int[] b)
        {
            if (a[0] != b[0])
                return a[0] - b[0];
            else if (a[1] != b[1])
                return a[1] - b[1];
            else
                return a[2] - b[2];    
        }
    }
}

