using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemoryGameI
{
    public class Polje
    {
        protected int num;//indekx slike
        protected int stanje;//prilikom random postavljanja da zna gde je vec postavljeno
        protected bool zauzeto;//kod igre da znam sta je pogodjeno a sta ne
        public Polje()
        {
            num = 0;
            stanje = 1; //1- slobodno 0-zauzeto ovo smao prilikom postavljanja elementata
            zauzeto = false;//true zauzeto ne mozes da postavis false zauzeto i mozes da postavis
        }
        public int Num
        {
            get { return num; }
            set { num = value; }
        }
        public int Stanje
        {
            get { return stanje; }
            set { stanje = value; }
        }
        public bool ZAUZETO
        {
            get { return zauzeto; }
            set { zauzeto = value; }
        }
    }
}
