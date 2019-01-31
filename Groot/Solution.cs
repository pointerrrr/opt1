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
        private readonly double MaxRijtijdDag = 0;// 60d * 12d;
        private readonly double MinRijtijdDag = 60d * 10d;
        private readonly double RijtijdStraf = 0;//5000000d;
        private readonly double RijtijdStrafMinuut = 0;//100000d;
        private readonly double MaxCapaciteit = 0;//100000d;
        private readonly double CapaciteitStraf = 0;//10000d;
        private readonly double CapaciteitStrafLiter = 0;//10d;

        public double Value { get { return Rijtijd + Strafpunten + StrafIntern; } }

        public double Rijtijd, Strafpunten, StrafIntern;

        public Solution()
        {
            Truck1 = new Truck();
            Truck2 = new Truck();
            Rijtijd = 300;
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

        public Solution RandomMutation(int max = 7)
        {
            Solution res = Copy();

            int choice = RNG.Next(max);
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
                    res.AddDumpen(truck);                    
                    break;
                case 3:
                    res.RemoveDumpen(truck);
                    break;
                case 4:
                    res.ShiftRandomOrderBetweenDays(truck);
                    break;
                case 5:
                    res.ShiftRandomOrderBetweenTrucks();
                    break;
                case 6:
                    res.ShiftRandomOrderWithinRoute(truck);
                    break;
                case 7:
                    res.TwoAndAHalfOpt(truck);
                    break;
                case 8:
                    res.TwoOpt(truck);
                    break;

            }
            return res;
        }

        public void AddRandomOrder(Truck truck)
        {
            int dag = RNG.Next(5);
            
            int orderId = Orders[RNG.Next(Orders.Length)].Key;

            int route = RNG.Next(truck.Dagen[dag].Count);

            int index = RNG.Next(truck.Dagen[dag][route].Item1.Count);


            AddSpecificOrder(truck, dag, route, index, orderId);
        }

        public void AddSpecificOrder(Truck truck, int dag, int route, int index, int orderId)
        {
            double oudRijtijd = truck.Rijtijden[dag];

            double capaciteitVoor = truck.Dagen[dag][route].Item2.Value;

            truck.AddOrder(orderId, index, dag, route);

            double capaciteitNa = truck.Dagen[dag][route].Item2.Value;

            bool validVoor = ValidCheck[orderId].Valid;
            ValidCheck[orderId][dag]++;
            bool validNa = ValidCheck[orderId].Valid;
            UpdateValid(orderId, validVoor, validNa);
            
            UpdateRijtijdStraf(oudRijtijd, truck.Rijtijden[dag]);
            UpdateCapaciteit(capaciteitVoor, capaciteitNa);
            Rijtijd += truck.Rijtijden[dag] - oudRijtijd;
        }

        public void RemoveRandomOrder(Truck truck)
        {
            int dag = RNG.Next(5);

            if (truck.Dagen[dag].Count < 1)
                return;

            int route = RNG.Next(truck.Dagen[dag].Count);

            int index = RNG.Next(truck.Dagen[dag][route].Item1.Count);

            RemoveSpecificOrder(truck, dag, route, index);
        }

        public void RemoveSpecificOrder(Truck truck, int dag, int route, int index)
        {
            if (truck.Dagen[dag][route].Item1.Count < 1)
                return;

            double oudRijtijd = truck.Rijtijden[dag];

            int orderId = truck.Dagen[dag][route].Item1[index];

            bool validVoor = ValidCheck[orderId].Valid;
            ValidCheck[orderId][dag]--;
            bool validNa = ValidCheck[orderId].Valid;
            UpdateValid(orderId, validVoor, validNa);

            double capaciteitVoor = truck.Dagen[dag][route].Item2.Value;
            truck.RemoveOrder(index, dag, route);
            double capaciteitNa = truck.Dagen[dag][route].Item2.Value;

            UpdateRijtijdStraf(oudRijtijd, truck.Rijtijden[dag]);
            UpdateCapaciteit(capaciteitVoor, capaciteitNa);
            Rijtijd += truck.Rijtijden[dag] - oudRijtijd;
        }

        public void ShiftRandomOrderWithinRoute(Truck truck)
        {
            int dag = RNG.Next(5);
            int route = RNG.Next(truck.Dagen[dag].Count);

            int plek1;
            int plek2;
            int plek1res;

            if (truck.Dagen[dag][route].Item1.Count < 1)
                return;

            plek1 = RNG.Next(truck.Dagen[dag][route].Item1.Count);
            plek1res = truck.Dagen[dag][route].Item1[plek1];

            RemoveSpecificOrder(truck, dag, route, plek1);

            plek2 = RNG.Next(truck.Dagen[dag][route].Item1.Count);

            AddSpecificOrder(truck, dag, route, plek2, plek1res);
        }

        public void ShiftRandomOrderBetweenRoutes(Truck truck)
        {
            int dag = RNG.Next(5);
            int route1 = RNG.Next(truck.Dagen[dag].Count);
            int route2 = RNG.Next(truck.Dagen[dag].Count);

            int plek1;
            int plek2;
            int plek1res;

            if (truck.Dagen[dag][route1].Item1.Count < 1 || truck.Dagen[dag][route2].Item1.Count < 1)
                return;

            plek1 = RNG.Next(truck.Dagen[dag][route1].Item1.Count);
            plek1res = truck.Dagen[dag][route1].Item1[plek1];

            plek2 = RNG.Next(truck.Dagen[dag][route2].Item1.Count);

            RemoveSpecificOrder(truck, dag, route1, plek1);
            AddSpecificOrder(truck, dag, route2, plek2, plek1res);
        }

        public void AddDumpen(Truck truck)
        {
            int dag = RNG.Next(5);
            int route = RNG.Next(truck.Dagen[dag].Count);

            double oudRijtijd = truck.Rijtijden[dag];

            if (truck.Dagen[dag][route].Item1.Count < 3)
                return;

            int index = RNG.Next(truck.Dagen[dag][route].Item1.Count - 1);

            truck.AddDumpen(dag, route, index);
            Rijtijd += truck.Rijtijden[dag] - oudRijtijd;
        }

        public void RemoveDumpen(Truck truck)
        {
            int dag = RNG.Next(5);
            int route = RNG.Next(truck.Dagen[dag].Count);

            double oudRijtijd = truck.Rijtijden[dag];

            truck.RemoveDumpen(dag, route);
            Rijtijd += truck.Rijtijden[dag] - oudRijtijd;
        }

        public void ShiftRandomOrderBetweenDays(Truck truck)
        {
            int plek1;
            int plek2;
            int plek1res;

            // Init random values
            int dag1 = RNG.Next(5);
            int dag2 = RNG.Next(5);
            int route1 = RNG.Next(truck.Dagen[dag1].Count);
            int route2 = RNG.Next(truck.Dagen[dag2].Count);

            if (dag1 == dag2)
                return;

            if (truck.Dagen[dag1][route1].Item1.Count < 1 || truck.Dagen[dag2][route2].Item1.Count < 1)
                return;

            plek1 = RNG.Next(truck.Dagen[dag1][route1].Item1.Count);
            plek2 = RNG.Next(truck.Dagen[dag2][route2].Item1.Count);

            plek1res = truck.Dagen[dag1][route1].Item1[plek1];

            RemoveSpecificOrder(truck, dag1, route1, plek1);
            AddSpecificOrder(truck, dag2, route2, plek2, plek1res);
        }

        public void ShiftRandomOrderBetweenTrucks()
        {
            int dag1 = RNG.Next(5);
            int dag2 = RNG.Next(5);
            int swapTo = RNG.Next(2);
            int swapFrom = swapTo == 0 ? 1 : 0;

            Truck Truck1 = this[swapTo];
            Truck Truck2 = this[swapFrom];

            int route1 = RNG.Next(Truck1.Dagen[dag1].Count);
            int route2 = RNG.Next(Truck2.Dagen[dag2].Count);

            if (Truck1.Dagen[dag1][route1].Item1.Count < 1 || Truck2.Dagen[dag2][route2].Item1.Count < 1)
                return;

            int plek1;
            int plek2;
            int plek1res;
            
            plek1 = RNG.Next(Truck1.Dagen[dag1][route1].Item1.Count);
            plek2 = RNG.Next(Truck2.Dagen[dag2][route2].Item1.Count);

            plek1res = Truck1.Dagen[dag1][route1].Item1[plek1];

            RemoveSpecificOrder(Truck1, dag1, route1, plek1);
            AddSpecificOrder(Truck2, dag2, route2, plek2, plek1res);
        }

        public void TwoOpt(Truck truck)
        {
            // TODO
        }

        public void TwoAndAHalfOpt(Truck truck)
        {            
            // TODO
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
            /*if (oud >= MinRijtijdDag && nieuw < MinRijtijdDag)
                StrafIntern += (MinRijtijdDag - nieuw) * RijtijdStrafMinuut + RijtijdStraf;
            else if (oud < MinRijtijdDag && nieuw >= MinRijtijdDag)
                StrafIntern -= (MinRijtijdDag - oud) * RijtijdStrafMinuut + RijtijdStraf;
            else if (oud < MinRijtijdDag && nieuw < MinRijtijdDag)
                StrafIntern += (oud - nieuw) * RijtijdStrafMinuut;*/
        }

        public void UpdateValid(int orderId, bool validVoor, bool validNa)
        {
            if (validVoor)
            {
                Strafpunten += OrdersDict[orderId].Frequentie * OrdersDict[orderId].LedigingDuurMinuten * 3d;
                //StrafIntern += OrdersDict[orderId].Frequentie * OrdersDict[orderId].LedigingDuurMinuten * 1000d;
            }
            else if (validNa)
            {
                Strafpunten -= OrdersDict[orderId].Frequentie * OrdersDict[orderId].LedigingDuurMinuten * 3d;
                //StrafIntern -= OrdersDict[orderId].Frequentie * OrdersDict[orderId].LedigingDuurMinuten * 1000d;
            }
        }

        public void UpdateCapaciteit(double capaciteitVoor, double capaciteitNa)
        {
            if (capaciteitVoor <= MaxCapaciteit && capaciteitNa > MaxCapaciteit)
                StrafIntern += CapaciteitStrafLiter * (capaciteitNa - MaxCapaciteit) + CapaciteitStraf;
            else if (capaciteitVoor > MaxCapaciteit && capaciteitNa <= MaxCapaciteit)
                StrafIntern -= CapaciteitStrafLiter * (capaciteitVoor - MaxCapaciteit) + CapaciteitStraf;
            else if (capaciteitVoor > MaxCapaciteit && capaciteitNa > MaxCapaciteit)
                StrafIntern += CapaciteitStrafLiter * (capaciteitNa - capaciteitVoor);
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
