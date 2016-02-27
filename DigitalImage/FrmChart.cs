using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AGisCore;

namespace DigitalImage
{
    public partial class FrmChart : Form
    {
        public FrmChart()
        {
            InitializeComponent();
        }

        public FrmChart(List<Function> a) {
            InitializeComponent();
            functionPlot1.funkcije = a;
        }

        private void FrmChart_Load(object sender, EventArgs e)
        {
            functionPlot1.Reset();
            functionPlot1.FitToScreen();
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            functionPlot1.CopyToClipboard();
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            if (functionPlot1.funkcije.Count > 0)
            {
                functionPlot1.funkcije[0].Color = Color.FromArgb(255-e.NewValue, functionPlot1.funkcije[0].Color);
                functionPlot1.Refresh();
            }
        }

        private void vScrollBar2_Scroll(object sender, ScrollEventArgs e)
        {
            if (functionPlot1.funkcije.Count > 1){
                functionPlot1.funkcije[1].Color = Color.FromArgb(255 - e.NewValue, functionPlot1.funkcije[1].Color);
                functionPlot1.Refresh();
            }
        }

        private void vScrollBar3_Scroll(object sender, ScrollEventArgs e)
        {
            if (functionPlot1.funkcije.Count > 2)
            {
                functionPlot1.funkcije[2].Color = Color.FromArgb(255 - e.NewValue, functionPlot1.funkcije[2].Color);
                functionPlot1.Refresh();
            }

        }

    }
}
