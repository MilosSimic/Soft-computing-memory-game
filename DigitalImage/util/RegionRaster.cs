using System;
using System.Collections.Generic;
using System.Drawing;

namespace DigitalImage.util
{
    public class RasterRegion
    {
        public String Tag = null;
        public int regId = 0;
        public List<Point> points = new List<Point>();

        public Boolean momentiOdredjeni = false;
        public int n = 0;  // ukupan broj tacaka

        public double c20 = 0;
        public double c11 = 0;
        public double c02 = 0;

        public int minX = int.MaxValue;
        public int maxX = 0;
        public int minY = int.MaxValue;
        public int maxY = 0;

        

        #region osnovne statisticke osobine regiona
        public double xM = 0;
        public double yM = 0;
        public double theta = 0;
        public double eccentricity = 0;
        public double majorAxisLength = 0;
        public double minorAxisLength = 0;
        #endregion

        public void odrediMomente()
        {
            if (momentiOdredjeni == true)
                return;
            n = points.Count;
            foreach (Point pp in points)
            {
                xM += pp.X;
                yM += pp.Y;

                // odredi "minimum bounding box" regiona
                if (pp.X < minX)
                    minX = pp.X;
                if (pp.X > maxX)
                    maxX = pp.X;
                if (pp.Y < minY)
                    minY = pp.Y;
                if (pp.Y > maxY)
                    maxY = pp.Y;
            }
            xM = xM / n;
            yM = yM / n;
            foreach (Point pp in points)
            {
                c20 += (pp.X - xM) * (pp.X - xM);
                c11 += (pp.X - xM) * (pp.Y - yM);
                c02 += (pp.Y - yM) * (pp.Y - yM);
            }
            // sad imamo vrednosti covariance matrix
            // c20 c11
            // c11 c02
            // odrediti karakteristicne vrednosti
            double a = 1;
            double b = -(c20 + c02);
            double c = c20 * c02 - c11 * c11;
            double D = b * b - 4 * c;
            double alfa1 = 0;
            double alfa2 = 0;
            if (D > 0)
            {
                D = Math.Sqrt(D);
                alfa1 = (-b + D) / 2 * a;
                alfa2 = (-b - D) / 2 * a;
                double temp1 = Math.Max(alfa1, alfa2);
                double temp2 = Math.Min(alfa1, alfa2);
                alfa1 = temp1;
                alfa2 = temp2;
                if (alfa1 != 0)
                    eccentricity = alfa2 / alfa1;
                majorAxisLength = alfa1;
                minorAxisLength = alfa2;
            }
            theta = 0.5 * Math.Atan2(2 * c11, (c20 - c02));
        }

        public byte[,] odrediSliku()
        {
            if (!momentiOdredjeni)
                odrediMomente();

            int height = maxY - minY + 1;
            int width = maxX - minX + 1;

            byte[,] retVal = new byte[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    retVal[i, j] = 255;
                }
            }

            foreach (Point p in points)
            {
                retVal[p.Y - minY, p.X - minX] = 0;
            }

            return retVal;
        }

        public byte[,] odrediNSliku()
        {
            if (!momentiOdredjeni)
                odrediMomente();

            List<Point> nPoints = new List<Point>();

            double ugao = Math.PI / 2 - Math.Abs(theta);
            foreach (Point p in points)
            {
                double nX = Math.Cos(ugao) * (p.X - xM) - Math.Sin(ugao) * (p.Y - yM) + xM;
                double nY = Math.Sin(ugao) * (p.X - xM) + Math.Cos(ugao) * (p.Y - yM) + yM;

                nPoints.Add(new Point((int)nX, (int)nY));
            }

            RasterRegion nRegion = new RasterRegion();
            nRegion.points = nPoints;

            return nRegion.odrediSliku();
        }

        public class RComparer : IComparer<RasterRegion>
        {
            #region IComparer<RasterRegion> Members
            public int Compare(RasterRegion a, RasterRegion b)
            {
                return a.minX.CompareTo(b.minX);
            }

            #endregion
        }

        public bool contains(Point point)
        {
            return points.Contains(point);
        }



        public Point tezisteRegiona()
        {
            int x = 0;
            int y = 0;
            if (points.Count > 0)
            {
                foreach (Point p in points)
                {
                    x += p.X;
                    y += p.Y;
                }
                x = x / points.Count;
                y = y / points.Count;
            }

            return new Point(x, y);
        }

    }

}
