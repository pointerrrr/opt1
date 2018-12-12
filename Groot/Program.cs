﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Groot
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] afstanden = File.ReadAllLines(@"..\..\afstanden.txt");

            int maxLenght = int.Parse(afstanden[afstanden.Length - 1].Split(';')[0]);
            
            AfstandRijtijd[,] afstandenMatrix = new AfstandRijtijd[maxLenght + 1, maxLenght + 1];

            for (int i = 1; i < afstanden.Length - 1; i++)
            {
                string rij = afstanden[i];
                string[] info = rij.Split(';');
                int id1 = int.Parse(info[0]);
                int id2 = int.Parse(info[1]);
                int afstand = int.Parse(info[2]);
                int rijtijd = int.Parse(info[3]);
                afstandenMatrix[id1, id2] = new AfstandRijtijd(afstand, rijtijd);                
            }

            

            string[] orderbestand = File.ReadAllLines(@"..\..\orderbestand.txt");

            OrderDescription[] orders = new OrderDescription[orderbestand.Length];

            for(int i = 1; i < orderbestand.Length - 1; i++)
            {
                string rij = orderbestand[i];

                string[] info = rij.Split(';');
                int a = int.Parse(info[0]);
                string b = info[1];
                int c = int.Parse(info[2][0].ToString());
                int d = int.Parse(info[3]);
                int e = int.Parse(info[4]);
                float f = float.Parse(info[5]);
                int g = int.Parse(info[6]);
                int h = int.Parse(info[7]);
                int ib = int.Parse(info[8]);

                orders[i] = new OrderDescription(a, b, c, d, e, f, g, h, i);
            }

            Console.ReadLine();
        }
    }

    public struct AfstandRijtijd
    {
        public int Afstand, Rijtijd;

        public AfstandRijtijd(int a, int b)
        {
            Afstand = a;
            Rijtijd = b;
        }
    }

    public struct OrderDescription
    {
        public int Order, Frequentie, AantContainers, VolumePerContainer, MatrixID, XCoordinaat, YCoordinaat;
        public float LedigingDuurMinuten;
        public string Plaats;

        public OrderDescription(int a, string b, int c, int d, int e, float f, int g, int h, int i)
        {
            Order = a;
            Plaats = b;
            Frequentie = c;
            AantContainers = d;
            VolumePerContainer = e;
            LedigingDuurMinuten = f;
            MatrixID = g;
            XCoordinaat = h;
            YCoordinaat = i;
        }
    }
}
