using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemoryGameI
{
    public class Test:ImageOpen
    {
        int i,j;
        public Test():base(){
            i = 0;
            j = 0;
        }
        public Test(int p,bool d,int i,int j):base(p,d) {
            this.i = i;
            this.j = j;
        }
        public int I
        {
            get { return i; }
            set { i = value; }
        }
        public int J
        {
            get { return j; }
            set { j = value; }
        }
        public int test(int i,int j,int i1,int j1,Tabla t)
        {
            int k=t.retIndex(i,j);
            int p = t.retIndex(i1, j1);

            if (k == p)
            {
                return 1;
            }
            else {
                return 0;
            }
        }
        public void setElems(int ii, int jj) {
            i = ii;
            j = jj;
        }
    }
}
