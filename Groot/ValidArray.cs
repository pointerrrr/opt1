using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groot
{


    public class ValidArray
    {
        public int Dag1, Dag2, Dag3, Dag4, Dag5, Frequentie;

        public bool Valid
        {
            get
            {
                switch (Frequentie)
                {
                    case 1:
                        int count = 0;
                        for (int i = 0; i < 5; i++)
                            if (this[i] == 1)
                                count++;
                            else if (this[i] > 1)
                                return false;
                        return count == 1;
                    case 2:
                        return (this[0] == 1 && this[1] == 0 && this[2] == 0 && this[3] == 1 && this[4] == 0) || (this[0] == 0 && this[1] == 1 && this[2] == 0 && this[3] == 0 && this[4] == 1);
                    case 3:
                        return this[0] == 1 && this[1] == 0 && this[2] == 1 && this[3] == 0 && this[4] == 1;
                    case 4:
                        count = 0;
                        for (int i = 0; i < 5; i++)
                            if (this[i] == 1)
                                count++;
                            else if (this[i] > 1)
                                return false;
                        return count == 4;
                    case 5:
                        return this[0] == 1 && this[1] == 1 && this[2] == 1 && this[3] == 1 && this[4] == 1;
                    default:
                        return false;
                }
            }
        }


        public ValidArray(int a = 0, int b = 0, int c = 0, int d = 0, int e = 0, int f = 1)
        {
            Dag1 = a; Dag2 = b; Dag3 = c; Dag4 = d; Dag5 = e; Frequentie = f;
        }

        public ValidArray Copy()
        {
            ValidArray res = new ValidArray(Dag1, Dag2, Dag3, Dag4, Dag5, Frequentie);
            return res;
        }

        public static bool operator ==(ValidArray a, ValidArray b)
        {
            return a[0] == b[0] && a[1] == b[1] && a[2] == b[2] && a[3] == b[3] && a[4] == b[4] && a.Frequentie == b.Frequentie;
        }

        public static bool operator !=(ValidArray a, ValidArray b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return this == obj as ValidArray;
        }

        public int this[int key]
        {
            get
            {
                switch (key)
                {
                    case 0:
                        return Dag1;
                    case 1:
                        return Dag2;
                    case 2:
                        return Dag3;
                    case 3:
                        return Dag4;
                    case 4:
                        return Dag5;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (key)
                {
                    case 0:
                        Dag1 = value;
                        break;
                    case 1:
                        Dag2 = value;
                        break;
                    case 2:
                        Dag3 = value;
                        break;
                    case 3:
                        Dag4 = value;
                        break;
                    case 4:
                        Dag5 = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        public override int GetHashCode()
        {
            return Dag1 + Dag2*10 + Dag3*100 + Dag4*1000 + Dag5*100000 + Frequentie*1000000;
        }
    }
}
