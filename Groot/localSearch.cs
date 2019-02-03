using System;
using System.Collections.Generic;

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
                    SolutionData newData = new SolutionData(currentSolution, FindChoice(6));
                    if (((newData.Value <= currentSolution.Value || AcceptanceChance(currentSolution.Value, newData.Value, T) >= RNG.NextDouble()) && newData.allow))
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
            choices = new int[] { 200, 10, 50, 50, 50, 50};
            sumRight = new int[choices.Length];

            sumRight[0] = choices[0];

            for (int i = 1; i < choices.Length; i++)
                sumRight[i] = choices[i] + sumRight[i - 1];
        }


        int FindChoice(int limit)
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

        private double AcceptanceChance(double currentValue, double newValue, double T)
        {
            double res = Math.Exp((currentValue - newValue) / T);
            return res;
        }
    }
}

