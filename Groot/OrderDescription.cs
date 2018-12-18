using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groot
{
    public struct OrderDescription
    {
        public int Order, Frequentie, AantContainers, VolumePerContainer, MatrixID, XCoordinaat, YCoordinaat;
        public double LedigingDuurMinuten;
        public string Plaats;
        public int[] ValidCheck;

        public OrderDescription(int a, string b, int c, int d, int e, double f, int g, int h, int i)
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
            ValidCheck = new int[5];
        }
    }
}
