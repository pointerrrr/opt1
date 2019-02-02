using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Groot.Data;

namespace Groot
{
    public class Truck
    {
        public List<Tuple<List<int>, Capaciteit>>[] Dagen;
        public double[] Rijtijden;

        public Truck()
        {
            Dagen = new List<Tuple<List<int>, Capaciteit>>[5];
            Rijtijden = new double[5];
            for(int i = 0; i < 5; i++)
            {
                Rijtijden[i] = 60;
                Dagen[i] = new List<Tuple<List<int>, Capaciteit>>();
                Dagen[i].Add(new Tuple<List<int>, Capaciteit>(new List<int>(), new Capaciteit(0)));
                Dagen[i].Add(new Tuple<List<int>, Capaciteit>(new List<int>(), new Capaciteit(0)));
            }
        }

        public Truck Copy()
        {
            Truck res = new Truck();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < Dagen[i].Count; j++)
                {
                    if (j > 1)
                        res.Dagen[i].Add(new Tuple<List<int>, Capaciteit>(new List<int>(), new Capaciteit(Dagen[i][j].Item2.Value)));
                    else
                        res.Dagen[i][j].Item2.SetValue(Dagen[i][j].Item2.Value);
                    for (int k = 0; k < Dagen[i][j].Item1.Count; k++)
                        res.Dagen[i][j].Item1.Add(Dagen[i][j].Item1[k]);
                }
                
            }
            Rijtijden.CopyTo(res.Rijtijden, 0);
            return res;
        }

        public void AddOrder(int orderId, int index, int dag, int route, double newCap)
        {
            Dagen[dag][route].Item1.Insert(index, orderId);
            Dagen[dag][route].Item2.SetValue(newCap);
        }

        public void RemoveOrder(int index, int dag, int route, double newCap)
        {
            Dagen[dag][route].Item1.RemoveAt(index);
            Dagen[dag][route].Item2.SetValue(newCap);
        }

        public void AddDumpen(int dag, int route, int index, double newCap1, double newCap2)
        {
            List<int> temp = new List<int>();
            temp.AddRange(Dagen[dag][route].Item1.Take(index));
            Dagen[dag][route].Item1.RemoveRange(0, index);
            Dagen[dag].Add(new Tuple<List<int>, Capaciteit>(temp, new Capaciteit(newCap1)));
            Dagen[dag][route].Item2.SetValue(newCap2);
        }

        public void RemoveDumpen(int dag, int route, double newCap)
        {
            Dagen[dag][route].Item1.AddRange(Dagen[dag][route + 1].Item1);
            Dagen[dag][route].Item2.SetValue(newCap);
            Dagen[dag].RemoveAt(route + 1);
        }

        public double getRijTijd(int dag)
        {
            double res = Rijtijden[dag];
            //res += 30 * Dagen[dag].Count;

            return res;
        }

        public double getCapaciteit(int dag, int route)
        {
            double result = 0;
            int i = 0;
            while (i < Dagen[dag][route].Item1.Count)
            {
                result += OrdersDict[Dagen[dag][route].Item1[i]].AantContainers * OrdersDict[Dagen[dag][route].Item1[i]].VolumePerContainer;
                i++;
            }

            return result;
        }
    }

    public class Capaciteit
    {
        public double Value;

        public Capaciteit(double value)
        {
            Value = value;
        }

        public void SetValue(double val)
        {
            Value = val;
        }
    }
}
