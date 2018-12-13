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
        int kmax = 100000;
        double temp = 100d;
        Random rng = new Random();
        Tuple<Truck, Truck> bestSolution = new Tuple<Truck, Truck>(new Truck(0), new Truck(1));
        AfstandRijtijd[,] afstandenMatrix = null;
        Dictionary<int, OrderDescription> orders = null;

        public LocalSearch()
        {
            inladen(afstandenMatrix, orders);
        }

        public void FindSolution(Tuple<Truck, Truck> trucks)
        {
            Tuple<Truck, Truck> currentSolution = new Tuple<Truck, Truck>(new Truck(0), new Truck(1));
            for (int i = 0; i < kmax; i++)
            {
                if(i % 10000 == 0)
                {
                    temp *= 0.99d;
                }

                Tuple<Truck, Truck> randomNeighbor = newNeighbor(currentSolution);

                if (acceptanceChance(currentSolution, randomNeighbor, temp) >= rng.NextDouble() || solutionValue(randomNeighbor) > solutionValue(currentSolution))
                {
                    currentSolution = randomNeighbor;
                    if (solutionValue(currentSolution) > solutionValue(bestSolution))
                    {
                        bestSolution = randomNeighbor;
                    }
                }

                trucks = bestSolution;
            }
        }

        double acceptanceChance(Tuple<Truck,Truck> currentSolution, Tuple<Truck,Truck> randomNeighbor, double temp)
        {
            return Math.Exp(((solutionValue(currentSolution) - solutionValue(randomNeighbor) ) / temp ) );
        }

        // TODO !!!
        Tuple<Truck, Truck> newNeighbor(Tuple<Truck, Truck> trucks)
        {

            return null;
        }

        double solutionValue(Tuple<Truck, Truck> trucks)
        {
            double solution = 0;
            foreach(KeyValuePair<int, OrderDescription> kvp in orders)
            {
                OrderDescription order = kvp.Value;
                if(!orderVoldaan(trucks, order))
                    solution += order.LedigingDuurMinuten * order.Frequentie * 3;
            }
            solution += addTijden(trucks.Item1);
            solution += addTijden(trucks.Item2);
            return solution;
        }

        double addTijden(Truck truck)
        {
            double solution = 0;
            foreach (List<int> dagen in truck.Dagen)
            {
                for (int i = 1; i < dagen.Count; i++)
                {
                    solution += afstandenMatrix[dagen[i - 1], dagen[i]].Rijtijd;
                }
            }
            return solution;
        }

        // TODO
        bool orderVoldaan(Tuple<Truck, Truck> trucks, OrderDescription order)
        {
            bool result = true;
            Predicate<int> p = i => i == order.Order;
            List<int> a = (List<int>)trucks.Item1.Dagen[0].Concat(trucks.Item2.Dagen[0]);
            List<int> b = (List<int>)trucks.Item1.Dagen[1].Concat(trucks.Item2.Dagen[1]);
            List<int> c = (List<int>)trucks.Item1.Dagen[2].Concat(trucks.Item2.Dagen[2]);
            List<int> d = (List<int>)trucks.Item1.Dagen[3].Concat(trucks.Item2.Dagen[3]);
            List<int> e = (List<int>)trucks.Item1.Dagen[4].Concat(trucks.Item2.Dagen[4]);

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

        void inladen(AfstandRijtijd[,] afstandenMatrix, Dictionary<int, OrderDescription> orders)
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

            orders = new Dictionary<int, OrderDescription>();

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

                orders[a] = new OrderDescription(a, b, c, d, e, f, g, h, ib);
            }
        }
    }
}
