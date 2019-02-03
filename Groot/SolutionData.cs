using System;
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
        public Capaciteit Capaciteit1, Capaciteit2, Capaciteit3, Capaciteit4;
        public Dictionary<int, ValidArray> SolValidCheck;
        public double SolutionRijtijd, SolutionStrafpunten, SolutionStrafIntern;
        public int Choice, truck, truck1, truck2, truck3, truck4, dag1, dag2, dag3, dag4, route1, route2, route3, route4, index1, index2, index3, index4, order1, order2, frequentie, dagselectie;
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

            if(oldSolution.OrderTrucks[order1].Count > 0)
            {
                allow = false;
                return;
            }

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
                    if (dagselectie == 0)
                    {
                        dag1 = 0;
                        dag2 = 3;
                    }
                    else
                    {
                        dag1 = 1;
                        dag2 = 4;
                    }
                    route1 = RNG.Next(t.Dagen[dag1].Count);
                    index1 = RNG.Next(t.Dagen[dag1][route1].Item1.Count);
                    Capaciteit1 = new Capaciteit(t.Dagen[dag1][route1].Item2.Value);
                    AddSpecificOrder(t, dag1, order1, route1, index1, Capaciteit1, Truck1Rijtijden);

                    route2 = RNG.Next(t.Dagen[dag2].Count);
                    index2 = RNG.Next(t.Dagen[dag2][route2].Item1.Count);
                    Capaciteit2 = new Capaciteit(t.Dagen[dag2][route2].Item2.Value);
                    SolValidCheck[order1][dag1]++;
                    AddSpecificOrder(t, dag2, order1, route2, index2, Capaciteit2, Truck1Rijtijden);
                    SolValidCheck[order1][dag1]--;
                    break;
                case 3:
                    dag1 = 0; dag2 = 2; dag3 = 4;

                    route1 = RNG.Next(t.Dagen[dag1].Count);
                    index1 = RNG.Next(t.Dagen[dag1][route1].Item1.Count);
                    Capaciteit1 = new Capaciteit(t.Dagen[dag1][route1].Item2.Value);
                    AddSpecificOrder(t, dag1, order1, route1, index1, Capaciteit1, Truck1Rijtijden);

                    route2 = RNG.Next(t.Dagen[dag2].Count);
                    index2 = RNG.Next(t.Dagen[dag2][route2].Item1.Count);
                    Capaciteit2 = new Capaciteit(t.Dagen[dag2][route2].Item2.Value);
                    SolValidCheck[order1][dag1]++;
                    AddSpecificOrder(t, dag2, order1, route2, index2, Capaciteit2, Truck1Rijtijden);

                    route3 = RNG.Next(t.Dagen[dag3].Count);
                    index3 = RNG.Next(t.Dagen[dag3][route3].Item1.Count);
                    Capaciteit3 = new Capaciteit(t.Dagen[dag3][route3].Item2.Value);
                    SolValidCheck[order1][dag2]++;
                    AddSpecificOrder(t, dag3, order1, route3, index3, Capaciteit3, Truck1Rijtijden);
                    SolValidCheck[order1][dag1]--;
                    SolValidCheck[order1][dag2]--;
                    break;
                case 4:
                    List<int> selectie = new List<int>(){ 0, 1, 2, 3, 4 };
                    selectie.RemoveAt(RNG.Next(5));
                    dag1 = selectie[0]; dag2 = selectie[1]; dag3 = selectie[2]; dag4 = selectie[3];

                    route1 = RNG.Next(t.Dagen[dag1].Count);
                    index1 = RNG.Next(t.Dagen[dag1][route1].Item1.Count);
                    Capaciteit1 = new Capaciteit(t.Dagen[dag1][route1].Item2.Value);
                    AddSpecificOrder(t, dag1, order1, route1, index1, Capaciteit1, Truck1Rijtijden);

                    route2 = RNG.Next(t.Dagen[dag2].Count);
                    index2 = RNG.Next(t.Dagen[dag2][route2].Item1.Count);
                    Capaciteit2 = new Capaciteit(t.Dagen[dag2][route2].Item2.Value);
                    SolValidCheck[order1][dag1]++;
                    AddSpecificOrder(t, dag2, order1, route2, index2, Capaciteit2, Truck1Rijtijden);

                    route3 = RNG.Next(t.Dagen[dag3].Count);
                    index3 = RNG.Next(t.Dagen[dag3][route3].Item1.Count);
                    Capaciteit3 = new Capaciteit(t.Dagen[dag3][route3].Item2.Value);
                    SolValidCheck[order1][dag2]++;
                    AddSpecificOrder(t, dag3, order1, route3, index3, Capaciteit3, Truck1Rijtijden);

                    route4 = RNG.Next(t.Dagen[dag4].Count);
                    index4 = RNG.Next(t.Dagen[dag4][route4].Item1.Count);
                    Capaciteit4 = new Capaciteit(t.Dagen[dag4][route4].Item2.Value);
                    SolValidCheck[order1][dag3]++;
                    AddSpecificOrder(t, dag4, order1, route4, index4, Capaciteit4, Truck1Rijtijden);
                    SolValidCheck[order1][dag1]--;
                    SolValidCheck[order1][dag2]--;
                    SolValidCheck[order1][dag3]--;
                    break;
            }
            allow = true;
        }

        void RemoveRandomOrder()
        {
            Truck t;
            truck = RNG.Next(2);
            t = oldSolution[truck];

            SolutionStrafpunten = oldSolution.Strafpunten;
            SolutionStrafIntern = oldSolution.StrafIntern;
            SolutionRijtijd = oldSolution.Rijtijd;
            SolValidCheck = oldSolution.ValidCheck;
            oldSolution[0].Rijtijden.CopyTo(Truck1Rijtijden, 0);
            oldSolution[1].Rijtijden.CopyTo(Truck2Rijtijden, 0);

            double[][] alleRijtijden = new double[2][];
            alleRijtijden[0] = Truck1Rijtijden;
            alleRijtijden[1] = Truck2Rijtijden;

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

            frequentie = oldSolution.OrderTrucks[order1].Count;

            switch(frequentie)
            {
                case 1:
                    Capaciteit1 = new Capaciteit(t.Dagen[dag1][route1].Item2.Value);
                    RemoveSpecificOrder(t, dag1, route1, index1, Capaciteit1, alleRijtijden[truck]);
                    break;
                case 2:
                    truck1 = oldSolution.OrderTrucks[order1][0].Truck;
                    dag1 = oldSolution.OrderTrucks[order1][0].Dag;
                    route1 = oldSolution.OrderTrucks[order1][0].Route;
                    index1 = oldSolution[truck1].Dagen[dag1][route1].Item1.FindIndex((o) => o == order1);
                    Capaciteit1 = new Capaciteit(oldSolution[truck1].Dagen[dag1][route1].Item2.Value);
                    RemoveSpecificOrder(oldSolution[truck1], dag1, route1, index1, Capaciteit1, alleRijtijden[truck1]);

                    truck2 = oldSolution.OrderTrucks[order1][1].Truck;
                    dag2 = oldSolution.OrderTrucks[order1][1].Dag;
                    route2 = oldSolution.OrderTrucks[order1][1].Route;
                    index2 = oldSolution[truck2].Dagen[dag2][route2].Item1.FindIndex((o) => o == order1);
                    Capaciteit2 = new Capaciteit(oldSolution[truck2].Dagen[dag2][route2].Item2.Value);
                    SolValidCheck[order1][dag1]--;
                    RemoveSpecificOrder(oldSolution[truck2], dag2, route2, index2, Capaciteit2, alleRijtijden[truck2]);
                    SolValidCheck[order1][dag1]++;
                    break;
                case 3:
                    truck1 = oldSolution.OrderTrucks[order1][0].Truck;
                    dag1 = oldSolution.OrderTrucks[order1][0].Dag;
                    route1 = oldSolution.OrderTrucks[order1][0].Route;
                    index1 = oldSolution[truck1].Dagen[dag1][route1].Item1.FindIndex((o) => o == order1);
                    Capaciteit1 = new Capaciteit(oldSolution[truck1].Dagen[dag1][route1].Item2.Value);
                    RemoveSpecificOrder(oldSolution[truck1], dag1, route1, index1, Capaciteit1, alleRijtijden[truck1]);

                    truck2 = oldSolution.OrderTrucks[order1][1].Truck;
                    dag2 = oldSolution.OrderTrucks[order1][1].Dag;
                    route2 = oldSolution.OrderTrucks[order1][1].Route;
                    index2 = oldSolution[truck2].Dagen[dag2][route2].Item1.FindIndex((o) => o == order1);
                    Capaciteit2 = new Capaciteit(oldSolution[truck2].Dagen[dag2][route2].Item2.Value);
                    SolValidCheck[order1][dag1]--;
                    RemoveSpecificOrder(oldSolution[truck2], dag2, route2, index2, Capaciteit2, alleRijtijden[truck2]);

                    truck3 = oldSolution.OrderTrucks[order1][2].Truck;
                    dag3 = oldSolution.OrderTrucks[order1][2].Dag;
                    route3 = oldSolution.OrderTrucks[order1][2].Route;
                    index3 = oldSolution[truck3].Dagen[dag3][route3].Item1.FindIndex((o) => o == order1);
                    Capaciteit3 = new Capaciteit(oldSolution[truck3].Dagen[dag3][route3].Item2.Value);
                    SolValidCheck[order1][dag2]--;
                    RemoveSpecificOrder(oldSolution[truck3], dag3, route3, index3, Capaciteit3, alleRijtijden[truck3]);
                    SolValidCheck[order1][dag1]++;
                    SolValidCheck[order1][dag2]++;
                    break;
                case 4:
                    truck1 = oldSolution.OrderTrucks[order1][0].Truck;
                    dag1 = oldSolution.OrderTrucks[order1][0].Dag;
                    route1 = oldSolution.OrderTrucks[order1][0].Route;
                    index1 = oldSolution[truck1].Dagen[dag1][route1].Item1.FindIndex((o) => o == order1);
                    Capaciteit1 = new Capaciteit(oldSolution[truck1].Dagen[dag1][route1].Item2.Value);
                    RemoveSpecificOrder(oldSolution[truck1], dag1, route1, index1, Capaciteit1, alleRijtijden[truck1]);

                    truck2 = oldSolution.OrderTrucks[order1][1].Truck;
                    dag2 = oldSolution.OrderTrucks[order1][1].Dag;
                    route2 = oldSolution.OrderTrucks[order1][1].Route;
                    index2 = oldSolution[truck2].Dagen[dag2][route2].Item1.FindIndex((o) => o == order1);
                    Capaciteit2 = new Capaciteit(oldSolution[truck2].Dagen[dag2][route2].Item2.Value);
                    SolValidCheck[order1][dag1]--;
                    RemoveSpecificOrder(oldSolution[truck2], dag2, route2, index2, Capaciteit2, alleRijtijden[truck2]);

                    truck3 = oldSolution.OrderTrucks[order1][2].Truck;
                    dag3 = oldSolution.OrderTrucks[order1][2].Dag;
                    route3 = oldSolution.OrderTrucks[order1][2].Route;
                    index3 = oldSolution[truck3].Dagen[dag3][route3].Item1.FindIndex((o) => o == order1);
                    Capaciteit3 = new Capaciteit(oldSolution[truck3].Dagen[dag3][route3].Item2.Value);
                    SolValidCheck[order1][dag2]--;
                    RemoveSpecificOrder(oldSolution[truck3], dag3, route3, index3, Capaciteit3, alleRijtijden[truck3]);

                    truck4 = oldSolution.OrderTrucks[order1][3].Truck;
                    dag4 = oldSolution.OrderTrucks[order1][3].Dag;
                    route4 = oldSolution.OrderTrucks[order1][3].Route;
                    index4 = oldSolution[truck4].Dagen[dag4][route4].Item1.FindIndex((o) => o == order1);
                    Capaciteit4 = new Capaciteit(oldSolution[truck4].Dagen[dag4][route4].Item2.Value);
                    SolValidCheck[order1][dag3]--;
                    RemoveSpecificOrder(oldSolution[truck4], dag4, route4, index4, Capaciteit4, alleRijtijden[truck4]);
                    SolValidCheck[order1][dag1]++;
                    SolValidCheck[order1][dag2]++;
                    SolValidCheck[order1][dag3]++;
                    break;
            }
            allow = true;
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

            double oudRijtijd1 = t.Rijtijden[dag1];
            double oudRijtijd2 = t.Rijtijden[dag2];

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
            int swapTo = truck == 0 ? 1 : 0;

            Truck Truck1 = oldSolution[truck];
            Truck Truck2 = oldSolution[swapTo];

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

        void UpdateValid(int orderId, bool validVoor, bool validNa)
        {
            if (validVoor)
            {
                SolutionStrafpunten += OrdersDict[orderId].Frequentie * OrdersDict[orderId].LedigingDuurMinuten * PenaltyModifier;
                allow = false;
            }
            else if (validNa)
            {
                SolutionStrafpunten -= OrdersDict[orderId].Frequentie * OrdersDict[orderId].LedigingDuurMinuten * PenaltyModifier;
                allow = true;
            }
        }
    }
}
