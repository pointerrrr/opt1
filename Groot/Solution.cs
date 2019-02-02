using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using static Groot.Data;

namespace Groot
{
    public class OrderTruck
    {
        public int Truck, Dag;

        public OrderTruck(int truck, int dag)
        {
            Truck = truck; Dag = dag;
        }
    }

    public class Solution
    {
        public Truck Truck1, Truck2;
        public Dictionary<int, ValidArray> ValidCheck;
        public Dictionary<int, List<OrderTruck>> OrderTrucks;
        
        public double Value { get { return Rijtijd + Strafpunten + StrafIntern; } }

        public double Rijtijd, Strafpunten, StrafIntern;

        public Solution()
        {
            Truck1 = new Truck();
            Truck2 = new Truck();
            Rijtijd = 600;
        }

        public Solution Copy()
        {
            Solution res = new Solution();
            res.Truck1 = Truck1.Copy();
            res.Truck2 = Truck2.Copy();
            res.ValidCheck = new Dictionary<int, ValidArray>();
            KeyValuePair<int, ValidArray>[] validcopy = ValidCheck.ToArray();
            foreach (KeyValuePair<int, ValidArray> kvp in validcopy)
                res.ValidCheck[kvp.Key] = kvp.Value.Copy();
            KeyValuePair<int, List<OrderTruck>>[] ordertruckcopy = OrderTrucks.ToArray();
            foreach (KeyValuePair<int, List<OrderTruck>> kvp in ordertruckcopy)
            {
                res.OrderTrucks[kvp.Key] = new List<OrderTruck>();
                foreach (OrderTruck ot in kvp.Value)
                    res.OrderTrucks[kvp.Key].Add(new OrderTruck(ot.Truck, ot.Dag));
            }

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
                    ShiftRandomOrderBetweenRoutes(truck, data);
                    break;
                case 3:
                    ShiftRandomOrderWithinRoute(truck, data);
                    break;
                case 4:
                    ShiftRandomOrderBetweenDays(truck, data);
                    break;
                case 5:
                    ShiftRandomOrderBetweenTrucks(data);
                    break;
                case 6:
                    AddDumpen(truck, data);
                    break;
                case 7:
                    RemoveDumpen(truck, data);
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
            StrafIntern = data.SolutionStrafIntern;
            Rijtijd = data.SolutionRijtijd;
        }

        public void ShiftRandomOrderBetweenRoutes(Truck truck, SolutionData data)
        {
            truck.RemoveOrder(data.index1, data.dag1, data.route1, data.Capaciteit1.Value);
            truck.AddOrder(data.order1, data.index2, data.dag1, data.route2, data.Capaciteit2.Value);

            truck.Rijtijden = data.Truck1Rijtijden;
            StrafIntern = data.SolutionStrafIntern;
            Rijtijd = data.SolutionRijtijd;
        }

        public void AddDumpen(Truck truck, SolutionData data)
        {
            truck.AddDumpen(data.dag1, data.route1, data.index1, data.Capaciteit1.Value, data.Capaciteit2.Value);

            truck.Rijtijden = data.Truck1Rijtijden;
            StrafIntern = data.SolutionStrafIntern;
            Rijtijd = data.SolutionRijtijd;
        }

        public void RemoveDumpen(Truck truck, SolutionData data)
        {
            truck.RemoveDumpen(data.dag1, data.route1, data.Capaciteit1.Value);

            truck.Rijtijden = data.Truck1Rijtijden;
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

        void AddSpecificDumpen(int dumptruck, int dumpdag, int dumproute, int dumpindex)
        {
            Truck t = this[dumptruck];
            int dag1 = dumpdag;
            int route1 = dumproute;
            int index1 = dumpindex;
            double oldRijtijd = Rijtijd;
            double oudRijtijd = t.Rijtijden[dumpdag];

            int a = 287;

            if (dumpindex != 0)
                a = OrdersDict[t.Dagen[dumpdag][dumproute].Item1[dumpindex - 1]].MatrixID;

            t.Rijtijden[dumpdag] -= AfstandenMatrix[a, OrdersDict[t.Dagen[dumpdag][dumproute].Item1[dumpindex]].MatrixID].Rijtijd;
            t.Rijtijden[dumpdag] += AfstandenMatrix[287, OrdersDict[t.Dagen[dumpdag][dumproute].Item1[dumpindex]].MatrixID].Rijtijd;
            t.Rijtijden[dumpdag] += AfstandenMatrix[a, 287].Rijtijd;
            t.Rijtijden[dumpdag] += 30;
            UpdateRijtijdStraf(oudRijtijd, t.Rijtijden[dumpdag]);

            double c1Voor = t.Dagen[dumpdag][dumproute].Item2.Value;
            double c2Voor = 0;

            Capaciteit Capaciteit1 = CalculateCapaciteit(t.Dagen[dumpdag][dumproute].Item1, 0, dumpindex - 1);
            Capaciteit Capaciteit2 = CalculateCapaciteit(t.Dagen[dumpdag][dumproute].Item1, dumpindex, t.Dagen[dumpdag][dumproute].Item1.Count);
            UpdateCapaciteit(c1Voor, Capaciteit1.Value);
            UpdateCapaciteit(c2Voor, Capaciteit2.Value);

            Rijtijd += t.Rijtijden[dumpdag] - oudRijtijd;
        }

        Capaciteit CalculateCapaciteit(List<int> l, int begin, int eind)
        {
            double result = 0;
            int i = begin;
            while (i < eind)
            {
                result += OrdersDict[l[i]].AantContainers * OrdersDict[l[i]].VolumePerContainer;
                i++;
            }

            return new Capaciteit(result);
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

        public void startOplossingInladen(string path, Solution s)
        {
            string[] visits = File.ReadAllLines(path);
            Truck t1 = s[0];
            Truck t2 = s[1];

            List<int[]> orders = new List<int[]>();

            for (int i = 0; i < visits.Length; i++)
            {
                int[] order = new int[4];
                string[] visit = visits[i].Split(';');

                order[0] = int.Parse(visit[0]);
                order[1] = int.Parse(visit[1]);
                order[2] = int.Parse(visit[2]);
                order[3] = int.Parse(visit[3]);

                orders.Add(order);
            }

            Comparison<int[]> comp = (a, b) => { return compareOrder(a, b); };
            orders.Sort(comp);

            int route = 0;
            int offset = 0;
            for (int i = 0; i < orders.Count; i++)
            {
                
                if (orders[i][3] != 0)
                    switch (orders[i][0])
                    {
                        case 1:
                            {
                                AddSpecificOrder(s[0], orders[i][1] - 1, route, orders[i][2] - 1 - offset, orders[i][3]);
                            }
                            break;
                        case 2:
                            {
                                AddSpecificOrder(s[1], orders[i][1] - 1, route, orders[i][2] - 1 - offset, orders[i][3]);
                            }
                            break;
                    }
                else if ( i + 1 < orders.Count && orders[i+1][2] != 1 )
                {
                    route++;
                    offset = orders[i][2];
                }
                else if( i +1 < orders.Count && orders[i+1][2] == 1)
                {
                    route = 0;
                    offset = 0;                    
                }

            }
        }

        int compareOrder(int[] a, int[] b)
        {
            if (a[0] != b[0])
                return a[0] - b[0];
            else if (a[1] != b[1])
                return a[1] - b[1];
            else
                return a[2] - b[2];
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
