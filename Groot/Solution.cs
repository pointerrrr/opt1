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

        public void Mutation(SolutionData data)
        {
            int choice = data.Choice;
            Truck truck = this[0];
            if(choice != 5)
            {
                truck = this[data.truck];
            }
                

            switch (choice)
            {
                case 0:
                    AddRandomOrder(truck, data);
                    break;
                case 1:
                    RemoveRandomOrder(truck, data);
                    break;
                case 2:
                    AddDumpen(truck, data);                    
                    break;
                case 3:
                    RemoveDumpen(truck, data);
                    break;
                case 4:
                    ShiftRandomOrderBetweenDays(truck, data);
                    break;
                case 5:
                    ShiftRandomOrderBetweenTrucks(data);
                    break;
                case 6:
                    ShiftRandomOrderWithinRoute(truck, data);
                    break;
                case 7:
                    ShiftRandomOrderBetweenRoutes(truck, data);
                    break;
                case 8:
                    TwoAndAHalfOpt(truck, data);
                    break;
                case 9:
                    TwoOpt(truck, data);
                    break;

            }
        }

        public void AddRandomOrder(Truck truck, SolutionData data)
        {
            truck.AddOrder(data.order1, data.index1, data.dag1, data.route1, data.Capaciteit1.Value);

            truck.Rijtijden = data.Truck1Rijtijden;
            ValidCheck[data.order1][data.dag1]++;
            Strafpunten = data.SolutionStrafpunten;
            StrafIntern = data.SolutionStrafIntern;
            Rijtijd = data.SolutionRijtijd;
        }

        public void RemoveRandomOrder(Truck truck, SolutionData data)
        {
            truck.RemoveOrder(data.index1, data.dag1, data.route1, data.Capaciteit1.Value);

            truck.Rijtijden = data.Truck1Rijtijden;
            ValidCheck[data.order1][data.dag1]--;
            Strafpunten = data.SolutionStrafpunten;
            StrafIntern = data.SolutionStrafIntern;
            Rijtijd = data.SolutionRijtijd;
        }

        public void ShiftRandomOrderWithinRoute(Truck truck, SolutionData data)
        {
            truck.RemoveOrder(data.index1, data.dag1, data.route1, data.Capaciteit1.Value);
            if(data.index2 >= data.index1)
                truck.AddOrder(data.order1, data.index2 - 1, data.dag1, data.route1, data.Capaciteit1.Value);
            else
                truck.AddOrder(data.order1, data.index2, data.dag1, data.route1, data.Capaciteit1.Value);

            truck.Rijtijden = data.Truck1Rijtijden;
            Strafpunten = data.SolutionStrafpunten;
            StrafIntern = data.SolutionStrafIntern;
            Rijtijd = data.SolutionRijtijd;
        }

        public void ShiftRandomOrderBetweenRoutes(Truck truck, SolutionData data)
        {
            truck.RemoveOrder(data.index1, data.dag1, data.route1, data.Capaciteit1.Value);
            truck.AddOrder(data.order1, data.index2, data.dag1, data.route2, data.Capaciteit2.Value);

            truck.Rijtijden = data.Truck1Rijtijden;
            Strafpunten = data.SolutionStrafpunten;
            StrafIntern = data.SolutionStrafIntern;
            Rijtijd = data.SolutionRijtijd;
        }

        public void AddDumpen(Truck truck, SolutionData data)
        {
            truck.AddDumpen(data.dag1, data.route1, data.index1, data.Capaciteit1.Value, data.Capaciteit2.Value);

            truck.Rijtijden = data.Truck1Rijtijden;
            Strafpunten = data.SolutionStrafpunten;
            StrafIntern = data.SolutionStrafIntern;
            Rijtijd = data.SolutionRijtijd;
        }

        public void RemoveDumpen(Truck truck, SolutionData data)
        {
            truck.RemoveDumpen(data.dag1, data.route1, data.Capaciteit1.Value);

            truck.Rijtijden = data.Truck1Rijtijden;
            Strafpunten = data.SolutionStrafpunten;
            StrafIntern = data.SolutionStrafIntern;
            Rijtijd = data.SolutionRijtijd;
        }

        public void ShiftRandomOrderBetweenDays(Truck truck, SolutionData data)
        {
            truck.RemoveOrder(data.index1, data.dag1, data.route1, data.Capaciteit1.Value);
            truck.AddOrder(data.order1, data.index2, data.dag2, data.route2, data.Capaciteit2.Value);

            truck.Rijtijden = data.Truck1Rijtijden;
            Strafpunten = data.SolutionStrafpunten;
            StrafIntern = data.SolutionStrafIntern;
            Rijtijd = data.SolutionRijtijd;
            ValidCheck[data.order1][data.dag1]--;
            ValidCheck[data.order1][data.dag2]++;
        }

        public void ShiftRandomOrderBetweenTrucks(SolutionData data)
        {
            int swapTo = data.truck;
            int swapFrom = swapTo == 0 ? 1 : 0;
            Truck Truck1 = this[swapTo];
            Truck Truck2 = this[swapFrom];

            Truck1.RemoveOrder(data.index1, data.dag1, data.route1, data.Capaciteit1.Value);
            Truck2.AddOrder(data.order1, data.index2, data.dag2, data.route2, data.Capaciteit2.Value);

            Truck1.Rijtijden = data.Truck1Rijtijden;
            Truck2.Rijtijden = data.Truck2Rijtijden;
            Strafpunten = data.SolutionStrafpunten;
            StrafIntern = data.SolutionStrafIntern;
            Rijtijd = data.SolutionRijtijd;
            ValidCheck[data.order1][data.dag1]--;
            ValidCheck[data.order1][data.dag2]++;
        }

        public void TwoOpt(Truck truck, SolutionData data)
        {
            // TODO
        }

        public void TwoAndAHalfOpt(Truck truck, SolutionData data)
        {            
            // TODO
        }

        // Adds a specific order WITHOUT using precalculated SolutionData
        public void AddSpecificOrder(Truck truck, int dag, int route, int index, int order)
        {
            double capaciteitVoor = truck.Dagen[dag][route].Item2.Value;
            double newCap = capaciteitVoor + OrdersDict[order].VolumePerContainer * OrdersDict[order].AantContainers;
            UpdateCapaciteit(capaciteitVoor, newCap);

            bool validVoor = ValidCheck[order].Valid;
            ValidCheck[order][dag]++;
            bool validNa = ValidCheck[order].Valid;
            ValidCheck[order][dag]--;

            UpdateValid(order, validVoor, validNa);

            double oudRijtijd = truck.Rijtijden[dag];

            int a = 0, b = 0, c = 0;

            if (index == 0)
                a = 287;
            else
                a = OrdersDict[truck.Dagen[dag][route].Item1[index - 1]].MatrixID;

            b = OrdersDict[order].MatrixID;
            truck.Rijtijden[dag] += OrdersDict[order].LedigingDuurMinuten;

            if (index >= truck.Dagen[dag][route].Item1.Count)
                c = 287;
            else
                c = OrdersDict[truck.Dagen[dag][route].Item1[index]].MatrixID;

            truck.Rijtijden[dag] -= AfstandenMatrix[a, c].Rijtijd;
            truck.Rijtijden[dag] += AfstandenMatrix[a, b].Rijtijd;
            truck.Rijtijden[dag] += AfstandenMatrix[b, c].Rijtijd;

            UpdateRijtijdStraf(oudRijtijd, truck.Rijtijden[dag]);
            Rijtijd += truck.Rijtijden[dag] - oudRijtijd;

            truck.AddOrder(order, index, dag, route, newCap);
        }

        void UpdateValid(int orderId, bool validVoor, bool validNa)
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

        void UpdateCapaciteit(double capaciteitVoor, double capaciteitNa)
        {
            if (capaciteitVoor <= MaxCapaciteit && capaciteitNa > MaxCapaciteit)
                StrafIntern += CapaciteitStrafLiter * (capaciteitNa - MaxCapaciteit) + CapaciteitStraf;
            else if (capaciteitVoor > MaxCapaciteit && capaciteitNa <= MaxCapaciteit)
                StrafIntern -= CapaciteitStrafLiter * (capaciteitVoor - MaxCapaciteit) + CapaciteitStraf;
            else if (capaciteitVoor > MaxCapaciteit && capaciteitNa > MaxCapaciteit)
                StrafIntern += CapaciteitStrafLiter * (capaciteitNa - capaciteitVoor);
        }
        void UpdateRijtijdStraf(double oud, double nieuw)
        {
            if (oud <= MaxRijtijdDag && nieuw > MaxRijtijdDag)
                StrafIntern += (nieuw - MaxRijtijdDag) * RijtijdStrafMinuut + RijtijdStraf;
            else if (oud > MaxRijtijdDag && nieuw <= MaxRijtijdDag)
                StrafIntern -= (oud - MaxRijtijdDag) * RijtijdStrafMinuut + RijtijdStraf;
            else if (oud > MaxRijtijdDag && nieuw > MaxRijtijdDag)
                StrafIntern += (nieuw - oud) * RijtijdStrafMinuut;
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
