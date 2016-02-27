using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using DigitalImage.util;
using System.Drawing.Drawing2D;

namespace OcrTemplate.Utilities
{

    public class ImageUtil
    {

        #region Funkcije za pretvaranje Bitmap objekta u matricu 'nijansi sive boje' i obrnuto
        public static unsafe byte[,] bitmapToByteMatrix(Bitmap src) {
            Bitmap source = new Bitmap(src);
            Rectangle lrEntire = new Rectangle(new Point(), source.Size);

            BitmapData lbdSource = source.LockBits(lrEntire, ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);

            int PixelSize = 4;
            int h = lbdSource.Height;
            int w = lbdSource.Width;

            byte[,] slika = new byte[h, w];
            for (int y = 0; y < h; y++)
            {
                byte* row = (byte*)lbdSource.Scan0 + (y * lbdSource.Stride);
                for (int x = 0; x < w; x++)
                {
                    byte b = (byte)Math.Abs(row[x * PixelSize + 0]);// Blue
                    byte g = (byte)Math.Abs(row[x * PixelSize + 1]);// Green
                    byte r = (byte)Math.Abs(row[x * PixelSize + 2]);// Red
                    byte a = (byte)Math.Abs(row[x * PixelSize + 3]);// Alpha

                    byte prosek = (byte)(((double)b + (double)g + (double)r) / 3.0);

                    slika[y, x] = prosek;
                    if (a == 0)
                        slika[y, x] = 255;
                }
            }
            source.UnlockBits(lbdSource);
            return slika;        
        }

        public static unsafe Bitmap matrixToBitmap(byte[,] slika)
        {

            int w = slika.GetLength(1);// .GetLowerBound(0);
            int h = slika.GetLength(0);
            int PixelSize = 4;
            Bitmap into = new Bitmap(w, h);
            Rectangle lrEntire = new Rectangle(new Point(), new Size(w, h));

            BitmapData lbdDest = into.LockBits(lrEntire, ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
            for (int y = 0; y < h; y++)
            {
                byte* rowDest = (byte*)lbdDest.Scan0 + (y * lbdDest.Stride);
                for (int x = 0; x < w; x++)
                {
                    rowDest[x * PixelSize + 0] = slika[y, x];
                    rowDest[x * PixelSize + 1] = slika[y, x];
                    rowDest[x * PixelSize + 2] = slika[y, x];
                }
            }
            into.UnlockBits(lbdDest);
            return into;
        }
        #endregion

        #region Funkcije za pretvaranje Bitmap objekta u 'Color' matricu
        public static unsafe Bitmap colorMatrixToBitmap(byte[, ,] slika)
        {

            int w = slika.GetLength(1);// .GetLowerBound(0);
            int h = slika.GetLength(0);
            int PixelSize = 4;
            Bitmap into = new Bitmap(w, h);
            Rectangle lrEntire = new Rectangle(new Point(), new Size(w, h));

            BitmapData lbdDest = into.LockBits(lrEntire, ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
            for (int y = 0; y < h; y++)
            {
                byte* rowDest = (byte*)lbdDest.Scan0 + (y * lbdDest.Stride);
                for (int x = 0; x < w; x++)
                {
                    rowDest[x * PixelSize + 0] = slika[y, x, 2];
                    rowDest[x * PixelSize + 1] = slika[y, x, 1];
                    rowDest[x * PixelSize + 2] = slika[y, x, 0];
                }
            }
            into.UnlockBits(lbdDest);
            return into;
        }

        public static byte[, ,] bitmapToColorMatrix(Bitmap src)
        {
            Bitmap source = new Bitmap(src);
            Rectangle lrEntire = new Rectangle(new Point(), source.Size);

            BitmapData lbdSource = source.LockBits(lrEntire, ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
            int h = lbdSource.Height;
            int w = lbdSource.Width;

            byte[, ,] slika = new byte[h, w, 3];
            int PixelSize = 4;
            unsafe
            {
                for (int y = 0; y < h; y++)
                {
                    byte* row = (byte*)lbdSource.Scan0 + (y * lbdSource.Stride);
                    for (int x = 0; x < w; x++)
                    {
                        slika[y, x, 0] = row[x * PixelSize + 2];// Red
                        slika[y, x, 1] = row[x * PixelSize + 1];// Green
                        slika[y, x, 2] = row[x * PixelSize + 0];// Blue
                    }
                }
            }
            source.UnlockBits(lbdSource);
            return slika;
        }
        #endregion

        #region Funkcije za Segmentaciju slike iz 'nijansi sive boje' u crno belo
        public static double mean(byte[,] slika) {
            int w = slika.GetLength(1);
            int h = slika.GetLength(0);
            byte[,] retVal = new byte[h, w];
            double mean = 0;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    mean += slika[y, x];
                }
            }
            mean = mean / (w * h);
            return mean;
        }

        public static byte[,] matrixToBinary(byte[,] slika, byte mean) {
            int w = slika.GetLength(1);
            int h = slika.GetLength(0);
            byte[,] retVal = new byte[h, w];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (slika[y, x] < mean)
                        retVal[y, x] = 0;
                    else
                        retVal[y, x] = 255;
                }
            }
            return retVal;
        }

        public static byte[,] matrixToBinaryTiles(byte[,] slika, int R, int C)
        {
            int w = slika.GetLength(1);
            int h = slika.GetLength(0);
            double dW = (double)w / C;
            double dH = (double)h / R;
            double[,] means = new double[R, C];
            double[,] mins = new double[R, C];
            double[,] maxs = new double[R, C];
            byte[,] retVal = new byte[h, w];

            int[] histogram = new int[255 / 2];
            int D = 4;
            double meanDD = 0;
            for (int r = 0; r < R; r++)
            {
                for (int c = 0; c < C; c++)
                {
                    means[r, c] = 0;
                    mins[r, c] = 0;
                    maxs[r, c] = 0;
                    int minD = 0;
                    int maxD = 0;
                    int maxDif = 0;
                    for (int y = 0; y < dH; y++) {
                        int A = 0;
                        int B = 0;
                        for (int x = 0; x < D; x++) {
                            A += slika[(int)(r*dH) + y, (int)(c*dW)+x];
                        }
                        for (int x = D; x < 2*D; x++)
                        {
                            B += slika[(int)(r * dH) + y, (int)(c * dW) + x];
                        }
                        for (int x = D; x < dW - D; x++)
                        {
                            int diff = Math.Abs(A - B);
                            if (diff >= maxDif) {
                                maxDif = diff;
                                minD = Math.Min(A, B);
                                maxD = Math.Max(A, B);
                            }
                            A -= slika[(int)(r * dH) + y, (int)(c * dW) + x-D];
                            A += slika[(int)(r * dH) + y, (int)(c * dW) + x];
                            B -= slika[(int)(r * dH) + y, (int)(c * dW) + x];
                            B += slika[(int)(r * dH) + y, (int)(c * dW) + x+D-1];
                        }
                    }
                    int TT = (maxD + minD)/(2*D);
                    int DD = (maxD - minD)/D;
                    histogram[DD / 2]++;
                    meanDD += DD;
                    for (int y = 0; y < dH; y++)
                    {
                        for (int x = 0; x < dW; x++)
                        {
                            if (DD > 20)
                            {
                                if (slika[(int)(r * dH) + y, (int)(c * dW) + x] < TT)//means[r, c])
                                    retVal[(int)(r * dH) + y, (int)(c * dW) + x] = 0;
                                else
                                    retVal[(int)(r * dH) + y, (int)(c * dW) + x] = 255;
                            }
                            else {
                                if(TT<80)
                                    retVal[(int)(r * dH) + y, (int)(c * dW) + x] = 0;
                                else
                                    retVal[(int)(r * dH) + y, (int)(c * dW) + x] = 255;
                            }
                        }
                    }
                }
            }
            meanDD = meanDD / (R * C);
            return retVal;
        }

        public static List<PointF> histogram(byte[,] slika) {
            int w = slika.GetLength(1);
            int h = slika.GetLength(0);
            int dV = 1;
            int L = (256 / dV);
            int[] histogram = new int[L];
            for (int i = 0; i < L; i++)
                histogram[i] = 0;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    byte b = slika[y, x];
                    int indeks = b/dV;
                    histogram[indeks]++;
                }
            }
            List<PointF> points = new List<PointF>();
            for (int i = 0; i < histogram.Length; i++)
            {
                points.Add(new PointF(i * dV, histogram[i]));
            }
            return points;
        }
        #endregion

        #region Osnovne morfoloske operacije
        public static byte[,] erosion(byte[,] slika) { 
            int w = slika.GetLength(1);
            int h = slika.GetLength(0);
            byte[,] retVal = (byte[,])slika.Clone();
            int[] ii = {0,  1,  1,  1,  0, -1, -1, -1};
            int[] jj = {1,  1,  0, -1, -1, -1,  0,  1 };
            int n = ii.Length;
            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    Boolean b = true;
                    for (int t = 0; t < n; t++)
                    {
                        //Point point = new Point(p.X + jj[t], p.Y + ii[t]);
                        if (slika[y + ii[t], x + jj[t]] != 0) // NIJE CRNA TACKA 
                        {
                            b = false;
                            break;
                        }
                    }
                    if (b == true)
                        retVal[y, x] = 0;
                    else
                        retVal[y, x] = 255;
                }
            }
            return retVal;
        }

        public static byte[,] dilation(byte[,] slika)
        {
            int w = slika.GetLength(1);
            int h = slika.GetLength(0);
            byte[,] retVal = (byte[,])slika.Clone();
            int[] ii = { 0, 1, 1, 1, 0, -1, -1, -1 };
            int[] jj = { 1, 1, 0, -1, -1, -1, 0, 1 };
            int n = ii.Length;
           
            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    Boolean b = false;
                    for (int t = 0; t < n; t++)
                    {
                        if (slika[y + ii[t], x + jj[t]] == 0) // BAR JEDNA CRNA TACKA 
                        {
                            b = true;
                            break;
                        }
                    }
                    if (b == true)
                        retVal[y, x] = 0;
                    else
                        retVal[y, x] = 255;
                }
            }
            return retVal;
        }
        #endregion

        #region Algoritam za obelezavanje regiona
        public static List<RasterRegion> regionLabeling(byte[,] slika)
        {
            List<RasterRegion> regions = new List<RasterRegion>();
            int w = slika.GetLength(1);
            int h = slika.GetLength(0);
            byte[,] retVal = new byte[h, w];
            int[] ii = { 0, 1, 1, 1, 0, -1, -1, -1 };
            int[] jj = { 1, 1, 0, -1, -1, -1, 0, 1 };
            int n = ii.Length;
            byte regNum = 0;
            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    if (slika[y, x] == 0)
                    {
                        regNum++;
                        byte rr = (byte)(regNum * 50);
                        if (rr == 0)
                            rr = 1;
                        slika[y, x] = rr;
                        List<Point> front = new List<Point>();
                        Point pt = new Point(x, y);
                        RasterRegion region = new RasterRegion();
                        region.regId = regNum;
                        region.points.Add(pt);
                        regions.Add(region);
                        front.Add(pt);
                        while (front.Count > 0)
                        {
                            Point p = front[0];
                            front.RemoveAt(0);
                            for (int t = 0; t < n; t++)
                            {
                                Point point = new Point(p.X + jj[t], p.Y + ii[t]);
                                if (point.X > -1 && point.X < w && point.Y > -1 && point.Y < h)
                                {
                                    byte pp = slika[point.Y, point.X];
                                    if (pp == 0)
                                    {
                                        slika[point.Y, point.X] = slika[y, x];
                                        region.points.Add(point);
                                        front.Add(point);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return regions;
        }
        #endregion

        #region Detekcija ivica primenom Sobel operatora
        public static byte[,] iviceSobel(byte[,] slika)
        {
            int w = slika.GetLength(1);
            int h = slika.GetLength(0);
            byte[,] nslika = new byte[h, w];
            int[,] maskaA = {{-1, 0, 1}, 
                             {-2, 0, 2},
                             {-1, 0, 1}};
            int[,] maskaB = {{-1, -2, -1}, 
                             { 0,  0,  0},
                             { 1,  2,  1}};
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    nslika[y, x] = (byte)(0);
                }
            }
            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    int sumA = 0;
                    int sumB = 0;
                    for (int yy = -1; yy < 2; yy++)
                        for (int xx = -1; xx < 2; xx++)
                        {
                            sumA += maskaA[yy + 1, xx + 1] *
                                (int)slika[y + yy, x + xx];
                            sumB += maskaB[yy + 1, xx + 1] *
                                (int)slika[y + yy, x + xx];
                        }
                    double s = sumA * sumA + sumB * sumB;
                    nslika[y, x] = (byte)(Math.Sqrt(s));
                }
            }
            return nslika;
        }
        #endregion

        #region Funkcije za invertovanje slike i razlike izmedju dve slike
        public static byte[,] invert(byte[,] slika)
        {
            int w = slika.GetLength(1);
            int h = slika.GetLength(0);
            byte[,] nslika = new byte[h, w];
            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    nslika[y, x] = (byte)(255 - slika[y, x]);
                }
            }
            return nslika;
        }

        public static byte[,] diff(byte[,] slikaA, byte[,] slikaB)
        {
            int w = slikaA.GetLength(1);
            int h = slikaA.GetLength(0);
            byte[,] nslika = new byte[h, w];
            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    nslika[y, x] = (byte)Math.Abs(slikaB[y, x] - slikaA[y, x]);
                }
            }
            return nslika;
        }
        #endregion

        // Funkcija koja radi sa metodom GetPixel
        //   puno je sporije nego kad se radi direktno sa memorijom
        public static int[,] bitmapToMatrix(Bitmap source)
        {
            Rectangle lrEntire = new Rectangle(new Point(), source.Size);

            int w = source.Size.Width;
            int h = source.Size.Height;
            int[,] retVal = new int[h, w];


            for (int y = 0; y < h; y++) // lbdSource.Height
            {
                for (int x = 0; x < w; x++) // lbdSource.Width
                {
                    Color c = source.GetPixel(x, y);

                    byte a = c.A;
                    byte b = c.B;// Blue
                    byte g = c.G;// Green
                    byte r = c.R;// Red

                    if (c.A == 0)
                    {
                        retVal[y, x] = 0;
                    }
                    else
                    {
                        double v = (c.R + c.G + c.B) / 3;
                        double hh = 1 - (double)v / 255;
                        if (hh > 0.5)
                            retVal[y, x] = 1;
                        else
                            retVal[y, x] = 0;
                    }
                }
            }
            return retVal;
        }

        public static byte[,] resizeImage2(byte[,] src, int width, int height)
        {
            Bitmap image = matrixToBitmap(src);
            Bitmap result = new Bitmap(width, height);
            result.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (Graphics graphics = Graphics.FromImage(result))
            {
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.DrawImage(image, 0, 0, result.Width, result.Height);
            }

            //return the resulting bitmap
            return bitmapToByteMatrix(result);
        }

        public static byte[,] resizeImage(byte[,] src, Size size)
        {
            Bitmap imgToResize = matrixToBitmap(src);
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;
            nPercentW = ((float)size.Width / (float)sourceWidth);
            nPercentH = ((float)size.Height / (float)sourceHeight);

            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);
            Bitmap b = new Bitmap(size.Width, size.Height);
            int offX = (size.Width - destWidth) / 2;
            int offY = (size.Height - destHeight) / 2;
            Graphics g = Graphics.FromImage((Image)b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //g.InterpolationMode = InterpolationMode.NearestNeighbor;

            g.DrawImage(imgToResize, offX, offY, destWidth, destHeight);
            g.Dispose();
            return bitmapToByteMatrix(b);
        }    
    

    }
}
