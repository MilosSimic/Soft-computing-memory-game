using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemoryGameI
{
    public class Tabla
    {
        protected Polje[,] tabela;
        int broj_slika;
        public Tabla()
        {
            broj_slika = 0;
            tabela = new Polje[6, 6];
            

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    Polje p = new Polje();
                    tabela[i, j] = p;
                    tabela[i, j].Num = 0;
                    tabela[i, j].Stanje = 1;
                    tabela[i, j].ZAUZETO = false;
                }
            }
        }
        public int BROJ_SLIKA
        {
            get { return broj_slika; }
            set { broj_slika = value; }
        }
        public void Popuni()
        {
            Random rndC = new Random();
            int iC;
            int iR;
            int brojac = 0;
            int index = 0;
            //int[] brojevi = { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5};
            int[] brojevi = { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5,6,6,7,7,8,8,9,9,10,10,11,11,12,12,13,13,14,14 };
            bool prosao = false;

            do
            {
                iC = 0;
                iR = 0;
                //daj neke indexe
                iC = rndC.Next() % 6;
                iR = rndC.Next() % 6;
                //ako je polje slobodno upisi
                if (tabela[iC, iR].Stanje == 1)
                {
                    tabela[iC, iR].Num = brojevi[index]; //postavi sliku,tj indeks slike
                    tabela[iC, iR].Stanje = 0; // postavi da je zauzeto polje
                    tabela[iC, iR].ZAUZETO = false;//nije zauzeto!

                    //broj_slika = brojevi.Count() - 1;//broj slika..ovo bi trebalo da postavi na numerir-u vrednost
                    if (index == broj_slika)
                    {
                        index = 0;
                    }
                    else
                    {
                        index++;
                    }

                    brojac++; //postavio je element

                }

                //proveri dal je pounio celu tablu
                if (brojac == 36)
                {
                    prosao = true;
                }
                else
                {
                    prosao = false;
                }


            } while (!prosao);
            
        }
        //vraca index zauzete pozicije,tj ovo je index ikone
        public int retIndex(int i, int j)
        {
            return tabela[i, j].Num;
        }
        public void print()
        {
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {

                    Console.Write(tabela[i,j].Num+" ");
                }
                Console.WriteLine();
            }
        }
        //ovim se resetuje ne pocetak
        public void obrisi()
        {
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    tabela[i, j].Num = 0;
                    tabela[i, j].Stanje = 1;
                    tabela[i, j].ZAUZETO = false;
                }
            }
        }
        public int getStanje(int i, int j)
        {
            return tabela[i, j].Stanje;
        }
        public void setStanje(int i, int j, int s)
        {

            tabela[i, j].Stanje = s;
        }
        public int getNum(int i,int j)
        {
            return tabela[i,j].Num;
        }
        public void setNum(int i, int j, int n)
        {

            tabela[i, j].Num = n;
        }
        public bool getZauzeto(int i, int j)
        {
            return tabela[i, j].ZAUZETO;
        }
        public void setZauzeto(int i,int j,bool z)
        {
            tabela[i,j].ZAUZETO=z;
        }
    }
}


