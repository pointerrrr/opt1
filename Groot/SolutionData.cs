﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Groot.Data;

namespace Groot
{
    public class SolutionData
    {
        Solution oldSolution;
        public double[] Truck1Rijtijden = new double[5], Truck2Rijtijden = new double[5];
        public Capaciteit Capaciteit1, Capaciteit2;
        public Dictionary<int, ValidArray> SolValidCheck;
        public double SolutionRijtijd, SolutionStrafpunten, SolutionStrafIntern;
        public int Choice, truck, dag1, dag2, dag3, dag4, route1, route2, route3, route4, index1, index2, index3, index4, order1, order2, frequentie, dagselectie;
        public bool accepted = true, allow = true;

        public double Value
        {
            get { return SolutionRijtijd + SolutionStrafpunten + SolutionStrafIntern; }
        }
        public SolutionData(Solution sol, int choice, bool mutate = true)
        {
            oldSolution = sol;
            Choice = choice;
            if (mutate)
                Calculate(Choice);
        }

        void Calculate(int choice)
        {
            switch (choice)
            {
                case 0:
                    AddRandomOrder();
                    break;
                case 1:
                    RemoveRandomOrder();
                    break;
                case 2:
                    ShiftRandomOrderBetweenRoutes();
                    break;
                case 3:
                    ShiftRandomOrderWithinRoute();
                    break;
                case 4:
                    ShiftRandomOrderBetweenDays();
                    break;
                case 5:
                    ShiftRandomOrderBetweenTrucks();
                    break;
                case 6:
                    AddRandomDumpen();
                    break;
                case 7:
                    RemoveDumpen();
                    break;
                case 8:
                    TwoAndAHalfOpt();
                    break;
                case 9:
                    TwoOpt();
                    break;

            }

            for (int j = 0; j < 5; j++)
            {
                if (Truck1Rijtijden[j] > MaxRijtijdDag || Truck2Rijtijden[j] > MaxRijtijdDag)
                {
                    allow = false;
                }
            }
            if ((Capaciteit1 != null && Capaciteit1.Value > MaxCapaciteit) || ( Capaciteit2 != null && Capaciteit2.Value > MaxCapaciteit))
                allow = false;


        }

        void AddSpecificOrder(Truck t, int dag, int orderId, int route, int index, Capaciteit cap, double[] rijtijden, int a = -1, int c = -1)
        {
            double capaciteitVoor = cap.Value;
            cap.SetValue(capaciteitVoor + OrdersDict[orderId].VolumePerContainer * OrdersDict[orderId].AantContainers);
            UpdateCapaciteit(capaciteitVoor, cap.Value);

            bool validVoor = SolValidCheck[orderId].Valid;
            SolValidCheck[orderId][dag]++;
            bool validNa = SolValidCheck[orderId].Valid;
            SolValidCheck[orderId][dag]--;

            UpdateValid(orderId, validVoor, validNa);

            double oudRijtijd = rijtijden[dag];

            int b = 0;

            if(a == -1)
                if (index == 0)
                    a = 287;
                else
                    a = OrdersDict[t.Dagen[dag][route].Item1[index - 1]].MatrixID;

            b = OrdersDict[orderId].MatrixID;
            rijtijden[dag] += OrdersDict[orderId].LedigingDuurMinuten;

            if(c == -1)
                if (index >= t.Dagen[dag][route].Item1.Count)
                    c = 287;
                else
                    c = OrdersDict[t.Dagen[dag][route].Item1[index]].MatrixID;

            rijtijden[dag] -= AfstandenMatrix[a, c].Rijtijd;
            rijtijden[dag] += AfstandenMatrix[a, b].Rijtijd;
            rijtijden[dag] += AfstandenMatrix[b, c].Rijtijd;

            UpdateRijtijdStraf(oudRijtijd, rijtijden[dag]);
            SolutionRijtijd += rijtijden[dag] - oudRijtijd;
        }

        void RemoveSpecificOrder(Truck t, int dag, int route, int index, Capaciteit cap, double[] rijtijden)
        {
            if (t.Dagen[dag][route].Item1.Count < 1)
            {
                accepted = false;
                return;
            }

            int orderId = t.Dagen[dag][route].Item1[index];

            bool validVoor = SolValidCheck[orderId].Valid;
            SolValidCheck[orderId][dag]--;
            bool validNa = SolValidCheck[orderId].Valid;
            SolValidCheck[orderId][dag]++;
            UpdateValid(orderId, validVoor, validNa);

            double capaciteitVoor = t.Dagen[dag][route].Item2.Value;
            cap.SetValue(capaciteitVoor - OrdersDict[orderId].AantContainers * OrdersDict[orderId].VolumePerContainer);
            UpdateCapaciteit(capaciteitVoor, cap.Value);

            double oudRijtijd = rijtijden[dag];

            int a = 0, b = 0, c = 0;

            if (index == 0)
                a = 287;
            else
                a = OrdersDict[t.Dagen[dag][route].Item1[index - 1]].MatrixID;

            b = OrdersDict[orderId].MatrixID;
            rijtijden[dag] -= OrdersDict[orderId].LedigingDuurMinuten;

            if (index + 1 >= t.Dagen[dag][route].Item1.Count)
                c = 287;
            else
                c = OrdersDict[t.Dagen[dag][route].Item1[index + 1]].MatrixID;

            rijtijden[dag] += AfstandenMatrix[a, c].Rijtijd;
            rijtijden[dag] -= AfstandenMatrix[a, b].Rijtijd;
            rijtijden[dag] -= AfstandenMatrix[b, c].Rijtijd;
            UpdateRijtijdStraf(oudRijtijd, rijtijden[dag]);
            SolutionRijtijd += rijtijden[dag] - oudRijtijd;
        }

        void AddRandomOrder()
        {
            Truck t;
            truck = RNG.Next(2);
            if (truck == 0)
                t = oldSolution.Truck1;
            else
                t = oldSolution.Truck2;

            SolutionStrafpunten = oldSolution.Strafpunten;
            SolutionStrafIntern = oldSolution.StrafIntern;
            SolutionRijtijd = oldSolution.Rijtijd;
            SolValidCheck = oldSolution.ValidCheck;
            t.Rijtijden.CopyTo(Truck1Rijtijden, 0);

            dag1 = RNG.Next(5);

            int orderKey = RNG.Next(Orders.Length);

            order1 = Orders[orderKey].Key;

            frequentie = Orders[orderKey].Value.Frequentie;

            switch (frequentie)
            {
                case 1:
                    route1 = RNG.Next(t.Dagen[dag1].Count);

                    index1 = RNG.Next(t.Dagen[dag1][route1].Item1.Count);

                    Capaciteit1 = new Capaciteit(t.Dagen[dag1][route1].Item2.Value);
                    AddSpecificOrder(t, dag1, order1, route1, index1, Capaciteit1, Truck1Rijtijden);
                    break;
                case 2:
                    dagselectie = RNG.Next(2);
                    break;
                case 3:
                    dag1 = 0; dag2 = 2; dag3 = 4;
                    break;
                case 4:
                    dagselectie = RNG.Next(5);
                    if(dagselectie != 4)
                    {

                    }
                    break;
            }

            
        }

        void RemoveRandomOrder()
        {
            Truck t;
            truck = RNG.Next(2);
            if (truck == 0)
                t = oldSolution.Truck1;
            else
                t = oldSolution.Truck2;

            SolutionStrafpunten = oldSolution.Strafpunten;
            SolutionStrafIntern = oldSolution.StrafIntern;
            SolutionRijtijd = oldSolution.Rijtijd;
            SolValidCheck = oldSolution.ValidCheck;
            t.Rijtijden.CopyTo(Truck1Rijtijden, 0);

            dag1 = RNG.Next(5);

            if (t.Dagen[dag1].Count < 1)
            {
                accepted = false;
                return;
            }

            route1 = RNG.Next(t.Dagen[dag1].Count);

            if(t.Dagen[dag1][route1].Item1.Count < 1)
            {
                accepted = false;
                return;
            }

            index1 = RNG.Next(t.Dagen[dag1][route1].Item1.Count);
            order1 = t.Dagen[dag1][route1].Item1[index1];

            Capaciteit1 = new Capaciteit(t.Dagen[dag1][route1].Item2.Value);
            RemoveSpecificOrder(t, dag1, route1, index1, Capaciteit1, Truck1Rijtijden);
        }


        public void AddSpecificDumpen(int dumptruck, int dumpdag, int dumproute, int dumpindex)
        {
            truck = dumptruck;
            Truck t = oldSolution[dumptruck];
            dag1 = dumpdag;
            route1 = dumproute;
            index1 = dumpindex;
            SolutionRijtijd = oldSolution.Rijtijd;
            t.Rijtijden.CopyTo(Truck1Rijtijden, 0);
            double oudRijtijd = Truck1Rijtijden[dumpdag];

            int a = 287;

            if (dumpindex != 0)
                a = OrdersDict[t.Dagen[dumpdag][dumproute].Item1[dumpindex - 1]].MatrixID;

            Truck1Rijtijden[dumpdag] -= AfstandenMatrix[a, OrdersDict[t.Dagen[dumpdag][dumproute].Item1[dumpindex]].MatrixID].Rijtijd;
            Truck1Rijtijden[dumpdag] += AfstandenMatrix[287, OrdersDict[t.Dagen[dumpdag][dumproute].Item1[dumpindex]].MatrixID].Rijtijd;
            Truck1Rijtijden[dumpdag] += AfstandenMatrix[a, 287].Rijtijd;
            Truck1Rijtijden[dumpdag] += 30;
            UpdateRijtijdStraf(oudRijtijd, Truck1Rijtijden[dumpdag]);

            double c1Voor = t.Dagen[dumpdag][dumproute].Item2.Value;
            double c2Voor = 0;

            Capaciteit1 = CalculateCapaciteit(t.Dagen[dumpdag][dumproute].Item1, 0, dumpindex - 1);
            Capaciteit2 = CalculateCapaciteit(t.Dagen[dumpdag][dumproute].Item1, dumpindex, t.Dagen[dumpdag][dumproute].Item1.Count);
            UpdateCapaciteit(c1Voor, Capaciteit1.Value);
            UpdateCapaciteit(c2Voor, Capaciteit2.Value);

            SolutionRijtijd += Truck1Rijtijden[dumpdag] - oudRijtijd;
        }

        void AddRandomDumpen()
        {
            Truck t;
            truck = RNG.Next(2);
            t = oldSolution[truck];

            dag1 = RNG.Next(5);
            route1 = RNG.Next(t.Dagen[dag1].Count);

            if (t.Dagen[dag1][route1].Item1.Count < 3)
            {
                accepted = false;
                return;
            }

            index1 = RNG.Next(t.Dagen[dag1][route1].Item1.Count - 1);

            AddSpecificDumpen(truck, dag1, route1, index1);
        }

        void RemoveDumpen()
        {
            Truck t;
            truck = RNG.Next(2);
            if (truck == 0)
                t = oldSolution.Truck1;
            else
                t = oldSolution.Truck2;

            dag1 = RNG.Next(5);
            if(t.Dagen[dag1].Count > 1)
                route1 = RNG.Next(t.Dagen[dag1].Count - 1);
            else
            {
                accepted = false;
                return;
            }

            SolutionRijtijd = oldSolution.Rijtijd;
            t.Rijtijden.CopyTo(Truck1Rijtijden, 0);

            double oudRijtijd = Truck1Rijtijden[dag1];

            if (t.Dagen[dag1].Count <= 1)
            {
                accepted = false;
                return;
            }
            if (t.Dagen[dag1][route1].Item1.Count == 0)
            {
                Truck1Rijtijden[dag1] -= 30;
            }
            else
            {
                if (route1 >= t.Dagen[dag1].Count - 1 || t.Dagen[dag1][route1 + 1].Item1.Count == 0)
                {
                    accepted = false;
                    return;
                }

                int maxOud = t.Dagen[dag1][route1].Item1.Count - 1;

                Truck1Rijtijden[dag1] -= AfstandenMatrix[287, OrdersDict[t.Dagen[dag1][route1 + 1].Item1[0]].MatrixID].Rijtijd;
                Truck1Rijtijden[dag1] -= AfstandenMatrix[OrdersDict[t.Dagen[dag1][route1].Item1[maxOud]].MatrixID, 287].Rijtijd;
                Truck1Rijtijden[dag1] += AfstandenMatrix[OrdersDict[t.Dagen[dag1][route1].Item1[maxOud]].MatrixID, OrdersDict[t.Dagen[dag1][route1 + 1].Item1[0]].MatrixID].Rijtijd;
                Truck1Rijtijden[dag1] -= 30;
            }
            UpdateRijtijdStraf(oudRijtijd, Truck1Rijtijden[dag1]);

            Capaciteit1 = new Capaciteit(t.Dagen[dag1][route1].Item2.Value + t.Dagen[dag1][route1 + 1].Item2.Value);
            UpdateCapaciteit(t.Dagen[dag1][route1].Item2.Value, Capaciteit1.Value);
            UpdateCapaciteit(t.Dagen[dag1][route1 + 1].Item2.Value, 0);

            SolutionRijtijd += Truck1Rijtijden[dag1] - oudRijtijd;
        }

        void ShiftRandomOrderBetweenDays()
        {
            Truck t;
            truck = RNG.Next(2);
            if (truck == 0)
                t = oldSolution.Truck1;
            else
                t = oldSolution.Truck2;

            SolutionStrafpunten = oldSolution.Strafpunten;
            SolutionStrafIntern = oldSolution.StrafIntern;
            SolutionRijtijd = oldSolution.Rijtijd;
            SolValidCheck = oldSolution.ValidCheck;
            t.Rijtijden.CopyTo(Truck1Rijtijden, 0);

            // Init random values
            dag1 = RNG.Next(5);
            dag2 = RNG.Next(5);
            route1 = RNG.Next(t.Dagen[dag1].Count);
            route2 = RNG.Next(t.Dagen[dag2].Count);

            if (dag1 == dag2)
            {
                accepted = false;
                return;
            }

            if (t.Dagen[dag1][route1].Item1.Count < 1 || t.Dagen[dag2][route2].Item1.Count < 1)
            {
                accepted = false;
                return;
            }

            index1 = RNG.Next(t.Dagen[dag1][route1].Item1.Count);
            index2 = RNG.Next(t.Dagen[dag2][route2].Item1.Count);

            order1 = t.Dagen[dag1][route1].Item1[index1];

            Capaciteit1 = new Capaciteit(t.Dagen[dag1][route1].Item2.Value);
            Capaciteit2 = new Capaciteit(t.Dagen[dag2][route2].Item2.Value);
            RemoveSpecificOrder(t, dag1, route1, index1, Capaciteit1, Truck1Rijtijden);
            SolValidCheck[order1][dag1]--;
            AddSpecificOrder(t, dag2, order1, route2, index2, Capaciteit2, Truck1Rijtijden);
            SolValidCheck[order1][dag1]++;
        }

        void ShiftRandomOrderBetweenTrucks()
        {
            dag1 = RNG.Next(5);
            dag2 = RNG.Next(5);
            truck = RNG.Next(2);
            int swapFrom = truck == 0 ? 1 : 0;

            Truck Truck1 = oldSolution[truck];
            Truck Truck2 = oldSolution[swapFrom];

            SolutionStrafpunten = oldSolution.Strafpunten;
            SolutionStrafIntern = oldSolution.StrafIntern;
            SolutionRijtijd = oldSolution.Rijtijd;
            SolValidCheck = oldSolution.ValidCheck;
            Truck1.Rijtijden.CopyTo(Truck1Rijtijden, 0);
            Truck2.Rijtijden.CopyTo(Truck2Rijtijden, 0);

            route1 = RNG.Next(Truck1.Dagen[dag1].Count);
            route2 = RNG.Next(Truck2.Dagen[dag2].Count);

            if (Truck1.Dagen[dag1][route1].Item1.Count < 1 || Truck2.Dagen[dag2][route2].Item1.Count < 1)
            {
                accepted = false;
                return;
            }

            index1 = RNG.Next(Truck1.Dagen[dag1][route1].Item1.Count);
            index2 = RNG.Next(Truck2.Dagen[dag2][route2].Item1.Count);

            order1 = Truck1.Dagen[dag1][route1].Item1[index1];

            Capaciteit1 = new Capaciteit(Truck1.Dagen[dag1][route1].Item2.Value);
            Capaciteit2 = new Capaciteit(Truck2.Dagen[dag2][route2].Item2.Value);
            RemoveSpecificOrder(Truck1, dag1, route1, index1, Capaciteit1, Truck1Rijtijden);
            if (accepted)
            {
                SolValidCheck[order1][dag1]--;
                AddSpecificOrder(Truck2, dag2, order1, route2, index2, Capaciteit2, Truck2Rijtijden);
                SolValidCheck[order1][dag1]++;
            }
        }

        void ShiftRandomOrderWithinRoute()
        {
            Truck t;
            truck = RNG.Next(2);
            t = oldSolution[truck];

            SolutionStrafIntern = oldSolution.StrafIntern;
            SolutionRijtijd = oldSolution.Rijtijd;
            SolValidCheck = oldSolution.ValidCheck;
            t.Rijtijden.CopyTo(Truck1Rijtijden, 0);

            dag1 = RNG.Next(5);
            route1 = RNG.Next(t.Dagen[dag1].Count);

            if (t.Dagen[dag1][route1].Item1.Count < 1)
            {
                accepted = false;
                return;
            }

            index1 = RNG.Next(t.Dagen[dag1][route1].Item1.Count);
            index2 = RNG.Next(t.Dagen[dag1][route1].Item1.Count - 1);
            if(index1 == index2)
            {
                accepted = false;
                return;
            }

            order1 = t.Dagen[dag1][route1].Item1[index1];

            Capaciteit1 = new Capaciteit(t.Dagen[dag1][route1].Item2.Value);
            RemoveSpecificOrder(t, dag1, route1, index1, Capaciteit1, Truck1Rijtijden);
            if (accepted)
                if (index2 >= index1)
                {
                    if(index2 - index1 == 1)
                    {
                        AddSpecificOrder(t, dag1, order1, route1, index2, Capaciteit1, Truck1Rijtijden, index1 == 0 ? 287 : OrdersDict[t.Dagen[dag1][route1].Item1[index1 - 1]].MatrixID, OrdersDict[t.Dagen[dag1][route1].Item1[index2]].MatrixID);
                    }
                    else
                        AddSpecificOrder(t, dag1, order1, route1, index2, Capaciteit1, Truck1Rijtijden);
                }
                else
                {
                    AddSpecificOrder(t, dag1, order1, route1, index2, Capaciteit1, Truck1Rijtijden);
                }
        }

        void ShiftRandomOrderBetweenRoutes()
        {
            Truck t;
            truck = RNG.Next(2);
            if (truck == 0)
                t = oldSolution.Truck1;
            else
                t = oldSolution.Truck2;

            SolutionStrafIntern = oldSolution.StrafIntern;
            SolutionRijtijd = oldSolution.Rijtijd;
            SolValidCheck = oldSolution.ValidCheck;
            t.Rijtijden.CopyTo(Truck1Rijtijden, 0);

            dag1 = RNG.Next(5);
            route1 = RNG.Next(t.Dagen[dag1].Count);
            route2 = RNG.Next(t.Dagen[dag1].Count);

            if (route1 == route2)
            {
                accepted = false;
                return;
            }

            if (t.Dagen[dag1][route1].Item1.Count < 1)
            {
                accepted = false;
                return;
            }

            index1 = RNG.Next(t.Dagen[dag1][route1].Item1.Count);
            index2 = RNG.Next(t.Dagen[dag1][route2].Item1.Count);

            order1 = t.Dagen[dag1][route1].Item1[index1];

            Capaciteit1 = new Capaciteit(t.Dagen[dag1][route1].Item2.Value);
            Capaciteit2 = new Capaciteit(t.Dagen[dag1][route2].Item2.Value);
            RemoveSpecificOrder(t, dag1, route1, index1, Capaciteit1, Truck1Rijtijden);
            AddSpecificOrder(t, dag1, order1, route2, index2, Capaciteit2, Truck1Rijtijden);
        }

        void TwoAndAHalfOpt()
        {
            // TODO
        }

        void TwoOpt()
        {
            // TODO
        }

        void UpdateValid(int orderId, bool validVoor, bool validNa)
        {
            if (validVoor)
            {
                SolutionStrafpunten += OrdersDict[orderId].Frequentie * OrdersDict[orderId].LedigingDuurMinuten * PenaltyModifier;
                //StrafIntern += OrdersDict[orderId].Frequentie * OrdersDict[orderId].LedigingDuurMinuten * 1000d;
            }
            else if (validNa)
            {
                SolutionStrafpunten -= OrdersDict[orderId].Frequentie * OrdersDict[orderId].LedigingDuurMinuten * PenaltyModifier;
                //StrafIntern -= OrdersDict[orderId].Frequentie * OrdersDict[orderId].LedigingDuurMinuten * 1000d;
            }
        }

        void UpdateCapaciteit(double capaciteitVoor, double capaciteitNa)
        {
            if (capaciteitVoor <= MaxCapaciteit && capaciteitNa > MaxCapaciteit)
                SolutionStrafIntern += CapaciteitStrafLiter * (capaciteitNa - MaxCapaciteit) + CapaciteitStraf;
            else if (capaciteitVoor > MaxCapaciteit && capaciteitNa <= MaxCapaciteit)
                SolutionStrafIntern -= CapaciteitStrafLiter * (capaciteitVoor - MaxCapaciteit) + CapaciteitStraf;
            else if (capaciteitVoor > MaxCapaciteit && capaciteitNa > MaxCapaciteit)
                SolutionStrafIntern += CapaciteitStrafLiter * (capaciteitNa - capaciteitVoor);
        }
        void UpdateRijtijdStraf(double oud, double nieuw)
        {
            if (oud <= MaxRijtijdDag && nieuw > MaxRijtijdDag)
                SolutionStrafIntern += (nieuw - MaxRijtijdDag) * RijtijdStrafMinuut + RijtijdStraf;
            else if (oud > MaxRijtijdDag && nieuw <= MaxRijtijdDag)
                SolutionStrafIntern -= (oud - MaxRijtijdDag) * RijtijdStrafMinuut + RijtijdStraf;
            else if (oud > MaxRijtijdDag && nieuw > MaxRijtijdDag)
                SolutionStrafIntern += (nieuw - oud) * RijtijdStrafMinuut;
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
    }
}
