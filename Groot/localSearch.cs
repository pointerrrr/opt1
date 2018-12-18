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
        int maxIterations, Q;
        double T;
        
        public LocalSearch(int maxIterations, double T, int Q)
        {
            this.maxIterations = maxIterations; this.T = T; this.Q = Q;
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
            }
            res.ValidCheck = new Dictionary<int, ValidArray>();
            for (int i = 0; i < Orders.Length; i++)
                res.ValidCheck[Orders[i].Key] = new ValidArray(f: Orders[i].Value.Frequentie);
            return res;
        }

        public Solution FindSolution()
        {
            Solution bestSolution = GetStartSolution();

            Solution currentSolution = bestSolution.Copy();

            for (int i = 0; i < maxIterations; i++)
            {
                if (i % Q == 0)
                {
                    T *= 0.99d;
                }

                Solution randomNeighbor = currentSolution.Copy().RandomMutation();

                if (randomNeighbor.Value >= currentSolution.Value || acceptanceChance(currentSolution, randomNeighbor, T) >= RNG.NextDouble())
                {
                    currentSolution = randomNeighbor.Copy();
                    if (currentSolution.Value > bestSolution.Value)
                    {
                        bestSolution = randomNeighbor.Copy();
                    }
                }
            }
            return bestSolution;
        }

        private double acceptanceChance(Solution currentSolution, Solution randomNeighbor, double T)
        {
            double res = Math.Exp((currentSolution.Value - randomNeighbor.Value) / T);
            return res;
        }
    }
}

