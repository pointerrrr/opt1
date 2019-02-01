using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Groot
{
    public static class Data
    {
        public readonly static Dictionary<int, OrderDescription> OrdersDict = new Dictionary<int, OrderDescription>();
        public readonly static KeyValuePair<int, OrderDescription>[] Orders;
        public readonly static AfstandRijtijd[,] AfstandenMatrix;

        public static Random RNG = new Random(1337);

        public readonly static double MaxRijtijdDag = 0;// 60d * 12d;
        public readonly static double MinRijtijdDag = 60d * 10d;
        public readonly static double RijtijdStraf = 0;//5000000d;
        public readonly static double RijtijdStrafMinuut = 0;//100000d;
        public readonly static double MaxCapaciteit = 0;//100000d;
        public readonly static double CapaciteitStraf = 0;//10000d;
        public readonly static double CapaciteitStrafLiter = 0;//10d;

        static Data()
        {
            AfstandenMatrix = afstandRijTijdInladen();
            OrdersDict = ordersInladen();
            Orders = OrdersDict.ToArray();
        }

        static AfstandRijtijd[,] afstandRijTijdInladen()
        {
            AfstandRijtijd[,] res;
            string[] afstanden = File.ReadAllLines(@"..\..\afstanden.txt");

            int maxLenght = int.Parse(afstanden[afstanden.Length - 1].Split(';')[0]);

            res = new AfstandRijtijd[maxLenght + 1, maxLenght + 1];

            for (int i = 1; i < afstanden.Length; i++)
            {
                string rij = afstanden[i];
                string[] info = rij.Split(';');
                int id1 = int.Parse(info[0]);
                int id2 = int.Parse(info[1]);
                double afstand = double.Parse(info[2]);
                double rijtijd = double.Parse(info[3]);
                res[id1, id2] = new AfstandRijtijd(id1, id2, afstand, rijtijd / 60);
            }
            return res;
        }

        static Dictionary<int, OrderDescription> ordersInladen()
        {
            Dictionary<int, OrderDescription> res = new Dictionary<int, OrderDescription>();

            string[] orderbestand = File.ReadAllLines(@"..\..\orderbestand.txt");

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
                res[a] = new OrderDescription(a, b, c, d, e, f, g, h, ib);
            }
            return res;
        }
    }
}
