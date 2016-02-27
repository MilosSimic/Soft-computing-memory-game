using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemoryGameI
{
    public class MyException:Exception
    {
        public MyException() : base("Izuzetak") { }
        public MyException(string message) : base(message) { }
        int t=1;
        //metoda za ispitivanje dal su otvorene slike OK
        public bool test(ImageOpen image1, ImageOpen image2)
        {
            if (image1.NUM == image2.NUM)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public int getTimeo(){return t;}
        public bool test(int[] niz)
        {
            if (niz[0] == niz[2] && niz[1] == niz[3])
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool test(Test image1, Test image2) {
            if (image1.NUM==image2.NUM)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool opentest(int k)
        {
            if (k == 3)
            {
                //k = 1;//ponisti ga zbog sledecih otvaranja
                return true;
            }
            else {
                return false;
            }
        }
    }
}
