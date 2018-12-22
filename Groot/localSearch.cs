using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using static Groot.Data;

namespace Groot
{
    class LocalSearch
    {
        private int MaxIterations, Q;
        private double startT = 0;
        private double T;
        
        public LocalSearch(int MaxIterations, double T, int Q)
        {
            this.MaxIterations = MaxIterations; this.T = T; this.Q = Q; startT = T;
        }

        Solution GetStartSolution()
        {
            Solution res = new Solution();
            for(int i = 0; i < 5; i++)
            {
                res[0].Dagen[i].Add(0);
                res[0].Rijtijden[i] += 30;
                res[1].Dagen[i].Add(0);
                res[1].Rijtijden[i] += 30;
                res.Rijtijd += 60;
                res.UpdateRijtijdStrafMin(60d * 10d, 30);
                res.UpdateRijtijdStrafMin(60d * 10d, 30);
            }
            res.ValidCheck = new Dictionary<int, ValidArray>();
            for (int i = 0; i < Orders.Length; i++)
            {
                res.ValidCheck[Orders[i].Key] = new ValidArray(f: Orders[i].Value.Frequentie);
                res.Strafpunten += Orders[i].Value.Frequentie * Orders[i].Value.LedigingDuurMinuten * 3d;
                //res.StrafIntern += Orders[i].Value.Frequentie * Orders[i].Value.LedigingDuurMinuten * 100d;
                
            }
            return res;
        }

        int lastDecrementFound = 0;

        public Solution FindSolution()
        {
            Solution currentSolution = GetStartSolution();

            AddCloud(currentSolution);

            Solution bestSolution = currentSolution.Copy();

            for (int i = 0; i < MaxIterations || lastDecrementFound < 1; i++)
            {
                if (i % Q == 0)
                {
                    T *= 0.99d;
                }

                Solution randomNeighbor = currentSolution.Copy().RandomMutation();
                lastDecrementFound++;
                if (randomNeighbor.Value <= currentSolution.Value || acceptanceChance(currentSolution, randomNeighbor, T) >= RNG.NextDouble())
                {
                    currentSolution = randomNeighbor.Copy();
                    if (currentSolution.Value < bestSolution.Value)
                    {
                        bestSolution = randomNeighbor.Copy();
                        lastDecrementFound = 0;
                    }
                }
            }

            lastDecrementFound = 0;
            T = startT;

            for(int i = 0; i < MaxIterations && lastDecrementFound < 100000; i++)
            {
                if (i % Q == 0)
                {
                    T *= 0.99d;
                }

                Solution randomNeighbor = currentSolution.Copy().RandomMutation(1);

                lastDecrementFound++;
                if (randomNeighbor.Value <= currentSolution.Value || acceptanceChance(currentSolution, randomNeighbor, T) >= RNG.NextDouble())
                {
                    currentSolution = randomNeighbor.Copy();
                    if (currentSolution.Value < bestSolution.Value)
                    {
                        bestSolution = randomNeighbor.Copy();
                        lastDecrementFound = 0;
                    }
                }
            }

            return bestSolution;
        }

        double[] reses = new double[1000];
        int counter = 0;
        private double acceptanceChance(Solution currentSolution, Solution randomNeighbor, double T)
        {
            double res = Math.Exp(((currentSolution.Value - randomNeighbor.Value)) / T);
            if(counter < 1000)
            {
                reses[counter++] = res;
            }
            else
            {
                double avg = reses.Average();
            }
            return res;
        }

        int[] ClosestBedrijven(int aantal, int matrixID)
        {
            int[] res = new int[aantal];
            List<Tuple<int, AfstandRijtijd>> pres = new List<Tuple<int, AfstandRijtijd>>();
            for (int i = 0; i < 1099; i++)
                pres.Add(new Tuple<int, AfstandRijtijd>(i, AfstandenMatrix[matrixID, i]));

            Comparison<Tuple<int, AfstandRijtijd>> comp = (a, b) => { return (int)(a.Item2.Rijtijd - b.Item2.Rijtijd); };

            pres.Sort(comp);
            pres.RemoveAt(0);

            Tuple<int, AfstandRijtijd>[] array = null;
            array = pres.Take(aantal).ToArray();
            for (int i = 0; i < array.Length; i++)
            {
                res[i] = Orders.First((o) => { return o.Value.MatrixID == array[i].Item1; }).Value.Order;
            }

            return res;
        }

        void AddCloud(Solution currentSolution)
        {
            for (int i = 0; i < 5; i++)
            {
                int index = RNG.Next(0, Orders.Length);
                currentSolution[0].AddOrder(Orders[index].Value.Order, 0, i);

                index = RNG.Next(0, Orders.Length);
                currentSolution[1].AddOrder(Orders[index].Value.Order, 0, i);


                int[] closest1 = ClosestBedrijven(1177, OrdersDict[currentSolution[0].Dagen[i][0]].MatrixID);
                int[] closest2 = ClosestBedrijven(1177, OrdersDict[currentSolution[1].Dagen[i][0]].MatrixID);
                int j = 0;
                while (currentSolution[0].Rijtijden[i] < 660 - 30)
                {
                    currentSolution.AddSpecificOrder(currentSolution[0], i, j + 1, closest1[j]);
                    j++;
                }
                j = 0;
                while (currentSolution[1].Rijtijden[i] < 660 - 30)
                {
                    currentSolution.AddSpecificOrder(currentSolution[1], i, j + 1, closest2[j]);
                    j++;
                }
                //currentSolution[0].AddOrder(0, aantalBedrijvenStart + 1, i);
                //currentSolution[1].AddOrder(0, aantalBedrijvenStart + 1, i);
            }
        }
    }
}

