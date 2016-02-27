using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OcrTemplate.Utilities;
using DigitalImage.util;
using AGisCore;
using AGisCore.Entity;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data.Common;
using DigitalImage.neuronska;
using AForge.Video.DirectShow;
using AForge.Video;
using MemoryGameI;
using System.Runtime.InteropServices;
using System.Threading;


namespace DigitalImage
{
    public partial class MainForm : Form
    {
        #region Atributi

        public const int TimeToOpen = 2;
        private int brojac = 0;
        private int indexer = -1;
        Point pt = new Point();
        EventArgs evt = new EventArgs();
        private const int prag = 2000;
        private const int frames = 20;
        private int frame = 0;

        CardsRegions cardsRegion = new CardsRegions();

        bool isMarkerRekognised = false;
        bool isTableRekognised = false;

        Dictionary<string, int> scoreTabela;
        Random broj;
        int testg;
        int prvi_put;
        int fi, si, fj, sj;
        int timeo;//koji put otvaram
        int open;//zbog otvaranja da probam sa povecavanjem i while
        Test image1, image2;
        bool see, see2;
        Tabla tabla;//tabla na kojoj se igra
        int[] niz;
        Label[] nizL;//niz labela na kojima se pokazuju slike
        Label[] nizS;//niz labela na koje kriju slike
        Label[,] mreza;//matrica onih sa slikama-kao tabela
        Label[,] mreza2;//matrica onih koje pokrivaju slike-kao tabela
        int cntrl;
        
        BackPropagation.BackPropagation bp = null;

        VideoCaptureDevice videoSource = null;
        bool webcamstarted = false;

        #endregion Atributi

        #region Konstruktor

        public MainForm()
        {
            prvi_put = 0;
            broj = new Random(123456);
            timeo = 0;
            cntrl = 0;
            testg = 0;
            fi = 0;
            fj = 0;
            si = 0;
            sj = 0;
            open = 0;
            see = false;
            see2 = false;
            //podatci da vidim gde mogu da smestim indexe za ispitivanje
            image1 = new Test(0, false, fi, fj);
            image2 = new Test(0, false, si, sj);
            scoreTabela = new Dictionary<string, int>();
            InitializeComponent();
            tabla = new Tabla();
            niz = new int[36];
            mreza2 = new Label[6, 6];
            nizL = new Label[] { A0,A1,A2,A3,A4,A5,B0,B1,B2,B3,B4,B5,
                                 C0,C1,C2,C3,C4,C5,D0,D1,D2,D3,D4,D5,
                                 E0,E1,E2,E3,E4,E5,F0,F1,F2,F3,F4,F5
                                };
            nizS = new Label[] { A00,A01,A02,A03,A04,A05,B00,B01,B02,B03,B04,B05,
                                 C00,C01,C02,C03,C04,C05,D00,D01,D02,D03,D04,D05,
                                 E00,E01,E02,E03,E04,E05,F00,F01,F02,F03,F04,F05
                                };
            mreza = new Label[6, 6];

            for(int i=0;i<36;i++)
            {
                nizL[i].ImageList = imageList1;
            }

            int ind = 0;
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    if (cntrl == 0)
                    {
                        mreza[i, j] = nizS[ind];
                        mreza2[i, j] = nizL[ind];
                        cntrl++;
                    }
                    else
                    {
                        if (ind == 35)
                        {
                            ind = 0;
                        }
                        else
                        {
                            ind++;
                            mreza[i, j] = nizS[ind];
                            mreza2[i, j] = nizL[ind];
                        }
                    }
                    mreza[i, j].BackColor = Color.DarkRed;
                    mreza[i, j].Visible = true;
                    mreza[i, j].Cursor = Cursors.Hand;
                }
            }

            button1.Enabled = false;
            button2.Enabled = false;

        }

        #endregion Konstruktor

        #region ostalo

        Bitmap prevImage = null;
        private void btnLoadBitmap_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                String imageFileName = ofd.FileName;
                Bitmap bmp = new Bitmap(imageFileName);
                prevImage = imageEditorDisplay1.mapa.bmp;

                imageEditorDisplay1.mapa.bmp = bmp;
                btnClear_Click(null, null);
                imageEditorDisplay1.FitImage();
                imageEditorDisplay1.Refresh();

                Properties.Settings.Default.imagePath = imageFileName;
                Properties.Settings.Default.Save();

            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog ofd = new SaveFileDialog();
            string fileName = DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss") + ".bmp";
            ofd.FileName = fileName;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                String imageFileName = ofd.FileName;
                imageEditorDisplay1.mapa.bmp.Save(imageFileName);
            }
        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            String str = Properties.Settings.Default.imagePath;
            if (!String.IsNullOrEmpty(str))
            {
                Bitmap bmp = new Bitmap(str);

                imageEditorDisplay1.mapa.bmp = bmp;
                imageEditorDisplay1.FitImage();
                imageEditorDisplay1.Refresh();
            }

        }

        private void btnBinaryTiles_Click(object sender, EventArgs e)
        {
            Bitmap bmp = imageEditorDisplay1.mapa.bmp;
            byte[,] slika = ImageUtil.bitmapToByteMatrix(bmp);
            byte[,] bSlika = ImageUtil.matrixToBinaryTiles(slika, 15, 15);
            Bitmap temp = ImageUtil.matrixToBitmap(bSlika);
            imageEditorDisplay1.mapa.bmp = temp;
            imageEditorDisplay1.FitImage();
            imageEditorDisplay1.Refresh();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            String str = Properties.Settings.Default.imagePath;
            if (!String.IsNullOrEmpty(str))
            {
                Bitmap bmp = new Bitmap(str);

                imageEditorDisplay1.mapa.bmp = bmp;
                imageEditorDisplay1.FitImage();
                imageEditorDisplay1.Refresh();
            }

            //Init igre
            Game_load();
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            imageEditorDisplay1.CopyToClipboard();
        }

        List<RasterRegion> kandidati = null;
        private void btnRegionLabeling_Click(object sender, EventArgs e)
        {

            Bitmap bmp = imageEditorDisplay1.mapa.bmp;
            byte[,] oSlika = ImageUtil.bitmapToByteMatrix(bmp);
            byte[,] slika = ImageUtil.matrixToBinaryTiles(oSlika, 15, 15);
            int w = slika.GetLength(1);
            int h = slika.GetLength(0);

            kandidati = ImageUtil.regionLabeling(slika);
            foreach (RasterRegion reg in kandidati)
            {
                reg.odrediMomente();
            }

            // ovde sloziti slova u redove i kolone :)))
            String[] linije = tbTrenningSet.Text.Replace("\r", "").Split('\n');
            String slova = tbTrenningSet.Text.Replace("\n", "").Replace("\r", "");
            int rr = 0;
            List<RasterRegion> slozeniKandidati = new List<RasterRegion>();
            for (int i = 0; i < linije.Length; i++)
            {
                String linija = linije[i];
                int N = linija.Length;
                List<RasterRegion> red = new List<RasterRegion>();
                for (int j = 0; j < N; j++)
                {
                    red.Add(kandidati[rr]);
                    rr++;
                }
                red.Sort(new DigitalImage.util.RasterRegion.RComparer());
                foreach (RasterRegion regS in red)
                    slozeniKandidati.Add(regS);
            }
            kandidati = slozeniKandidati;

            int regId = 0;
            foreach (RasterRegion reg in kandidati)
            {
                List<Tacka> tacke = new List<Tacka>();
                tacke.Add(new Tacka(reg.minX - 1, reg.minY - 1));
                tacke.Add(new Tacka(reg.maxX + 1, reg.minY - 1));
                tacke.Add(new Tacka(reg.maxX + 1, reg.maxY + 1));
                tacke.Add(new Tacka(reg.minX - 1, reg.maxY + 1));
                Poligon rec = new Poligon(tacke, Color.FromArgb(10, Color.Green));
                imageEditorDisplay1.selectedIndexer.add(rec);
                reg.Tag = "" + slova[regId];
                Labela lbl = new Labela(reg.minX - 1, reg.minY - 1);
                lbl.boja = Color.Red;
                lbl.labela = reg.Tag;
                imageEditorDisplay1.selectedIndexer.add(lbl);
                regId++;
            }
            //imageEditorDisplay1.mapa.bmp = ImageUtil.matrixToBitmap(slika);
            imageEditorDisplay1.FitImage();
            imageEditorDisplay1.Refresh();
        }


        private static int BROJ_CIFARA = 10;
        double[, ,] obucavajuciSkup = null;
        int brojUzoraka = 0;
        private void btnTrainingSet_Click(object sender, EventArgs e)
        {
            brojUzoraka = kandidati.Count;
            obucavajuciSkup = new double[brojUzoraka, 2, 64];
            Size size = new Size(64, 64);
            for (int uzorak = 0; uzorak < brojUzoraka; uzorak++)
            {
                RasterRegion reg = kandidati[uzorak];
                byte[,] regSlika = reg.odrediNSliku();
                byte[,] nSlika = ImageUtil.resizeImage(regSlika, size);
                double[] ulaz = pripremiSlikuZaVNM(nSlika);

                for (int k = 0; k < ulaz.Length; k++)
                {
                    obucavajuciSkup[uzorak, 0, k] = ulaz[k];
                }
                int cifra = int.Parse(reg.Tag);
                for (int ii = 0; ii < BROJ_CIFARA; ii++)
                {
                    if (ii == cifra)
                        obucavajuciSkup[uzorak, 1, ii] = 1;
                    else
                        obucavajuciSkup[uzorak, 1, ii] = 0;
                }
                Bitmap regSlikaBmp = ImageUtil.matrixToBitmap(nSlika);
                regSlikaBmp.Save("slika" + uzorak + ".bmp");
            }
            MessageBox.Show("Obucavajuci skup formiran! ");
        }

        private void btnTraining_Click(object sender, EventArgs e)
        {
            if (bp != null)
            {
                brojUzoraka = obucavajuciSkup.GetLength(0) + bp.obucavajuciSkup.GetLength(0);
                double[, ,] novi = new double[brojUzoraka, 2, 64];
                for (int i = 0; i < brojUzoraka; i++)
                {
                    if (i < obucavajuciSkup.GetLength(0))
                    {
                        for (int j = 0; j < obucavajuciSkup.GetLength(1); j++)
                        {
                            for (int k = 0; k < obucavajuciSkup.GetLength(2); k++)
                            {
                                novi[i, j, k] = obucavajuciSkup[i, j, k];
                            }
                        }
                    }
                    else
                    {
                        for (int j = 0; j < bp.obucavajuciSkup.GetLength(1); j++)
                        {
                            for (int k = 0; k < bp.obucavajuciSkup.GetLength(2); k++)
                            {
                                novi[i, j, k] = bp.obucavajuciSkup[i - obucavajuciSkup.GetLength(0), j, k];
                            }
                        }
                    }
                }

                obucavajuciSkup = novi;
            }
            bp = new BackPropagation.BackPropagation(brojUzoraka, obucavajuciSkup);
            bp.obuci();

            Function f1 = new Function(Color.Red, bp.greske, Function.VBAR);
            List<Function> ff = new List<Function>();
            ff.Add(f1);
            FrmChart chart = new FrmChart(ff);
            chart.Show();
        }

        private double[] pripremiSlikuZaVNM(byte[,] slika)
        {
            for (int i = 0; i < slika.GetLength(0); i++)
            {
                for (int j = 0; j < slika.GetLength(1); j++)
                {
                    if (slika[i, j] < 250)
                        slika[i, j] = 0;
                    else
                        slika[i, j] = 255;
                }
            }

            double[] retVal = new double[64];

            for (int i = 0; i < slika.GetLength(0); i++)
            {
                for (int j = 0; j < slika.GetLength(1); j++)
                {
                    if (slika[i, j] != 255)
                    {
                        int ii = i / 8;
                        int jj = j / 8;

                        retVal[ii * 8 + jj]++;
                    }
                }
            }

            for (int i = 0; i < retVal.Length; i++)
            {
                retVal[i] = retVal[i] / 64;
            }

            return retVal;
        }

        private void btnRecognizeAll_Click(object sender, EventArgs e)
        {
            ClearImageEditor();

            Bitmap bmp = imageEditorDisplay1.mapa.bmp;

            byte[,] slika = ImageUtil.bitmapToByteMatrix(bmp);
            byte[,] bSlika = ImageUtil.matrixToBinaryTiles(slika, 15, 15);
            //imageEditorDisplay1.mapa.bmp = ImageUtil.matrixToBitmap(bSlika);
            List<RasterRegion> regions = ImageUtil.regionLabeling(bSlika);
            foreach (RasterRegion reg in regions)
            {
                reg.odrediMomente();
            }
            regions.Sort((a, b) =>
            {
                return a.minX.CompareTo(b.minX);
            });
            string rez = "";

            foreach (RasterRegion reg in regions)
            {
                reg.odrediMomente();
                byte[,] regSlika = reg.odrediNSliku();
                regSlika = ImageUtil.resizeImage(regSlika, new Size(64, 64));
                double[] ulaz = pripremiSlikuZaVNM(regSlika);
                int cifra = bp.izracunajCifru(ulaz);
                rez += cifra + ", ";

                List<Tacka> tacke = new List<Tacka>();
                tacke.Add(new Tacka(reg.minX - 1, reg.minY - 1));
                tacke.Add(new Tacka(reg.maxX + 1, reg.minY - 1));
                tacke.Add(new Tacka(reg.maxX + 1, reg.maxY + 1));
                tacke.Add(new Tacka(reg.minX - 1, reg.maxY + 1));
                Labela lbl = new Labela(reg.minX - 1, reg.minY - 1);
                lbl.boja = Color.Red;
                lbl.size = 10;
                lbl.labela = cifra.ToString();
                imageEditorDisplay1.selectedIndexer.add(lbl);
                Poligon rec = new Poligon(tacke, Color.FromArgb(10, Color.Green));

                imageEditorDisplay1.selectedIndexer.add(rec);
            }

            tbText.Text = rez;
        }

        private int compare(int a, int b)
        {
            if (a > b)
                return a;
            return b;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearImageEditor();
        }

        private void ClearImageEditor()
        {
            imageEditorDisplay1.mapa.poligoni.Clear();
            imageEditorDisplay1.mapa.tacke.Clear();
            imageEditorDisplay1.mapa.linije.Clear();
            imageEditorDisplay1.mapa.labele.Clear();
            imageEditorDisplay1.selectedIndexer.clear();
            imageEditorDisplay1.Refresh();
        }
#endregion

        #region Neuronska mreza za prepoznavanje boje

        BackPropagationImage bpi = null;    //neuronska mreza za odredjivanje boje

        private void setObucSkup(Bitmap bmp, int x, int y, bool isMarker, int brojac)
        {
            Color c = bmp.GetPixel(x, y);
            
            
            obucavajuciSkup[brojac, 0, 0] = c.R / 255.0;
            obucavajuciSkup[brojac, 0, 1] = c.G / 255.0;
            obucavajuciSkup[brojac, 0, 2] = c.B / 255.0;

            /*obucavajuciSkup[brojac, 0, 0] = c.GetHue() / 360.0;
            obucavajuciSkup[brojac, 0, 1] = c.GetSaturation();
            obucavajuciSkup[brojac, 0, 2] = c.GetBrightness();
            if ((brojac % 5) == 0)
            {
                int hu = (int)c.GetHue();
                int sa = (int)(c.GetSaturation() * 100);
                int ba = (int)(c.GetBrightness() * 100);
                Console.WriteLine(hu + "\t" + sa + "\t" + ba + "\t" + isMarker);
            }*/

            if (isMarker == true)
            {
                obucavajuciSkup[brojac, 1, 0] = 1;
                obucavajuciSkup[brojac, 1, 1] = 0;
            }
            else
            {
                obucavajuciSkup[brojac, 1, 0] = 0;
                obucavajuciSkup[brojac, 1, 1] = 1;
            }
        }

        private int brojTacakaMarkera = 2;
        private void btnTraningSetImage_Click(object sender, EventArgs e)
        {
            Bitmap bmp = imageEditorDisplay1.mapa.bmp;

            List<Tacka> tackeZaObuku = imageEditorDisplay1.mapa.tacke;

            brojUzoraka = tackeZaObuku.Count * 5;
            int brojUzorakaPetina = tackeZaObuku.Count;
            obucavajuciSkup = new double[brojUzoraka, 2, 10];

            bool isMarker = true;

            for (int i = 0; i < brojUzorakaPetina; i++)
            {
                int x = (int) tackeZaObuku[i].x;
                int y = (int)tackeZaObuku[i].y;

                if (i > brojTacakaMarkera)
                    isMarker = false;
                setObucSkup(bmp, x, y, isMarker, i * 5);
                setObucSkup(bmp, x - 2, y, isMarker, i * 5 + 1);
                setObucSkup(bmp, x + 2, y, isMarker, i * 5 + 2);
                setObucSkup(bmp, x, y - 2, isMarker, i * 5 + 3);
                setObucSkup(bmp, x, y + 2, isMarker, i * 5 + 4);
            }
            MessageBox.Show("Obucavajuci skup formiran! ");
        }

        private void btnTraningImage_Click(object sender, EventArgs e)
        {
            bpi = new BackPropagationImage(brojUzoraka, obucavajuciSkup);
            bpi.obuci();
            isMarkerRekognised = true;

            //OVDE BILA 
            startGame();

            Function f1 = new Function(Color.Red, bpi.greske, Function.VBAR);
            List<Function> ff = new List<Function>();
            ff.Add(f1);
            FrmChart chart = new FrmChart(ff);
            chart.Show();
        }


        private void btnImageColor_Click(object sender, EventArgs e)
        {
            ClearImageEditor();

            Bitmap bmp = imageEditorDisplay1.mapa.bmp;

            int w = bmp.Width;
            int h = bmp.Height;
            Bitmap result = new Bitmap(w,h);

            //za svaki piksel ucitane slike proveri da li je zelen ili nije i prikaze novu sliku (zuto-crna)
            for (int i = 0; i<w; i++)
                for (int j = 0; j < h; j++)
                {
					// TODO: kao public static unsafe byte[,] bitmapToByteMatrix(Bitmap src),public static unsafe Bitmap matrixToBitmap(byte[,] slika)
                    Color c = bmp.GetPixel(i, j);
                    double r = c.R / 255.0;
                    double g = c.G / 255.0;
                    double b = c.B / 255.0;

                    /*double hue = c.GetHue() / 360.0;
                    double sat = c.GetSaturation();
                    double val = c.GetBrightness();*/


                    double[] ulaz = { r, g, b };
                    //double[] ulaz = { hue, sat, val };

                    if (bpi.izracunajPiksel(ulaz) == 0)
                        result.SetPixel(i, j, Color.Black);
                    else
                        result.SetPixel(i, j, Color.Yellow);
                }

            imageEditorDisplay1.mapa.bmp = result;
            btnClear_Click(null, null);
            imageEditorDisplay1.FitImage();
            imageEditorDisplay1.Refresh();
        }

        #endregion

        #region Obicna Kamera

        private void btnObicnaKamera_Click(object sender, EventArgs e)
        {
            if (webcamstarted == false)
            {
                // list of video devices
                FilterInfoCollection videoDevices = new FilterInfoCollection(
                                        FilterCategory.VideoInputDevice);
                // create video source
                if (videoDevices.Count == 1)
                {
                    videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                }
                else
                {
                    videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                    //videoSource = new VideoCaptureDevice(videoDevices[videoDevices.Count - 1].MonikerString);
                }
                videoSource.NewFrame += new NewFrameEventHandler(video_NewFrameObicnaKAmera);
                // setup control and start playing
                //videoSource.DesiredFrameSize = new Size(1280, 720);
                videoSource.Start();
                btnObicnaKamera.Text = "Slika";
                webcamstarted = true;
            }
            else
            {
                this.videoSource.Stop();
                btnObicnaKamera.Text = "Obicna kamera";
                webcamstarted = false;
            }
        }

        private void video_NewFrameObicnaKAmera(object sender, NewFrameEventArgs eventArgs)
        {
            if (imageEditorDisplay1.InvokeRequired)
            {
                // get new frame
                Bitmap bitmap = eventArgs.Frame;
                //Console.WriteLine(bitmap.Size.Height + " " + bitmap.Size.Width);
                // process the frame

                byte[, ,] slika = ImageUtil.bitmapToColorMatrix(bitmap);
                imageEditorDisplay1.mapa.bmp = ImageUtil.colorMatrixToBitmap(slika);
                imageEditorDisplay1.FitImage();
                try
                {
                    imageEditorDisplay1.BeginInvoke(new MethodInvoker(() => imageEditorDisplay1.Refresh()));
                }
                catch (Exception)
                {
                }

            }
            else
            {
                videoSource.SignalToStop();
            }
        }
        #endregion

        #region Kamera za prepoznavanje
        private void btnKamera_Click(object sender, EventArgs e)
        {
            if (webcamstarted == false)
            {
                // list of video devices
                FilterInfoCollection videoDevices = new FilterInfoCollection(
                                        FilterCategory.VideoInputDevice);
                // create video source
                if (videoDevices.Count == 1)
                {
                    videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                }
                else
                {
                    videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                    //videoSource = new VideoCaptureDevice(videoDevices[videoDevices.Count - 1].MonikerString);
                }
                videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
                videoSource.NewFrame += new NewFrameEventHandler(video_NewFrameObicnaKAmera);
                // setup control and start playing
                //videoSource.DesiredFrameSize = new Size(1280, 720);
                videoSource.Start();
                btnKamera.Text = "Slika";
                webcamstarted = true;
            }
            else
            {
                this.videoSource.Stop();
                btnKamera.Text = "webcam";
                webcamstarted = false;
            }
        }

        private int brFrame = 2;
        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {

            if (imageEditorDisplay2.InvokeRequired)
            {
                if (brFrame == 2)
                {
                    brFrame = 0;
                    // get new frame
                    Bitmap bitmap = eventArgs.Frame;
                    // process the frame

                    byte[, ,] slika = ImageUtil.bitmapToColorMatrix(bitmap);
                    byte[,] obradjenaSlika = getBlackWhite(slika);
                    imageEditorDisplay2.mapa.bmp = ImageUtil.matrixToBitmap(obradjenaSlika);
                    imageEditorDisplay2.FitImage();

                    byte[,] dilatSlika = ImageUtil.dilation(obradjenaSlika);
                    byte[,] dilatSlika1 = ImageUtil.dilation(dilatSlika);
                    byte[,] dilatSlika2 = ImageUtil.dilation(dilatSlika1);
                    byte[,] dilatSlika3 = ImageUtil.dilation(dilatSlika2);
                    byte[,] dilatSlika4 = ImageUtil.dilation(dilatSlika3);
                    byte[,] dilatSlika5 = ImageUtil.dilation(dilatSlika4);

                    kandidati = ImageUtil.regionLabeling(dilatSlika5);
                    //Console.WriteLine("Broj regiona: " + kandidati.Count);

                    //najveci region
                    int num = 0;

                    RasterRegion bigest = new RasterRegion();
                    foreach (RasterRegion rg in kandidati)
                    {
                        if (num < rg.points.Count)
                        {
                            num = rg.points.Count;
                            bigest = rg;
                        }
                    }

                    if(frame == frames){
                        frame = 0;
                        if(bigest.points.Count > prag){
                            Point teziste = new Point(0, 0);
                            teziste = bigest.tezisteRegiona();


                            //index karte ako postoji (-1 ako ne)
                            int intex = cardsRegion.getIndexOfCard(teziste);

                            //ovo nece bas da radi jer ako ne postoji marker mis ne moze da se pomeri sa (0,0) pozicije i ne moze da se ugasi
                            int indX = intex % 6;
                            int indY = intex / 6;
                            int x = this.Location.X + 100 + 78 * indX;
                            int y = this.Location.Y + 150 + 81 * indY;

                            //Console.WriteLine("Pozition = " + indX + " " + indY);
                            Cursor.Position = new Point(x, y);
                            
                            //78 x 81 y
                            

                            //klik misa
                            SimulateMouseClick();
                            //TODO
                            //openIndex(intex, sender, evt);
                            
                        }
                    }

                    try
                    {
                        imageEditorDisplay2.BeginInvoke(new MethodInvoker(() => imageEditorDisplay2.Refresh()));
                    }
                    catch (Exception)
                    {
                    }
                }
                brFrame++;
                frame++;
            }
            else
            {
                videoSource.SignalToStop();
            }
        }
        #endregion

        #region Ostale funkcije

        private void startGame()
        {
            //dugmice dozvoli
            button1.Enabled = true;
            button2.Enabled = true;

            /*samo postavi druge boje*/
            for (int i = 0; i < 6;i++ )
            {
                for (int j = 0; j < 6; j++)
                {
                    mreza[i, j].BackColor = Color.DarkRed;
                }
            }

            A00.Click += new EventHandler(A00_Click);
            A00.MouseHover += new EventHandler(A00_MouseHover);
            A00.MouseLeave += new EventHandler(A00_MouseLeave);

            A01.Click += new EventHandler(A01_Click);
            A01.MouseHover += new EventHandler(A01_MouseHover);
            A01.MouseLeave += new EventHandler(A01_MouseLeave);

            A02.Click += new EventHandler(A02_Click);
            A02.MouseHover += new EventHandler(A02_MouseHover);
            A02.MouseLeave += new EventHandler(A02_MouseLeave);

            A03.Click += new EventHandler(A03_Click);
            A03.MouseHover += new EventHandler(A03_MouseHover);
            A03.MouseLeave += new EventHandler(A03_MouseLeave);

            A04.Click += new EventHandler(A04_Click);
            A04.MouseHover += new EventHandler(A04_MouseHover);
            A04.MouseLeave += new EventHandler(A03_MouseLeave);

            A05.Click += new EventHandler(A05_Click);
            A05.MouseHover += new EventHandler(A05_MouseHover);
            A05.MouseLeave += new EventHandler(A05_MouseLeave);

            B00.Click += new EventHandler(B00_Click);
            B00.MouseHover += new EventHandler(B00_MouseHover);
            B00.MouseLeave += new EventHandler(B00_MouseLeave);

            B01.Click += new EventHandler(B01_Click);
            B01.MouseHover += new EventHandler(B01_MouseHover);
            B01.MouseLeave += new EventHandler(B01_MouseLeave);

            B02.Click += new EventHandler(B02_Click);
            B02.MouseHover += new EventHandler(B02_MouseHover);
            B02.MouseLeave += new EventHandler(B02_MouseLeave);

            B03.Click += new EventHandler(B03_Click);
            B03.MouseHover += new EventHandler(B03_MouseHover);
            B03.MouseLeave += new EventHandler(B03_MouseLeave);

            B04.Click += new EventHandler(B04_Click);
            B04.MouseHover += new EventHandler(B04_MouseHover);
            B04.MouseLeave += new EventHandler(B04_MouseLeave);

            B05.Click += new EventHandler(B05_Click);
            B05.MouseHover += new EventHandler(B05_MouseHover);
            B05.MouseLeave += new EventHandler(B05_MouseLeave);

            C00.Click += new EventHandler(C00_Click);
            C00.MouseHover += new EventHandler(C00_MouseHover);
            C00.MouseLeave += new EventHandler(C00_MouseLeave);

            C01.Click += new EventHandler(C01_Click);
            C01.MouseHover += new EventHandler(C01_MouseHover);
            C01.MouseLeave += new EventHandler(C01_MouseLeave);

            C02.Click += new EventHandler(C02_Click);
            C02.MouseHover += new EventHandler(C02_MouseHover);
            C02.MouseLeave += new EventHandler(C02_MouseLeave);

            C03.Click += new EventHandler(C03_Click);
            C03.MouseHover += new EventHandler(C03_MouseHover);
            C03.MouseLeave += new EventHandler(C03_MouseLeave);

            C04.Click += new EventHandler(C04_Click);
            C04.MouseHover += new EventHandler(C04_MouseHover);
            C04.MouseLeave += new EventHandler(C04_MouseLeave);

            C05.Click += new EventHandler(C05_Click);
            C05.MouseHover += new EventHandler(C05_MouseHover);
            C05.MouseLeave += new EventHandler(C05_MouseLeave);

            D00.Click += new EventHandler(D00_Click);
            D00.MouseHover += new EventHandler(D00_MouseHover);
            D00.MouseLeave += new EventHandler(D00_MouseLeave);

            D01.Click += new EventHandler(D01_Click);
            D01.MouseHover += new EventHandler(D01_MouseHover);
            D01.MouseLeave += new EventHandler(D01_MouseLeave);

            D02.Click += new EventHandler(D02_Click);
            D02.MouseHover += new EventHandler(D02_MouseHover);
            D02.MouseLeave += new EventHandler(D02_MouseLeave);

            D03.Click += new EventHandler(D03_Click);
            D03.MouseHover += new EventHandler(D03_MouseHover);
            D03.MouseLeave += new EventHandler(D03_MouseLeave);

            D04.Click += new EventHandler(D04_Click);
            D04.MouseHover += new EventHandler(D04_MouseHover);
            D04.MouseLeave += new EventHandler(D04_MouseLeave);

            D05.Click += new EventHandler(D05_Click);
            D05.MouseHover += new EventHandler(D05_MouseHover);
            D05.MouseLeave += new EventHandler(D05_MouseLeave);

            E00.Click += new EventHandler(E00_Click);
            E00.MouseHover += new EventHandler(E00_MouseHover);
            E00.MouseLeave += new EventHandler(E00_MouseLeave);

            E01.Click += new EventHandler(E01_Click);
            E01.MouseHover += new EventHandler(E01_MouseHover);
            E01.MouseLeave += new EventHandler(E01_MouseLeave);

            E02.Click += new EventHandler(E02_Click);
            E02.MouseHover += new EventHandler(E02_MouseHover);
            E02.MouseLeave += new EventHandler(E02_MouseLeave);

            E03.Click += new EventHandler(E03_Click);
            E03.MouseHover += new EventHandler(E03_MouseHover);
            E03.MouseLeave += new EventHandler(E03_MouseLeave);

            E04.Click += new EventHandler(E04_Click);
            E04.MouseHover += new EventHandler(E04_MouseHover);
            E04.MouseLeave += new EventHandler(E04_MouseLeave);

            E05.Click += new EventHandler(E05_Click);
            E05.MouseHover += new EventHandler(E05_MouseHover);
            E05.MouseLeave += new EventHandler(E05_MouseLeave);

            F00.Click += new EventHandler(F00_Click);
            F00.MouseHover += new EventHandler(F00_MouseHover);
            F00.MouseLeave += new EventHandler(F00_MouseLeave);

            F01.Click += new EventHandler(F01_Click);
            F01.MouseHover += new EventHandler(F01_MouseHover);
            F01.MouseLeave += new EventHandler(F01_MouseLeave);

            F02.Click += new EventHandler(F02_Click);
            F02.MouseHover += new EventHandler(F02_MouseHover);
            F02.MouseLeave += new EventHandler(F02_MouseLeave);

            F03.Click += new EventHandler(F03_Click);
            F03.MouseHover += new EventHandler(F03_MouseHover);
            F03.MouseLeave += new EventHandler(F03_MouseLeave);

            F04.Click += new EventHandler(F04_Click);
            F04.MouseHover += new EventHandler(F04_MouseHover);
            F04.MouseLeave += new EventHandler(F04_MouseLeave);

            F05.Click += new EventHandler(F05_Click);
            F05.MouseHover += new EventHandler(F05_MouseHover);
            F05.MouseLeave += new EventHandler(F05_MouseLeave);
        }

        private byte[,] getBlackWhite(byte[, ,] original)
        {

            
            int w = original.GetLength(0);
            int h = original.GetLength(1);
            byte[,] retVal = new byte[w, h];

            for (int i = 0; i < w; i++)
                for (int j = 0; j < h; j++)
                {
                    double r = original[i, j, 0] / 255.0;
                    double g = original[i, j, 1] / 255.0;
                    double b = original[i, j, 2] / 255.0;
                    double[] ulaz = { r, g, b };

                    if (bpi.izracunajPiksel(ulaz) == 0)
                    {
                        retVal[i, j] = 0;
                    }
                    else
                    {
                        retVal[i, j] = 255;
                    }
                }
            return retVal;

        }

        private byte[,] getBlackWhiteTable(byte[, ,] original)
        {


            int w = original.GetLength(0);
            int h = original.GetLength(1);
            byte[,] retVal = new byte[w, h];

            for (int i = 0; i < w; i++)
                for (int j = 0; j < h; j++)
                {
                    double r = original[i, j, 0] / 255.0;
                    double g = original[i, j, 1] / 255.0;
                    double b = original[i, j, 2] / 255.0;
                    double[] ulaz = { r, g, b };

                    if (bpTable.izracunajPiksel(ulaz) == 0)
                    {
                        retVal[i, j] = 0;
                    }
                    else
                    {
                        retVal[i, j] = 255;
                    }
                }
            return retVal;

        }


        private byte[, ,] addFrameAround(byte[, ,] original)
        {
            int w = original.GetLength(0) / 3;
            int h = original.GetLength(1) / 3;
            byte[, ,] retVal = original;
            for (int i = 0; i<w; i++)
                for (int j = 0; j < 3; j++)
                {
                    retVal[w + i, h + j, 0] = 255;
                    retVal[w + i, h + j, 1] = 0;
                    retVal[w + i, h + j, 2] = 0;
                }
            return retVal;
        }
        #endregion

        #region Metode za igru

        public void endtest()
        {
            testg = 0;//prilikom svakog poziva ponisti radi ponovnog ispitivanja
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    if (mreza[i, j].Visible == true)
                    {
                        testg++;//proverava dal je kraj igre
                        //ako nije povecace promenljivu
                    }
                }
            }
        }

        public void eraseMreza()
        {
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    tabla.setZauzeto(i, j, true);
                    mreza[i, j].Visible = true;
                    mreza[i, j].Enabled = true;
                }
            }
        }

        public void Game_load()
        {
            tabla.BROJ_SLIKA = 11;
            StartPosition = FormStartPosition.CenterScreen;
            int k = 0;

            tabla.Popuni();//pri pokretanju je nova igra samim tim je poopnjena tabla

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    niz[k] = tabla.retIndex(i, j);
                    k++;
                }
            }
            for (int z = 0; z < 36; z++)
            {
                nizL[z].ImageIndex = niz[z];
            }
        }

        public void NovaIgra()
        {
            prvi_put = 0;//da ponistim prvi put se ne gube poeni
            eraseMreza();
            tabla.obrisi();
            timeo = 0;
            cntrl = 0;
            testg = 0;
            fi = 0;
            fj = 0;
            si = 0;
            sj = 0;
            open = 0;
            see = false;
            see2 = false;
            image1.erase();
            image2.erase();
            int k = 0;
            
            tabla.Popuni();//pri pokretanju je nova igra samim tim je poopnjena tabla
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    niz[k] = tabla.retIndex(i, j);
                    k++;
                }
            }
            for (int z = 0; z < 36; z++)
            {
                nizL[z].ImageIndex = niz[z];
            }
            
        }

        public void test()
        {
            if (open == 2)
            {
                throw new MyException();
            }
        }

        public void zatvoriBezpotrebne()
        {
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    if (tabla.getZauzeto(i, j) == true)
                    {
                        mreza[i, j].Visible = false;//i da se labela ne vidi,tj ne pokriva sliku!
                        //pogodjena je...
                    }
                    else
                    {
                        mreza[i, j].Visible = true;//i da se labela vidi,tj pokriva sliku!
                        //nije pogodjena
                    }
                }
            }
        }

        public void testopen()
        {
            if (timeo == 3)
            {
                throw new MyException();//baci izuzetak
            }
        }

        public void Okje()
        {
            /*Za sada debilno*/
            MessageBox.Show("Cestitke kraj igre :-D");
        }

        public void registrujPoene()
        {
            /*Registruj poene ako je potrebno*/
        }

        public void solve()
        {
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    tabla.setZauzeto(i, j, true);
                    mreza[i, j].Visible = false;
                    mreza[i, j].Enabled = false;
                }
            }
        }

        public void Brain(Label Benith, Label Under)
        {
                timeo++;//svaki put kada se ovo pozive otvorena je karta
                //ovde da skida poene prilikom svakog otvaranja sto bas i nema smisla
                try
                {
                    testopen();//gleda dal je otvorena treca,tj da zatvori one koje nisu ok!
                }
                catch (MyException menew)
                {
                    see2 = menew.opentest(timeo);//vratim drugi,tj dal je otvorena treca karta
                    //da bih mogao da zatvorim ostale
                    if (see2 == true)
                    {//ako je otvorena treca karta
                        zatvoriBezpotrebne();//zatvori bespotrebne,ovde ce zatvoriti
                        //sve sa false vrednostima
                        timeo = 1;//postavim timeo na 1 jer je nesto otvoreno i ocekuje se sledece otvaranje
                    }
                }

                if (image1.STATE == false)
                {
                    //Benith.BeginInvoke(new MethodInvoker(() => Benith.Visible = false));
                    Benith.Visible = false;//prikazem je
                    tabla.setZauzeto(fi, fj, true);//privremeno je zauzeta zbog ispitivanja
                    image1.STATE = true;//ako ova nije postavljena postavim je
                    image1.NUM = Under.ImageIndex;//postavim koja je slika
                    open = 1;//otvorena je jedna,znaci mora jos jedna
                }
                else
                {
                    //posto sam obe otvorio poeni pocinju da spadaju
                    //BeginInvoke(new MethodInvoker(() => imageEditorDisplay1.Refresh()));
                    //Benith.BeginInvoke(new MethodInvoker(() => Benith.Visible = false));
                    Benith.Visible = false;
                    tabla.setZauzeto(si, sj, true);
                    image2.STATE = true;
                    image2.NUM = Under.ImageIndex;
                    open = 2;//otvorena je i druga sada moze da se ispituje
                    //sada ispitam te otvorene
                    prvi_put++;
                    try
                    {
                        test();
                    }
                    catch (MyException me)
                    {
                        see = me.test(image1, image2);//vratim prvi rezultat
                        if (see == false)
                        {
                            //ako nisu jednaki postavi ih na false
                            tabla.setZauzeto(fi, fj, false);
                            tabla.setZauzeto(si, sj, false);
                            registrujPoene();//ako nisu dobre gudimo poene
                            //u protivnom ih ostavi na true
                        }
                        else
                        {//ako su jednaki vidi dal su svi odkriveni,ako jesu to je kraj
                            endtest();//proverava dal je kraj igre i signalizira
                            if (testg == 0)
                            {
                                Okje();//U redu sve je otvoreno,ajd sada to da signaliziramo popalimo sta treba
                            }

                        }

                        image1.erase();
                        image2.erase();
                    }

                }
        }

        #endregion Metode za igru

        #region Dogadjaji za igru

        private void A00_Click(object sender, EventArgs e)
        {
            const int i = 0;
            const int j = 0;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            BeginInvoke(new MethodInvoker(() => Brain(A00, A0)));
        }
        private void A01_Click(object sender, EventArgs e)
        {

            const int i = 0;
            const int j = 1;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(A01, A1);
            BeginInvoke(new MethodInvoker(() => Brain(A01, A1)));
        }
        private void A02_Click(object sender, EventArgs e)
        {

            const int i = 0;
            const int j = 2;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(A02, A2);
            BeginInvoke(new MethodInvoker(() => Brain(A02, A2)));
        }
        private void A03_Click(object sender, EventArgs e)
        {
            int i = 0;
            int j = 3;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(A03, A3);
            BeginInvoke(new MethodInvoker(() => Brain(A03, A3)));
        }
        private void A04_Click(object sender, EventArgs e)
        {
            int i = 0;
            int j = 4;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(A04, A4);
            BeginInvoke(new MethodInvoker(() => Brain(A04, A4)));
        }
        private void A05_Click(object sender, EventArgs e)
        {
            int i = 0;
            int j = 5;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(A05, A5);
            BeginInvoke(new MethodInvoker(() => Brain(A05, A5)));
        }
        private void B00_Click(object sender, EventArgs e)
        {
            int i = 1;
            int j = 0;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(B00, B0);
            BeginInvoke(new MethodInvoker(() => Brain(B00, B0)));
        }
        private void B01_Click(object sender, EventArgs e)
        {
            int i = 1;
            int j = 1;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(B01, B1);
            BeginInvoke(new MethodInvoker(() => Brain(B01, B1)));
        }
        private void B02_Click(object sender, EventArgs e)
        {
            int i = 1;
            int j = 2;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(B02, B2);
            BeginInvoke(new MethodInvoker(() => Brain(B02, B2)));
        }
        private void B03_Click(object sender, EventArgs e)
        {
            int i = 1;
            int j = 3;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(B03, B3);
            BeginInvoke(new MethodInvoker(() => Brain(B03, B3)));
        }
        private void B04_Click(object sender, EventArgs e)
        {
            int i = 1;
            int j = 4;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(B04, B4);
            BeginInvoke(new MethodInvoker(() => Brain(B04, B4)));
        }
        private void B05_Click(object sender, EventArgs e)
        {
            int i = 1;
            int j = 5;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(B05, B5);
            BeginInvoke(new MethodInvoker(() => Brain(B05, B5)));
        }
        private void C00_Click(object sender, EventArgs e)
        {
            int i = 2;
            int j = 0;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(C00, C0);
            BeginInvoke(new MethodInvoker(() => Brain(C00, C0)));
        }
        private void C01_Click(object sender, EventArgs e)
        {
            int i = 2;
            int j = 1;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(C01, C1);
            BeginInvoke(new MethodInvoker(() => Brain(C01, C1)));
        }
        private void C02_Click(object sender, EventArgs e)
        {
            int i = 2;
            int j = 2;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(C02, C2);
            BeginInvoke(new MethodInvoker(() => Brain(C02, C2)));
        }
        private void C03_Click(object sender, EventArgs e)
        {
            int i = 2;
            int j = 3;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(C03, C3);
            BeginInvoke(new MethodInvoker(() => Brain(C03, C3)));
        }
        private void C04_Click(object sender, EventArgs e)
        {
            int i = 2;
            int j = 4;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(C04, C4);
            BeginInvoke(new MethodInvoker(() => Brain(C04, C4)));
        }
        private void C05_Click(object sender, EventArgs e)
        {
            int i = 2;
            int j = 5;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(C05, C5);
            BeginInvoke(new MethodInvoker(() => Brain(C05, C5)));
        }
        private void D00_Click(object sender, EventArgs e)
        {
            int i = 3;
            int j = 0;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(D00, D0);
            BeginInvoke(new MethodInvoker(() => Brain(D00, D0)));
        }
        private void D01_Click(object sender, EventArgs e)
        {
            int i = 3;
            int j = 1;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(D01, D1);
            BeginInvoke(new MethodInvoker(() => Brain(D01, D1)));
        }
        private void D02_Click(object sender, EventArgs e)
        {
            int i = 3;
            int j = 2;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(D02, D2);
            BeginInvoke(new MethodInvoker(() => Brain(D02, D2)));
        }
        private void D03_Click(object sender, EventArgs e)
        {
            int i = 3;
            int j = 3;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(D03, D3);
            BeginInvoke(new MethodInvoker(() => Brain(D03, D3)));
        }
        private void D04_Click(object sender, EventArgs e)
        {
            int i = 3;
            int j = 4;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(D04, D4);
            BeginInvoke(new MethodInvoker(() => Brain(D04, D4)));
        }
        private void D05_Click(object sender, EventArgs e)
        {
            int i = 3;
            int j = 5;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(D05, D5);
            BeginInvoke(new MethodInvoker(() => Brain(D05, D5)));
        }
        private void E00_Click(object sender, EventArgs e)
        {
            int i = 4;
            int j = 0;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(E00, E0);
            BeginInvoke(new MethodInvoker(() => Brain(E00, E0)));
        }
        private void E01_Click(object sender, EventArgs e)
        {
            int i = 4;
            int j = 1;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(E01, E1);
            BeginInvoke(new MethodInvoker(() => Brain(E01, E1)));
        }
        private void E02_Click(object sender, EventArgs e)
        {
            int i = 4;
            int j = 2;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(E02, E2);
            BeginInvoke(new MethodInvoker(() => Brain(E02, E2)));
        }
        private void E03_Click(object sender, EventArgs e)
        {
            int i = 4;
            int j = 3;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(E03, E3);
            BeginInvoke(new MethodInvoker(() => Brain(E03, E3)));
        }
        private void E04_Click(object sender, EventArgs e)
        {
            int i = 4;
            int j = 4;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(E04, E4);
            BeginInvoke(new MethodInvoker(() => Brain(E04, E4)));
        }
        private void E05_Click(object sender, EventArgs e)
        {
            int i = 4;
            int j = 5;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(E05, E5);
            BeginInvoke(new MethodInvoker(() => Brain(E05, E5)));
        }
        private void F00_Click(object sender, EventArgs e)
        {
            int i = 5;
            int j = 0;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(F00, F0);
            BeginInvoke(new MethodInvoker(() => Brain(F00, F0)));
        }
        private void F01_Click(object sender, EventArgs e)
        {
            int i = 5;
            int j = 1;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(F01, F1);
            BeginInvoke(new MethodInvoker(() => Brain(F01, F1)));
        }
        private void F02_Click(object sender, EventArgs e)
        {
            int i = 5;
            int j = 2;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(F02, F2);
            BeginInvoke(new MethodInvoker(() => Brain(F02, F2)));
        }
        private void F03_Click(object sender, EventArgs e)
        {
            int i = 5;
            int j = 3;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(F03, F3);
            BeginInvoke(new MethodInvoker(() => Brain(F03, F3)));
        }
        private void F04_Click(object sender, EventArgs e)
        {
            int i = 5;
            int j = 4;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(F04, F4);
            BeginInvoke(new MethodInvoker(() => Brain(F04, F4)));
        }
        private void F05_Click(object sender, EventArgs e)
        {
            int i = 5;
            int j = 5;
            if (image1.STATE == false)
            {
                fi = i;
                fj = j;
            }
            else
            {
                si = i;
                sj = j;
            }
            //Brain(F05, F5);
            BeginInvoke(new MethodInvoker(() => Brain(F05, F5)));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void A01_MouseHover(object sender, EventArgs e)
        {
            A01.BackColor = Color.DarkRed;
            //openTimer.Start();
        }
        private void A00_MouseHover(object sender, EventArgs e)
        {
            A00.BackColor = Color.DarkRed;
            //openTimer.Start();
        }
        private void A00_MouseLeave(object sender, EventArgs e)
        {
            A00.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }
        private void A01_MouseLeave(object sender, EventArgs e)
        {
            A01.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }

        private void A02_MouseHover(object sender, EventArgs e)
        {
            A02.BackColor = Color.DarkRed;
            //openTimer.Start();
        }

        private void A02_MouseLeave(object sender, EventArgs e)
        {
            A02.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }

        private void A03_MouseHover(object sender, EventArgs e)
        {
            A03.BackColor = Color.DarkRed;
            //openTimer.Start();
        }

        private void A03_MouseLeave(object sender, EventArgs e)
        {
            A04.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }
        private void A04_MouseHover(object sender, EventArgs e)
        {
            A04.BackColor = Color.DarkRed;
            //openTimer.Start();
        }

        private void A04_MouseLeave(object sender, EventArgs e)
        {
            A04.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }
        private void A05_MouseHover(object sender, EventArgs e)
        {
            A05.BackColor = Color.DarkRed;
            //openTimer.Start();
        }

        private void A05_MouseLeave(object sender, EventArgs e)
        {
            A05.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }

        private void B00_MouseHover(object sender, EventArgs e)
        {
            B00.BackColor = Color.DarkRed;
            //openTimer.Start();
        }

        private void B00_MouseLeave(object sender, EventArgs e)
        {
            B00.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }

        private void B01_MouseHover(object sender, EventArgs e)
        {
            B01.BackColor = Color.DarkRed;
            //openTimer.Start();
        }

        private void B01_MouseLeave(object sender, EventArgs e)
        {
            B01.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }

        private void B02_MouseHover(object sender, EventArgs e)
        {
            B02.BackColor = Color.DarkRed;
            //openTimer.Start();
        }

        private void B02_MouseLeave(object sender, EventArgs e)
        {
            B02.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }

        private void B03_MouseHover(object sender, EventArgs e)
        {
            B03.BackColor = Color.DarkRed;
            //openTimer.Start();
        }

        private void B03_MouseLeave(object sender, EventArgs e)
        {
            B03.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }

        private void B04_MouseHover(object sender, EventArgs e)
        {
            B04.BackColor = Color.DarkRed;
            //openTimer.Start();
        }

        private void B04_MouseLeave(object sender, EventArgs e)
        {
            B04.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }

        private void B05_MouseHover(object sender, EventArgs e)
        {
            B05.BackColor = Color.DarkRed;
            //openTimer.Start();
        }

        private void B05_MouseLeave(object sender, EventArgs e)
        {
            B05.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }

        private void C00_MouseHover(object sender, EventArgs e)
        {
            C00.BackColor = Color.DarkRed;
            //openTimer.Start();
        }

        private void C00_MouseLeave(object sender, EventArgs e)
        {
            C00.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }

        private void C01_MouseHover(object sender, EventArgs e)
        {
            C01.BackColor = Color.DarkRed;
            //openTimer.Start();
        }

        private void C01_MouseLeave(object sender, EventArgs e)
        {
            C01.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }

        private void C02_MouseHover(object sender, EventArgs e)
        {
            C02.BackColor = Color.DarkRed;
            //openTimer.Start();
        }

        private void C02_MouseLeave(object sender, EventArgs e)
        {
            C02.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }

        private void C03_MouseHover(object sender, EventArgs e)
        {
            C03.BackColor = Color.DarkRed;
            //openTimer.Start();
        }

        private void C03_MouseLeave(object sender, EventArgs e)
        {
            C03.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }

        private void C04_MouseHover(object sender, EventArgs e)
        {
            C04.BackColor = Color.DarkRed;
            //openTimer.Start();
        }

        private void C04_MouseLeave(object sender, EventArgs e)
        {
            C04.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }

        private void C05_MouseHover(object sender, EventArgs e)
        {
            C05.BackColor = Color.DarkRed;
            //openTimer.Start();
        }

        private void C05_MouseLeave(object sender, EventArgs e)
        {
            C05.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }

        private void D00_MouseHover(object sender, EventArgs e)
        {
            D00.BackColor = Color.DarkRed;
            //openTimer.Start();
        }

        private void D00_MouseLeave(object sender, EventArgs e)
        {
            D00.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }

        private void D01_MouseHover(object sender, EventArgs e)
        {
            D01.BackColor = Color.DarkRed;
            //openTimer.Start();
        }

        private void D01_MouseLeave(object sender, EventArgs e)
        {
            D01.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }

        private void D02_MouseHover(object sender, EventArgs e)
        {
            D02.BackColor = Color.DarkRed;
            //openTimer.Start();
        }

        private void D02_MouseLeave(object sender, EventArgs e)
        {
            D02.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }

        private void D03_MouseHover(object sender, EventArgs e)
        {
            D03.BackColor = Color.DarkRed;
            //openTimer.Start();
        }

        private void D03_MouseLeave(object sender, EventArgs e)
        {
            D03.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }

        private void D04_MouseHover(object sender, EventArgs e)
        {
            D04.BackColor = Color.DarkRed;
            //openTimer.Start();
        }

        private void D04_MouseLeave(object sender, EventArgs e)
        {
            D04.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }

        private void D05_MouseHover(object sender, EventArgs e)
        {
            D05.BackColor = Color.DarkRed;
            //openTimer.Start();
        }

        private void D05_MouseLeave(object sender, EventArgs e)
        {
            D05.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }

        private void E00_MouseHover(object sender, EventArgs e)
        {
            E00.BackColor = Color.DarkRed;
            //openTimer.Start();
        }

        private void E00_MouseLeave(object sender, EventArgs e)
        {
            E00.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }

        private void E01_MouseHover(object sender, EventArgs e)
        {
            E01.BackColor = Color.DarkRed;
            //openTimer.Start();
        }

        private void E01_MouseLeave(object sender, EventArgs e)
        {
            E01.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }

        private void E02_MouseHover(object sender, EventArgs e)
        {
            E02.BackColor = Color.DarkRed;
            //openTimer.Start();
        }

        private void E02_MouseLeave(object sender, EventArgs e)
        {
            E02.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }

        private void E03_MouseHover(object sender, EventArgs e)
        {
            E03.BackColor = Color.DarkRed;
            ////openTimer.Start();
        }

        private void E03_MouseLeave(object sender, EventArgs e)
        {
            E03.BackColor = Color.Lavender;
            ////openTimer.Stop();
            brojac = 0;
        }

        private void E04_MouseHover(object sender, EventArgs e)
        {
            E04.BackColor = Color.DarkRed;
            //openTimer.Start();
        }

        private void E04_MouseLeave(object sender, EventArgs e)
        {
            E04.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }

        private void E05_MouseHover(object sender, EventArgs e)
        {
            E05.BackColor = Color.DarkRed;
            //openTimer.Start();
        }

        private void E05_MouseLeave(object sender, EventArgs e)
        {
            E05.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }

        private void F00_MouseHover(object sender, EventArgs e)
        {
            F00.BackColor = Color.DarkRed;
            //openTimer.Start();
        }

        private void F00_MouseLeave(object sender, EventArgs e)
        {
            F00.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }

        private void F01_MouseHover(object sender, EventArgs e)
        {
            F01.BackColor = Color.DarkRed;
            //openTimer.Start();
        }

        private void F01_MouseLeave(object sender, EventArgs e)
        {
            F01.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }

        private void F02_MouseHover(object sender, EventArgs e)
        {
            F02.BackColor = Color.DarkRed;
            //openTimer.Start();
        }

        private void F02_MouseLeave(object sender, EventArgs e)
        {
            F02.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }

        private void F03_MouseHover(object sender, EventArgs e)
        {
            F03.BackColor = Color.DarkRed;
            //openTimer.Start();
        }

        private void F03_MouseLeave(object sender, EventArgs e)
        {
            F03.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }

        private void F04_MouseHover(object sender, EventArgs e)
        {
            F04.BackColor = Color.DarkRed;
            //openTimer.Start();
        }

        private void F04_MouseLeave(object sender, EventArgs e)
        {
            F04.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }

        private void F05_MouseHover(object sender, EventArgs e)
        {
            F05.BackColor = Color.DarkRed;
            //openTimer.Start();
        }

        private void F05_MouseLeave(object sender, EventArgs e)
        {
            F05.BackColor = Color.Lavender;
            //openTimer.Stop();
            brojac = 0;
        }
        
        #endregion Dogadjaji za igru

        #region Ostali dogadjaji

        private void imageEditorDisplay1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            NovaIgra();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            solve();
        }

        #endregion Ostali dogodjaji

        #region Auto Traning Event

        private static int autoFrameCounter = 32;
        private int autoTraningFrameCounter = autoFrameCounter;
        private int secondsCounter = 3;
        private bool takeAPicture = false;

        private void btnAutoTraing_Click(object sender, EventArgs e)
        {
            /*if (webcamstarted == false)
            {
                #region podesavanje pravougaonika
                List<Tacka> tacke = new List<Tacka>();

                int h = 288;
                int w = 352;

                tacke.Add(new Tacka(w * 0.4, h * 0.4));
                tacke.Add(new Tacka(w * 0.6, h * 0.4));
                tacke.Add(new Tacka(w * 0.6, h * 0.6));
                tacke.Add(new Tacka(w * 0.4, h * 0.6));
                Poligon p = new Poligon();
                p.tacke = tacke;

                imageEditorDisplay1.mapa.poligoni.Add(p);
                imageEditorDisplay1.selectedIndexer.add(p);
                #endregion

                // list of video devices
                FilterInfoCollection videoDevices = new FilterInfoCollection(
                                        FilterCategory.VideoInputDevice);
                // create video source
                if (videoDevices.Count == 1)
                {
                    videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                }
                else
                {
                    videoSource = new VideoCaptureDevice(videoDevices[videoDevices.Count - 1].MonikerString);
                }
                videoSource.NewFrame += new NewFrameEventHandler(video_NewFrameAutoTraning);
                // setup control and start playing
                //videoSource.DesiredFrameSize = new Size(1280, 720);
                videoSource.Start();
                //btnAutoTraing.Text = "Slika";
                webcamstarted = true;
                //autoTraningFrameCounter = autoFrameCounter;
                timerCam.Start();
            }*/

            /*else
            {
                this.videoSource.Stop();
                //btnAutoTraing.Text = "Obicna kamera";
                webcamstarted = false;
            }*/
        }

        
        private void video_NewFrameAutoTraning(object sender, NewFrameEventArgs eventArgs)
        {
            if (imageEditorDisplay1.InvokeRequired)
            {
                if (takeAPicture == false)
                {
                    // get new frame
                    Bitmap bitmap = eventArgs.Frame;
                    //Console.WriteLine(bitmap.Size.Height + " " + bitmap.Size.Width);
                    // process the frame

                    byte[, ,] slika = ImageUtil.bitmapToColorMatrix(bitmap);
                    imageEditorDisplay1.mapa.bmp = ImageUtil.colorMatrixToBitmap(slika);
                    imageEditorDisplay1.FitImage();
                    try
                    {
                        imageEditorDisplay1.BeginInvoke(new MethodInvoker(() => imageEditorDisplay1.Refresh()));
                    }
                    catch (Exception)
                    {
                    }

                    imageEditorDisplay1.mapa.poligoni.Clear();
                    imageEditorDisplay1.selectedIndexer.clear();   
                }
                else
                {
                    takeAPicture = false;
                    this.videoSource.Stop();
                    webcamstarted = false;
                }

            }
            else
            {
                videoSource.SignalToStop();
            }
        }

        
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (secondsCounter == 0)
            {
                label1.Text = "Time: " + secondsCounter;
                timerCam.Stop();
                secondsCounter = 3;
                takeAPicture = true;
            }
            else
            {
                label1.Text = "Time: " + secondsCounter;
                --secondsCounter;
            }
        }

        #endregion

        #region Neuronska mreza za prepoznavanje boje karata

        BackPropagationImage bpTable = null;    //neuronska mreza za odredjivanje boje

        private void btnTrananigSetKarte_Click_1(object sender, EventArgs e)
        {
            

            Bitmap bmp = imageEditorDisplay1.mapa.bmp;

            List<Tacka> tackeZaObuku = imageEditorDisplay1.mapa.tacke;

            brojUzoraka = tackeZaObuku.Count * 5;
            int brojUzorakaPetina = tackeZaObuku.Count;
            obucavajuciSkup = new double[brojUzoraka, 2, 10];

            bool isMarker = true;

            for (int i = 0; i < brojUzorakaPetina; i++)
            {
                int x = (int)tackeZaObuku[i].x;
                int y = (int)tackeZaObuku[i].y;

                if (i > 3)
                    isMarker = false;
                setObucSkupKarte(bmp, x, y, isMarker, i * 5);
                setObucSkupKarte(bmp, x - 2, y, isMarker, i * 5 + 1);
                setObucSkupKarte(bmp, x + 2, y, isMarker, i * 5 + 2);
                setObucSkupKarte(bmp, x, y - 2, isMarker, i * 5 + 3);
                setObucSkupKarte(bmp, x, y + 2, isMarker, i * 5 + 4);
            }
            MessageBox.Show("Obucavajuci skup formiran! ");
        }

        private void setObucSkupKarte(Bitmap bmp, int x, int y, bool isMarker, int brojac)
        {
            Color c = bmp.GetPixel(x, y);
            obucavajuciSkup[brojac, 0, 0] = c.R / 255.0;
            obucavajuciSkup[brojac, 0, 1] = c.G / 255.0;
            obucavajuciSkup[brojac, 0, 2] = c.B / 255.0;
            if (isMarker == true)
            {
                obucavajuciSkup[brojac, 1, 0] = 1;
                obucavajuciSkup[brojac, 1, 1] = 0;
            }
            else
            {
                obucavajuciSkup[brojac, 1, 0] = 0;
                obucavajuciSkup[brojac, 1, 1] = 1;
            }
        }
        

        private void btnTranaingTable_Click(object sender, EventArgs e)
        {
            bpTable = new BackPropagationImage(brojUzoraka, obucavajuciSkup);
            bpTable.obuci();
            isTableRekognised = true;

            Function f1 = new Function(Color.Red, bpTable.greske, Function.VBAR);
            List<Function> ff = new List<Function>();
            ff.Add(f1);
            FrmChart chart = new FrmChart(ff);
            chart.Show();
        }

        private void btnImageColor2_Click(object sender, EventArgs e)
        {
            ClearImageEditor();

            Bitmap bmp = imageEditorDisplay1.mapa.bmp;


            byte[, ,] slika = ImageUtil.bitmapToColorMatrix(bmp);
            byte[,] obradjenaSlika = getBlackWhiteTable(slika);

            kandidati = ImageUtil.regionLabeling(obradjenaSlika);
            Console.WriteLine("Broj regiona: " + kandidati.Count);
            if (kandidati.Count == 36)
            {
                cardsRegion.Regions = kandidati;
            }
            else
            {
                kandidati.RemoveAll(el => el.points.Count < 300);
                cardsRegion.Regions = kandidati;
                Console.WriteLine("Broj regiona: " + kandidati.Count);
                Console.WriteLine("EROORRRRR");
            }

            

            int w = bmp.Width;
            int h = bmp.Height;
            Bitmap result = new Bitmap(w, h);

            //za svaki piksel ucitane slike proveri da li je zelen ili nije i prikaze novu sliku (zuto-crna)
            for (int i = 0; i < w; i++)
                for (int j = 0; j < h; j++)
                {
                    // TODO: kao public static unsafe byte[,] bitmapToByteMatrix(Bitmap src),public static unsafe Bitmap matrixToBitmap(byte[,] slika)
                    Color c = bmp.GetPixel(i, j);
                    double r = c.R / 255.0;
                    double g = c.G / 255.0;
                    double b = c.B / 255.0;
                    //double a = c.A / 255.0;
                    double[] ulaz = { r, g, b };
                    if (bpTable.izracunajPiksel(ulaz) == 0)
                        result.SetPixel(i, j, Color.Black);
                    else
                        result.SetPixel(i, j, Color.Yellow);
                }

            imageEditorDisplay1.mapa.bmp = result;
            btnClear_Click(null, null);
            imageEditorDisplay1.FitImage();
            imageEditorDisplay1.Refresh();
        }

        #endregion

        #region Dogadjaj za otvaranje karte

        private void openIndex(int index,object sender,EventArgs e)
        {
            switch (index)
            {
                case 0:
                    A00_Click(sender, e);
                    break;
                case 1:
                    A01_Click(sender, e);
                    break;
                case 2:
                    A02_Click(sender, e);
                    break;
                case 3:
                    A03_Click(sender, e);
                    break;
                case 4:
                    A04_Click(sender, e);
                    break;
                case 5:
                    A05_Click(sender, e);
                    break;
                case 6:
                    B00_Click(sender, e);
                    break;
                case 7:
                    B01_Click(sender, e);
                    break;
                case 8:
                    B02_Click(sender, e);
                    break;
                case 9:
                    B03_Click(sender, e);
                    break;
                case 10:
                    B04_Click(sender, e);
                    break;
                case 11:
                    B05_Click(sender, e);
                    break;
                case 12:
                    C00_Click(sender, e);
                    break;
                case 13:
                    C01_Click(sender, e);
                    break;
                case 14:
                    C02_Click(sender, e);
                    break;
                case 15:
                    C03_Click(sender, e);
                    break;
                case 16:
                    C04_Click(sender, e);
                    break;
                case 17:
                    C05_Click(sender, e);
                    break;
                case 18:
                    D00_Click(sender, e);
                    break;
                case 19:
                    D01_Click(sender, e);
                    break;
                case 20:
                    D02_Click(sender, e);
                    break;
                case 21:
                    D03_Click(sender, e);
                    break;
                case 22:
                    D04_Click(sender, e);
                    break;
                case 23:
                    D05_Click(sender, e);
                    break;
                case 24:
                    E00_Click(sender, e);
                    break;
                case 25:
                    E01_Click(sender, e);
                    break;
                case 26:
                    E02_Click(sender, e);
                    break;
                case 27:
                    E03_Click(sender, e);
                    break;
                case 28:
                    E04_Click(sender, e);
                    break;
                case 29:
                    E05_Click(sender, e);
                    break;
                case 30:
                    F00_Click(sender, e);
                    break;
                case 31:
                    F01_Click(sender, e);
                    break;
                case 32:
                    F02_Click(sender, e);
                    break;
                case 33:
                    F03_Click(sender, e);
                    break;
                case 34:
                    F04_Click(sender, e);
                    break;
                case 35:
                    F05_Click(sender, e);
                    break;
                default:
                    Console.WriteLine("EEEEEEEEROOOOOOOOOOORRR!");
                    break;
            }
        }

        /*private void openTimer_Tick(object sender, EventArgs e)
        {
            if (brojac == TimeToOpen)
            {
                brojac = 0;
                //openTimer.Stop();
                //indexer = cardsRegion.getIndexOfCard(/*pt Cursor.Position);
            }

            brojac++;
        }*/

        #endregion Dogadjaj za otvaranje karte

        private const UInt32 MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const UInt32 MOUSEEVENTF_LEFTUP = 0x0004;
        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, uint dwExtraInf);
        private void SimulateMouseClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);//make left button down
            Thread.Sleep(200);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);//make left button up
        }

        private void imageEditorDisplay2_Load(object sender, EventArgs e)
        {

        }

    }
}
