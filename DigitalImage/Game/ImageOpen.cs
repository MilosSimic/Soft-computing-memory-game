using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemoryGameI
{
    public class ImageOpen
    {
        private int num;
        private bool state;
        public ImageOpen() { num = 0; state = false; }
        public ImageOpen(int ii, bool jj) { num = ii; state = jj; }
        public int NUM
        {
            get { return num; }
            set { num = value; }
        }
        public bool STATE
        {
            get { return state; }
            set { state = value; }
        }
        public void erase()
        {
            num = 0;
            state = false;
        }
    }
}
