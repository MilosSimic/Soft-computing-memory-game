namespace DigitalImage
{
    partial class FrmThreshold
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnToBinar = new System.Windows.Forms.Button();
            this.functionPlot1 = new AGisCore.FunctionPlot();
            this.hScrollBar1 = new System.Windows.Forms.HScrollBar();
            this.SuspendLayout();
            // 
            // btnToBinar
            // 
            this.btnToBinar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnToBinar.Location = new System.Drawing.Point(489, 2);
            this.btnToBinar.Name = "btnToBinar";
            this.btnToBinar.Size = new System.Drawing.Size(120, 23);
            this.btnToBinar.TabIndex = 24;
            this.btnToBinar.Text = "Mean";
            this.btnToBinar.UseVisualStyleBackColor = true;
            this.btnToBinar.Click += new System.EventHandler(this.btnToBinar_Click);
            // 
            // functionPlot1
            // 
            this.functionPlot1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.functionPlot1.Grid = false;
            this.functionPlot1.Location = new System.Drawing.Point(4, 3);
            this.functionPlot1.Name = "functionPlot1";
            this.functionPlot1.Size = new System.Drawing.Size(481, 188);
            this.functionPlot1.TabIndex = 28;
            // 
            // hScrollBar1
            // 
            this.hScrollBar1.Location = new System.Drawing.Point(29, 194);
            this.hScrollBar1.Maximum = 256;
            this.hScrollBar1.Name = "hScrollBar1";
            this.hScrollBar1.Size = new System.Drawing.Size(424, 17);
            this.hScrollBar1.TabIndex = 29;
            this.hScrollBar1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.hScrollBar1_Scroll);
            // 
            // FrmThreshold
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(611, 218);
            this.Controls.Add(this.hScrollBar1);
            this.Controls.Add(this.functionPlot1);
            this.Controls.Add(this.btnToBinar);
            this.Name = "FrmThreshold";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Threshold";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnToBinar;
        public AGisCore.FunctionPlot functionPlot1;
        private System.Windows.Forms.HScrollBar hScrollBar1;
    }
}