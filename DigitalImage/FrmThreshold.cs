using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OcrTemplate.Utilities;
using AGisCore;
using AGisCore.Entity;

namespace DigitalImage
{

    public partial class FrmThreshold : Form
    {
        MainForm mainForm = null;
        Bitmap orginalImage = null;
    
        public FrmThreshold(MainForm aMainForm)
        {
            InitializeComponent();
            mainForm = aMainForm;
            Bitmap bmp = mainForm.imageEditorDisplay1.mapa.bmp;
            orginalImage = (Bitmap)mainForm.imageEditorDisplay1.mapa.bmp.Clone(); ;
            
            byte[,] slika = ImageUtil.bitmapToByteMatrix(bmp);
            double mm = ImageUtil.mean(slika);

            List<PointF> points = ImageUtil.histogram(slika);
            double maxY = double.MinValue;
            foreach (PointF p in points)
                maxY = Math.Max(maxY, p.Y);
            Function f1 = new Function(Color.Red, points, Function.VBAR);
            List<Function> ff = new List<Function>();
            ff.Add(f1);
            functionPlot1.funkcije = ff;
            functionPlot1.Reset();
            functionPlot1.FitToScreen();

            Linija ll = new Linija(new Tacka(mm, 0), new Tacka(mm, maxY));
            ll.boja = Color.Blue;

            functionPlot1.linije.Add(ll);
            byte[,] bSlika = ImageUtil.matrixToBinary(slika, (byte)mm);
            Bitmap into = ImageUtil.matrixToBitmap(bSlika);

            mainForm.imageEditorDisplay1.mapa.bmp = into;
            mainForm.imageEditorDisplay1.FitImage();
            mainForm.imageEditorDisplay1.Refresh();

            hScrollBar1.Value = (int)mm;

        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            double mm = e.NewValue;
            threshold((int)mm);
        }

        private void threshold(int mm) {
            Bitmap bmp = mainForm.imageEditorDisplay1.mapa.bmp;
            byte[,] slika = ImageUtil.bitmapToByteMatrix(orginalImage);
            List<PointF> points = ImageUtil.histogram(slika);

            Linija ll = functionPlot1.linije[0]; //new Linija(new Tacka(mm, 0), new Tacka(mm, maxY));
            ll.tacke[0].x = mm;
            ll.tacke[1].x = mm;

            byte[,] bSlika = ImageUtil.matrixToBinary(slika, (byte)mm);
            Bitmap into = ImageUtil.matrixToBitmap(bSlika);

            functionPlot1.Refresh();
            mainForm.imageEditorDisplay1.mapa.bmp = into;
            mainForm.imageEditorDisplay1.FitImage();
            mainForm.imageEditorDisplay1.Refresh();        
        }

        private void btnToBinar_Click(object sender, EventArgs e)
        {
            byte[,] slika = ImageUtil.bitmapToByteMatrix(orginalImage);
            double mm = ImageUtil.mean(slika);
            hScrollBar1.Value = (int)mm;
            functionPlot1.Refresh();
            threshold((int)mm);
        }

    }
}
