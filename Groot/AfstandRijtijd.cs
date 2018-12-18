using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groot
{
    public class AfstandRijtijd
    {
        public int Id1, Id2;
        public double Afstand, Rijtijd;
        public AfstandRijtijd(int id1, int id2, double afstand, double rijtijd)
        {
            Id1 = id1; Id2 = id2;
            Afstand = afstand; Rijtijd = rijtijd;
        }
    }
}
