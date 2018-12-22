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
        private readonly double MaxRijtijdDag = 60d * 12d;
        private readonly double MinRijtijdDag = 60d * 10d;
        private readonly double RijtijdStraf = 500000d;
        private readonly double RijtijdStrafMinuut = 1000d;
        private readonly double MaxCapaciteit = 100000d;
        private readonly double CapaciteitStraf = 10000d;
        private readonly double CapaciteitStrafLiter = 10d;

        public double Value { get { return Rijtijd + Strafpunten + StrafIntern; } }

        public double Rijtijd, Strafpunten, StrafIntern;

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
            res.StrafIntern = StrafIntern;
            return res;
        }

        public Solution RandomMutation()
        {
            Solution res = Copy();

            int choice = RNG.Next(7);
            int truckChoice = RNG.Next(2);
            Truck truck = res[truckChoice];

            switch (choice)
            {
                case 0:
                    res.AddRandomOrder(truck);
                    break;
                case 1:
                    res.RemoveRandomOrder(truck);
                    break;
                case 2:
                    res.SwapRandomOrder(truck);
                    break;
                case 3:
                    res.AddRandomDumpen(truck);
                    break;
                case 4:
                    res.ChangeRandomOrderDay(truck);
                    break;
                case 5:
                    res.ChangeRandomOrderTruck();
                    break;
                case 6:
                    res.TwoAndAHalfOpt(truck);
                    break;

            }
            return res;
        }

        public void AddRandomOrder(Truck truck)
        {
            int dag = RNG.Next(5);
            
            int orderId = Orders[RNG.Next(Orders.Length)].Key;

            int index = RNG.Next(truck.Dagen[dag].Count - 1);

            AddSpecificOrder(truck, dag, index, orderId);
        }

        public void AddSpecificOrder(Truck truck, int dag, int index, int orderId)
        {
            double oudRijtijd = truck.Rijtijden[dag];

            int[] capaciteitVoor = CheckCapacity(truck, dag, index);

            truck.AddOrder(orderId, index, dag);

            int[] capaciteitNa = CheckCapacity(truck, dag, index);

            if (orderId != 0)
            {
                bool validVoor = ValidCheck[orderId].Valid;
                ValidCheck[orderId][dag]++;
                bool validNa = ValidCheck[orderId].Valid;

                if (validVoor)
                {
                    Strafpunten += OrdersDict[orderId].Frequentie * OrdersDict[orderId].LedigingDuurMinuten * 3d;
                    //StrafIntern += OrdersDict[orderId].Frequentie * OrdersDict[orderId].LedigingDuurMinuten * 100d;
                }
                else if (validNa)
                {
                    Strafpunten -= OrdersDict[orderId].Frequentie * OrdersDict[orderId].LedigingDuurMinuten * 3d;
                    //StrafIntern -= OrdersDict[orderId].Frequentie * OrdersDict[orderId].LedigingDuurMinuten * 100d;
                }
            }
            UpdateRijtijdStraf(oudRijtijd, truck.Rijtijden[dag]);
            ChangeCapaciteit(capaciteitVoor, capaciteitNa);
            Rijtijd += truck.Rijtijden[dag] - oudRijtijd;
        }

        public void RemoveRandomOrder(Truck truck)
        {
            int dag = RNG.Next(5);

            if (truck.Dagen[dag].Count < 3)
                return;

            int index = RNG.Next(truck.Dagen[dag].Count - 1);

            RemoveSpecificOrder(truck, dag, index);
        }

        public void RemoveSpecificOrder(Truck truck, int dag, int index)
        {
            double oudRijtijd = truck.Rijtijden[dag];

            int orderId = truck.Dagen[dag][index];
            if (orderId != 0)
            {
                bool validVoor = ValidCheck[orderId].Valid;
                ValidCheck[orderId][dag]--;
                bool validNa = ValidCheck[orderId].Valid;

                if (validVoor)
                {
                    Strafpunten += OrdersDict[orderId].Frequentie * OrdersDict[orderId].LedigingDuurMinuten * 3d;
                    //StrafIntern += OrdersDict[orderId].Frequentie * OrdersDict[orderId].LedigingDuurMinuten * 100d;
                }
                else if (validNa)
                {
                    Strafpunten -= OrdersDict[orderId].Frequentie * OrdersDict[orderId].LedigingDuurMinuten * 3d;
                    //StrafIntern -= OrdersDict[orderId].Frequentie * OrdersDict[orderId].LedigingDuurMinuten * 100d;
                }
            }

            int[] capaciteitVoor = CheckCapacity(truck, dag, index);
            truck.RemoveOrder(index, dag);
            int[] capaciteitNa = CheckCapacity(truck, dag, index);
            UpdateRijtijdStraf(oudRijtijd, truck.Rijtijden[dag]);
            ChangeCapaciteit(capaciteitVoor, capaciteitNa);
            Rijtijd += truck.Rijtijden[dag] - oudRijtijd;
        }

        public void SwapRandomOrder(Truck truck)
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

            RemoveSpecificOrder(truck, dag, plek1);
            AddSpecificOrder(truck, dag, plek1, plek2res);

            RemoveSpecificOrder(truck, dag, plek2);
            AddSpecificOrder(truck, dag, plek2, plek1res);
        }

        public void AddRandomDumpen(Truck truck)
        {
            int dag = RNG.Next(5);

            int index = RNG.Next(truck.Dagen[dag].Count - 1);

            double oudRijtijd = truck.Rijtijden[dag];

            AddSpecificOrder(truck, dag, index, 0);
        }

        public void ChangeRandomOrderDay(Truck truck)
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

                RemoveSpecificOrder(truck, dag1, plek1);
                RemoveSpecificOrder(truck, dag2, plek2);
                AddSpecificOrder(truck, dag1, plek1, plek2res);
                AddSpecificOrder(truck, dag2, plek2, plek1res);
            }
        }

        public void ChangeRandomOrderTruck()
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

                RemoveSpecificOrder(Truck1, dag1, plek1);
                AddSpecificOrder(Truck2, dag2, plek2, plek1res);
            }
        }

        public void TwoAndAHalfOpt(Truck truck)
        {
            
            int dag = RNG.Next(5);

            if (truck.Dagen[dag].Count < 4)
                return;

            int plek1 = RNG.Next(0, truck.Dagen[dag].Count - 1);
            
            int plek1res = truck.Dagen[dag][plek1];

            RemoveSpecificOrder(truck, dag, plek1);

            int plek2 = RNG.Next(0, truck.Dagen[dag].Count - 1);

            AddSpecificOrder(truck, dag, plek2, plek1res);
        }

        public void UpdateRijtijdStraf(double oud, double nieuw)
        {
            UpdateRijtijdStrafMax(oud, nieuw);
            UpdateRijtijdStrafMin(oud, nieuw);

            
        }

        public void UpdateRijtijdStrafMax(double oud, double nieuw)
        {
            if (oud <= MaxRijtijdDag && nieuw > MaxRijtijdDag)
                StrafIntern += (nieuw - MaxRijtijdDag) * RijtijdStrafMinuut + RijtijdStraf;
            else if (oud > MaxRijtijdDag && nieuw <= MaxRijtijdDag)
                StrafIntern -= (oud - MaxRijtijdDag) * RijtijdStrafMinuut + RijtijdStraf;
            else if (oud > MaxRijtijdDag && nieuw > MaxRijtijdDag)
                StrafIntern += (nieuw - oud) * RijtijdStrafMinuut;
        }

        public void UpdateRijtijdStrafMin(double oud, double nieuw)
        {
            if (oud >= MinRijtijdDag && nieuw < MinRijtijdDag)
                StrafIntern += (MinRijtijdDag - nieuw) * RijtijdStrafMinuut + RijtijdStraf;
            else if (oud < MinRijtijdDag && nieuw >= MinRijtijdDag)
                StrafIntern -= (MinRijtijdDag - oud) * RijtijdStrafMinuut + RijtijdStraf;
            else if (oud < MinRijtijdDag && nieuw < MinRijtijdDag)
                StrafIntern += (oud - nieuw) * RijtijdStrafMinuut;
        }

        public void ChangeCapaciteit(int[] voorList, int[] naList)
        {
            if (voorList.Length == 1 && naList.Length == 1)
            {
                int voor = voorList[0];
                int na = naList[0];
                double max = MaxCapaciteit;

                if (voor <= max && na > max)
                    StrafIntern += CapaciteitStrafLiter * (na - max) + CapaciteitStraf;
                else if (voor > max && na <= max)
                    StrafIntern -= CapaciteitStrafLiter * (voor - max) + CapaciteitStraf;
                else if (voor > max && na > max)
                    StrafIntern += CapaciteitStrafLiter * (na - voor);
            }
            if (voorList.Length > 1 && naList.Length == 1)
                ;
            if (voorList.Length == 1 && naList.Length > 1)
                ;
            if (voorList.Length > 1 && naList.Length > 1)
                ;
        }

        public int[] CheckCapacity(Truck truck ,int dag, int index)
        {
            int result = 0;
            int i = index;
            int j = index + 1;

            if (truck.Dagen[dag][index] != 0)
            {

                while (i >= 0 && truck.Dagen[dag][i] != 0)
                {
                    result += OrdersDict[truck.Dagen[dag][i]].AantContainers * OrdersDict[truck.Dagen[dag][i]].VolumePerContainer;
                    i--;
                }
                while (j < truck.Dagen[dag].Count && truck.Dagen[dag][j] != 0)
                {
                    result += OrdersDict[truck.Dagen[dag][j]].AantContainers * OrdersDict[truck.Dagen[dag][j]].VolumePerContainer;
                    j++;
                }
                return new int[] { result};
            }
            else
            {
                int result1 = 0, result2 = 0;
                i = index -1;
                while (i >= 0 && truck.Dagen[dag][i] != 0)
                {
                    result1 += OrdersDict[truck.Dagen[dag][i]].AantContainers * OrdersDict[truck.Dagen[dag][i]].VolumePerContainer;
                    i--;
                }
                while (j < truck.Dagen[dag].Count && truck.Dagen[dag][j] != 0)
                {
                    result2 += OrdersDict[truck.Dagen[dag][j]].AantContainers * OrdersDict[truck.Dagen[dag][j]].VolumePerContainer;
                    j++;
                }
                return new int[] { result1, result2 };
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
