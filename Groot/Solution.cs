using System;
using System.Collections.Generic;
using System.Linq;

using static Groot.Data;

namespace Groot
{
    public class Solution
    {
        public Truck Truck1, Truck2;
        public Dictionary<int, ValidArray> ValidCheck;

        public double Value { get { return Rijtijd + Strafpunten; } }

        public double Rijtijd, Strafpunten;

        public Solution()
        {
            Truck1 = new Truck();
            Truck2 = new Truck();
        }

        public Solution Copy()
        {
            Solution res = new Solution();
            res.Truck1 = Truck1.Copy();
            res.Truck2 = Truck2.Copy();
            res.ValidCheck = new Dictionary<int, ValidArray>();
            KeyValuePair<int, ValidArray>[] copy = ValidCheck.ToArray();
            foreach (KeyValuePair<int, ValidArray> kvp in copy)
                res.ValidCheck[kvp.Key] = kvp.Value.Copy();
            res.Rijtijd = Rijtijd;
            res.Strafpunten = Strafpunten;
            return res;
        }

        public Solution RandomMutation()
        {
            Solution res = Copy();

            int choice = RNG.Next(4);
            int truckChoice = RNG.Next(2);
            Truck truck = res[truckChoice];

            switch (choice)
            {
                case 0:
                    res.addRandomOrder(truck);
                    break;
                case 1:
                    res.removeRandomOrder(truck);
                    break;
                case 2:
                    res.swapRandomOrder(truck);
                    break;
                case 3:
                    res.addRandomDumpen(truck);
                    break;
                case 4:
                    res.changeRandomOrderDay(truck);
                    break;
                case 5:
                    res.changeRandomOrderTruck();
                    break;
            }
            return res;
        }

        void addRandomOrder(Truck truck)
        {
            int dag = RNG.Next(5);
            double oudRijtijd = truck.Rijtijden[dag];

            int orderId = Orders[RNG.Next(Orders.Length)].Key;

            int index = RNG.Next(truck.Dagen[dag].Count - 1);

            truck.AddOrder(orderId, index, dag);

            Rijtijd += truck.Rijtijden[dag] - oudRijtijd;
        }

        void removeRandomOrder(Truck truck)
        {
            int dag = RNG.Next(5);

            if (truck.Dagen[dag].Count < 3)
                return;

            double oudRijtijd = truck.Rijtijden[dag];

            int index = RNG.Next(truck.Dagen[dag].Count - 1);

            truck.RemoveOrder(index, dag);

            Rijtijd += truck.Rijtijden[dag] - oudRijtijd;
        }

        void swapRandomOrder(Truck truck)
        {
            int dag = RNG.Next(5);
            int plek1;
            int plek2;
            int plek1res;
            int plek2res;

            double oudRijtijd = truck.Rijtijden[dag];

            if (truck.Dagen[dag].Count < 4)
                return;

            plek1 = RNG.Next(0, truck.Dagen[dag].Count - 1);
            plek1res = truck.Dagen[dag][plek1];

            plek2 = RNG.Next(0, truck.Dagen[dag].Count - 1);
            plek2res = truck.Dagen[dag][plek2];

            if (plek1 == plek2)
                return;

            truck.RemoveOrder(plek1, dag);
            truck.AddOrder(plek2res, plek1, dag);

            truck.RemoveOrder(plek2, dag);
            truck.AddOrder(plek1res, plek2, dag);

            Rijtijd += truck.Rijtijden[dag] - oudRijtijd;
        }

        void addRandomDumpen(Truck truck)
        {
            int dag = RNG.Next(5);

            int index = RNG.Next(truck.Dagen[dag].Count - 1);

            double oudRijtijd = truck.Rijtijden[dag];

            truck.AddOrder(0, index, dag);

            Rijtijd += truck.Rijtijden[dag] - oudRijtijd;
        }

        void changeRandomOrderDay(Truck truck)
        {
            int plek1;
            int plek2;
            int plek1res;
            int plek2res;
            int count = 0;

            // Init random values
            int dag1 = RNG.Next(5);
            int dag2 = RNG.Next(5);

            if (dag1 == dag2)
                return;

            double oudRijtijd1 = truck.Rijtijden[dag1];
            double oudRijtijd2 = truck.Rijtijden[dag2];

            plek1 = RNG.Next(0, truck.Dagen[dag1].Count - 1);
            plek2 = RNG.Next(0, truck.Dagen[dag2].Count - 1);

            while ((truck.Dagen[dag1][plek1] == 0 || truck.Dagen[dag2][plek2] == 0) && count++ < 10)
            {
                plek1 = RNG.Next(0, truck.Dagen[dag1].Count - 1);
                plek2 = RNG.Next(0, truck.Dagen[dag2].Count - 1);
            }

            // Swap orders if not dump
            if (count < 10)
            {
                plek1res = truck.Dagen[dag1][plek1];
                plek2res = truck.Dagen[dag2][plek2];
                truck.RemoveOrder(plek1, dag1);
                truck.RemoveOrder(plek2, dag2);
                truck.AddOrder(plek2res, plek1, dag1);
                truck.AddOrder(plek1res, plek2, dag2);
                Rijtijd += truck.Rijtijden[dag1] - oudRijtijd1;
                Rijtijd += truck.Rijtijden[dag2] - oudRijtijd2;
            }
        }

        void changeRandomOrderTruck()
        {
            int dag1 = RNG.Next(5);
            int dag2 = RNG.Next(5);
            int swapTo = RNG.Next(2);
            int swapFrom = swapTo == 0 ? 1 : 0;

            Truck Truck1 = this[swapTo];
            Truck Truck2 = this[swapFrom];

            int plek1;
            int plek2;
            int plek1res;
            int counter = 0;

            double oudRijtijd1 = Truck1.Rijtijden[dag1];
            double oudRijtijd2 = Truck2.Rijtijden[dag2];

            plek1 = RNG.Next(0, Truck1.Dagen[dag1].Count - 1);
            plek2 = RNG.Next(0, Truck2.Dagen[dag2].Count - 1);

            while ((Truck1.Dagen[dag1][plek1] == 0 || Truck2.Dagen[dag2][plek2] == 0) && counter++ < 10)
            {
                plek1 = RNG.Next(0, Truck1.Dagen[dag1].Count - 1);
                plek2 = RNG.Next(0, Truck2.Dagen[dag2].Count - 1);
            }

            if (counter < 10)
            {
                plek1res = Truck1.Dagen[dag1][plek1];
                Truck1.RemoveOrder(plek1, dag1);
                Truck2.AddOrder(plek1res, plek2, dag2);
                Rijtijd += Truck1.Rijtijden[dag1] - oudRijtijd1;
                Rijtijd += Truck2.Rijtijden[dag2] - oudRijtijd2;
            }
        }

        public Truck this[int key]
        {
            get
            {
                switch(key)
                {
                    case 0:
                        return Truck1;
                    case 1:
                        return Truck2;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (key)
                {
                    case 0:
                        Truck1 = value;
                        break;
                    case 1:
                        Truck2 = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }
    }


}
