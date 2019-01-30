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
                Dagen[i] = new List<Tuple<List<int>, Capaciteit>>();
                Dagen[i].Add(new Tuple<List<int>, Capaciteit>(new List<int>(), new Capaciteit(0)));
            }
        }

        public Truck Copy()
        {
            Truck res = new Truck();
            for (int i = 0; i < 5; i++)
                for (int j = 0; j < Dagen[i].Count; j++)
                {
                    res.Dagen[i].Add(new Tuple<List<int>, Capaciteit>(new List<int>(), new Capaciteit(Dagen[i][j].Item2.Value)));
                    for (int k = 0; k < Dagen[i][j].Item1.Count; k++)
                        res.Dagen[i][j].Item1.Add(Dagen[i][j].Item1[k]);
                }
            Rijtijden.CopyTo(res.Rijtijden, 0);
            return res;
        }

        public void AddOrder(int orderId, int index, int dag, int route)
        {
            int a = 0, b = 0, c = 0;

            if (index == 0 || Dagen[dag][route].Item1[index -1] == 0)
                a = 287;
            else
                a = OrdersDict[Dagen[dag][route].Item1[index - 1]].MatrixID;

            b = OrdersDict[orderId].MatrixID;
            Rijtijden[dag] += OrdersDict[orderId].LedigingDuurMinuten;

            if (index >= Dagen[dag][route].Item1.Count - 1)
                c = 287;
            else
                c = OrdersDict[Dagen[dag][route].Item1[index]].MatrixID;

            Rijtijden[dag] -= AfstandenMatrix[a, c].Rijtijd;
            Rijtijden[dag] += AfstandenMatrix[a, b].Rijtijd;
            Rijtijden[dag] += AfstandenMatrix[b, c].Rijtijd;

            Dagen[dag][route].Item2.SetValue(Dagen[dag][route].Item2.Value + OrdersDict[orderId].AantContainers * OrdersDict[orderId].VolumePerContainer);

            Dagen[dag][route].Item1.Insert(index, orderId);
        }

        public void RemoveOrder(int index, int dag, int route)
        {
            int a = 0, b = 0, c = 0;

            int orderId = Dagen[dag][route].Item1[index];

            if (index == 0 || Dagen[dag][route].Item1[index - 1] == 0)
                a = 287;
            else
                a = OrdersDict[Dagen[dag][route].Item1[index - 1]].MatrixID;

            b = OrdersDict[orderId].MatrixID;
            Rijtijden[dag] -= OrdersDict[orderId].LedigingDuurMinuten;

            if (index + 1 >= Dagen[dag][route].Item1.Count - 1)
                c = 287;
            else
                c = OrdersDict[Dagen[dag][route].Item1[index + 1]].MatrixID;

            Rijtijden[dag] += AfstandenMatrix[a, c].Rijtijd;
            Rijtijden[dag] -= AfstandenMatrix[a, b].Rijtijd;
            Rijtijden[dag] -= AfstandenMatrix[b, c].Rijtijd;

            Dagen[dag][route].Item2.SetValue(Dagen[dag][route].Item2.Value - OrdersDict[Dagen[dag][route].Item1[index]].AantContainers * OrdersDict[Dagen[dag][route].Item1[index]].VolumePerContainer);
            Dagen[dag][route].Item1.RemoveAt(index);
        }

        public void AddDumpen(int dag, int route, int index)
        {
            List<int> temp = new List<int>();
            temp.AddRange(Dagen[dag][route].Item1.Take(index));
            Dagen[dag][route].Item1.RemoveRange(0, index);
            Dagen[dag].Add(new Tuple<List<int>, Capaciteit>(temp, new Capaciteit(0)));

            int nieuweRoute = Dagen[dag].Count - 1;
            int maxNieuweRoute = Dagen[dag][nieuweRoute].Item1.Count - 1;

            Rijtijden[dag] -= AfstandenMatrix[OrdersDict[Dagen[dag][nieuweRoute].Item1[maxNieuweRoute]].MatrixID, OrdersDict[Dagen[dag][route].Item1[0]].MatrixID].Rijtijd;
            Rijtijden[dag] += AfstandenMatrix[287, OrdersDict[Dagen[dag][route].Item1[0]].MatrixID].Rijtijd;
            Rijtijden[dag] += AfstandenMatrix[OrdersDict[Dagen[dag][nieuweRoute].Item1[maxNieuweRoute]].MatrixID, 287].Rijtijd;

            Dagen[dag][nieuweRoute].Item2.SetValue(getCapaciteit(dag, nieuweRoute));
            Dagen[dag][route].Item2.SetValue(Dagen[dag][route].Item2.Value - Dagen[dag][nieuweRoute].Item2.Value);
        }

        public void RemoveDumpen(int dag, int route)
        {
            if (route >= Dagen[dag].Count - 1)
                return;

            int maxOud = Dagen[dag][route].Item1.Count - 1;

            Rijtijden[dag] -= AfstandenMatrix[287, OrdersDict[Dagen[dag][route + 1].Item1[0]].MatrixID].Rijtijd;
            Rijtijden[dag] -= AfstandenMatrix[OrdersDict[Dagen[dag][route].Item1[maxOud]].MatrixID, 287].Rijtijd;
            Rijtijden[dag] += AfstandenMatrix[OrdersDict[Dagen[dag][route].Item1[maxOud]].MatrixID, OrdersDict[Dagen[dag][route + 1].Item1[0]].MatrixID].Rijtijd;
            
            Dagen[dag][route].Item1.AddRange(Dagen[dag][route + 1].Item1);
            Dagen[dag][route].Item2.SetValue(Dagen[dag][route].Item2.Value + Dagen[dag][route + 1].Item2.Value);
            Dagen[dag].RemoveAt(route + 1);

        }

        public double getRijTijd(int dag)
        {
            double res = Rijtijden[dag];
            res += 30 * Dagen[dag].Count;

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

    public struct Capaciteit
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
