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
        public List<int>[] Dagen;
        public double[] Rijtijden;

        public Truck()
        {
            Dagen = new List<int>[5];
            Rijtijden = new double[5];
            for(int i = 0; i < 5; i++)
            {
                Dagen[i] = new List<int>();
            }
        }

        public Truck Copy()
        {
            Truck res = new Truck();
            for (int i = 0; i < 5; i++)
                for (int j = 0; j < Dagen[i].Count; j++)
                    res.Dagen[i].Add(Dagen[i][j]);
            Rijtijden.CopyTo(res.Rijtijden, 0);
            return res;
        }

        public void AddOrder(int orderId, int index, int dag)
        {
            int a = 0, b = 0, c = 0;

            if (index == 0 || Dagen[dag][index -1] == 0)
                a = 287;
            else
                a = OrdersDict[Dagen[dag][index - 1]].MatrixID;

            if (orderId == 0)
            {
                b = 287;
                Rijtijden[dag] += 30;
            }
            else
            {
                b = OrdersDict[orderId].MatrixID;
                Rijtijden[dag] += OrdersDict[orderId].LedigingDuurMinuten;
            }

            if (Dagen[dag][index] == 0)
                c = 287;
            else
                c = OrdersDict[Dagen[dag][index]].MatrixID;

            Rijtijden[dag] -= AfstandenMatrix[a, c].Rijtijd;
            Rijtijden[dag] += AfstandenMatrix[a, b].Rijtijd;
            Rijtijden[dag] += AfstandenMatrix[b, c].Rijtijd;

            Dagen[dag].Insert(index, orderId);
        }

        public void RemoveOrder(int index, int dag)
        {
            int a = 0, b = 0, c = 0;

            int orderId = Dagen[dag][index];

            if (index == 0 || Dagen[dag][index - 1] == 0)
                a = 287;
            else
                a = OrdersDict[Dagen[dag][index - 1]].MatrixID;

            if (Dagen[dag][index] == 0)
            {
                b = 287;
                Rijtijden[dag] -= 30;
            }
            else
            {
                b = OrdersDict[orderId].MatrixID;
                Rijtijden[dag] -= OrdersDict[orderId].LedigingDuurMinuten;
            }

            if (Dagen[dag][index + 1] == 0)
                c = 287;
            else
                c = OrdersDict[Dagen[dag][index + 1]].MatrixID;

            Rijtijden[dag] += AfstandenMatrix[a, c].Rijtijd;
            Rijtijden[dag] -= AfstandenMatrix[a, b].Rijtijd;
            Rijtijden[dag] -= AfstandenMatrix[b, c].Rijtijd;

            Dagen[dag].RemoveAt(index);
        }
    }
}
