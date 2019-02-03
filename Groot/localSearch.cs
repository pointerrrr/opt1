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
            InitChoiceChances();
        }

        Solution GetStartSolution()
        {
            Solution res = new Solution();
            
            res.ValidCheck = new Dictionary<int, ValidArray>();
            for (int i = 0; i < Orders.Length; i++)
            {
                res.ValidCheck[Orders[i].Key] = new ValidArray(f: Orders[i].Value.Frequentie);
                res.Strafpunten += Orders[i].Value.Frequentie * Orders[i].Value.LedigingDuurMinuten * PenaltyModifier;
            }
            return res;
        }

        public static int lastDecrementFound = 0;

        public Solution FindSolution()
        {
            Solution currentSolution = GetStartSolution();

            Solution bestSolution = currentSolution.Copy();
            for (int reiterations = 0; reiterations < 10; reiterations++)
            {
                T = startT;
                lastDecrementFound = 0;
                for (int i = 0; i < MaxIterations || lastDecrementFound < 10000000; i++)
                {
                    if (i % Q == 0)
                    {
                        T *= 0.99d;
                    }
                    lastDecrementFound++;
                    SolutionData newData = new SolutionData(currentSolution, findChoice(6));
                    if (((newData.Value <= currentSolution.Value || acceptanceChance(currentSolution.Value, newData.Value, T) >= RNG.NextDouble()) && newData.allow))
                    {
                        if (!newData.accepted)
                            continue;


                        currentSolution.Mutation(newData);

                        if (currentSolution.Value < bestSolution.Value)
                        {
                            bestSolution = currentSolution.Copy();
                            lastDecrementFound = 0;
                        }
                    }
                }
            }
            return bestSolution;
        }

        int[] choices;
        int[] sumRight;

        void InitChoiceChances()
        {
            choices = new int[] { 200, 10, 50, 50, 50, 50, 1, 1, 0, 0 };
            sumRight = new int[choices.Length];

            sumRight[0] = choices[0];

            for (int i = 1; i < choices.Length; i++)
                sumRight[i] = choices[i] + sumRight[i - 1];
        }


        int findChoice(int limit)
        {
            int max = 0;

            for (int i = 0; i < sumRight.Length; i++)
                max+= choices[i];

            int choice = RNG.Next(max);

            int res = 0;

            for(int i = 0; i < limit; i++)
                if(choice < sumRight[i])
                {
                    res = i;
                    break;
                }


            return res;
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
                while (currentSolution[0].Rijtijden[i] < 600 && currentSolution[0].Dagen[i][0].Item2.Value < 75000)
                {
                    currentSolution.AddSpecificOrder(currentSolution[0], i, 0, j + 1, closest1[j]);
                    j++;
                }
                j = 0;
                while (currentSolution[1].Rijtijden[i] < 600 && currentSolution[1].Dagen[i][0].Item2.Value < 75000)
                {
                    currentSolution.AddSpecificOrder(currentSolution[1], i, 0, j + 1, closest2[j]);
                    j++;
                }
            }
        }
    }
}

