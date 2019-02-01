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

            res.ValidCheck = new Dictionary<int, ValidArray>();
            for (int i = 0; i < Orders.Length; i++)
            {
                res.ValidCheck[Orders[i].Key] = new ValidArray(f: Orders[i].Value.Frequentie);
                res.Strafpunten += Orders[i].Value.Frequentie * Orders[i].Value.LedigingDuurMinuten * 3d;
            }
            return res;
        }

        int lastDecrementFound = 0;

        public Solution FindSolution()
        {
            Solution currentSolution = GetStartSolution();

            AddCloud(currentSolution);

            Solution bestSolution = currentSolution.Copy();

            for (int i = 0; i < MaxIterations/* || lastDecrementFound < 1*/; i++)
            {
                if (i % Q == 0)
                {
                    T *= 0.99d;
                }

                SolutionData newData = new SolutionData(currentSolution, RNG.Next(7));
                if (newData.Value <= currentSolution.Value || acceptanceChance(currentSolution.Value, newData.Value, T) >= RNG.NextDouble())
                {
                    currentSolution.Mutation(newData);

                    //Solution randomNeighbor = currentSolution.Copy.RandomMutation();
                    //lastDecrementFound++;
                    bool allow = true;
                    for (int j = 0; j < 5; j++)
                    {
                        if (currentSolution.Truck1.Rijtijden[j] >= 12 * 60 || currentSolution.Truck2.Rijtijden[j] >= 12 * 60)
                        {
                            allow = false;
                            goto Checked;
                        }
                        for (int k = 0; k < currentSolution.Truck1.Dagen[j].Count; k++)
                        {
                            if (currentSolution.Truck1.Dagen[j][k].Item2.Value > 100000)
                            {
                                allow = false;
                                goto Checked;
                            }
                        }
                        for (int k = 0; k < currentSolution.Truck2.Dagen[j].Count; k++)
                        {
                            if (currentSolution.Truck2.Dagen[j][k].Item2.Value > 100000)
                            {
                                allow = false;
                                goto Checked;
                            }
                        }
                    }
                Checked:
                    if (newData.Value <= currentSolution.Value || allow)
                        if (currentSolution.Value < bestSolution.Value)
                        {
                            bestSolution = currentSolution.Copy();
                            //lastDecrementFound = 0;
                        }
                }
            }
            /*
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
                        bestSolution = currentSolution.Copy();
                        lastDecrementFound = 0;
                    }
                }
            }*/

            return bestSolution;
        }

        double[] reses = new double[1000];
        double avg = 0;
        int counter = 0;
        private double acceptanceChance(double currentValue, double newValue, double T)
        {
            double res = Math.Exp((currentValue - newValue) / T);
            if(counter < 1000)
                reses[counter++] = res;
            else
                avg = reses.Average();

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
                int index = RNG.Next(Orders.Length);
                currentSolution.AddSpecificOrder(currentSolution[0], i, 0, 0, Orders[index].Value.Order);

                index = RNG.Next(0, Orders.Length);
                currentSolution.AddSpecificOrder(currentSolution[1], i, 0, 0, Orders[index].Value.Order);


                int[] closest1 = ClosestBedrijven(1177, OrdersDict[currentSolution[0].Dagen[i][0].Item1[0]].MatrixID);
                int[] closest2 = ClosestBedrijven(1177, OrdersDict[currentSolution[1].Dagen[i][0].Item1[0]].MatrixID);
                int j = 0;
                while (currentSolution[0].Rijtijden[i] < 660)
                {
                    currentSolution.AddSpecificOrder(currentSolution[0], i, 0, j + 1, closest1[j]);
                    j++;
                }
                j = 0;
                while (currentSolution[1].Rijtijden[i] < 660)
                {
                    currentSolution.AddSpecificOrder(currentSolution[1], i, 0, j + 1, closest2[j]);
                    j++;
                }
            }
        }
    }
}

