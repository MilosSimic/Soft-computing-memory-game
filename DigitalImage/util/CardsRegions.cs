using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;


namespace DigitalImage.util
{
    class CardsRegions
    {
        private List<RasterRegion> regions = new List<RasterRegion>();

        public int getIndexOfCard(Point point)
        {
            int i = 0;
            foreach (RasterRegion reg in regions)
            {
                if (reg.contains(point))
                {
                    return i;
                }
                else
                {
                    i++;
                }
            }
            return -1;
        }

        public List<RasterRegion> Regions
        {
            set { regions = value; }
        }
    }
}
