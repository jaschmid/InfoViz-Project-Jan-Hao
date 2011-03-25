namespace InfoVizProject
{
    partial class Form1
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

        // to unable the double click
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            if (m.Msg != 0xA3)
                base.WndProc(ref m);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.endYearLabel = new System.Windows.Forms.Label();
            this.startYearLabel = new System.Windows.Forms.Label();
            this.textBoxYear = new System.Windows.Forms.TextBox();
            this.comboBox_choropleth = new System.Windows.Forms.ComboBox();
            this.trackBarYearSelecter = new System.Windows.Forms.TrackBar();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.toolTipChoropleth = new System.Windows.Forms.ToolTip(this.components);
            this.BarGraphContainer = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarYearSelecter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BarGraphContainer)).BeginInit();
            this.BarGraphContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer1.Size = new System.Drawing.Size(784, 562);
            this.splitContainer1.SplitterDistance = 503;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.endYearLabel);
            this.splitContainer2.Panel1.Controls.Add(this.startYearLabel);
            this.splitContainer2.Panel1.Controls.Add(this.textBoxYear);
            this.splitContainer2.Panel1.Controls.Add(this.comboBox_choropleth);
            this.splitContainer2.Panel1.Controls.Add(this.trackBarYearSelecter);
            this.splitContainer2.Size = new System.Drawing.Size(503, 562);
            this.splitContainer2.SplitterDistance = 313;
            this.splitContainer2.TabIndex = 0;
            // 
            // endYearLabel
            // 
            this.endYearLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.endYearLabel.Location = new System.Drawing.Point(458, 255);
            this.endYearLabel.Name = "endYearLabel";
            this.endYearLabel.Size = new System.Drawing.Size(42, 13);
            this.endYearLabel.TabIndex = 7;
            this.endYearLabel.Text = "label1";
            this.endYearLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // startYearLabel
            // 
            this.startYearLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.startYearLabel.Location = new System.Drawing.Point(0, 255);
            this.startYearLabel.Name = "startYearLabel";
            this.startYearLabel.Size = new System.Drawing.Size(48, 13);
            this.startYearLabel.TabIndex = 6;
            this.startYearLabel.Text = "dgadfgsd";
            this.startYearLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxYear
            // 
            this.textBoxYear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxYear.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.textBoxYear.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxYear.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.textBoxYear.Location = new System.Drawing.Point(401, 13);
            this.textBoxYear.Name = "textBoxYear";
            this.textBoxYear.ReadOnly = true;
            this.textBoxYear.Size = new System.Drawing.Size(100, 13);
            this.textBoxYear.TabIndex = 5;
            this.textBoxYear.TabStop = false;
            this.textBoxYear.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // comboBox_choropleth
            // 
            this.comboBox_choropleth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_choropleth.FormattingEnabled = true;
            this.comboBox_choropleth.Location = new System.Drawing.Point(3, 12);
            this.comboBox_choropleth.Name = "comboBox_choropleth";
            this.comboBox_choropleth.Size = new System.Drawing.Size(121, 21);
            this.comboBox_choropleth.TabIndex = 1;
            this.comboBox_choropleth.TabStop = false;
            this.comboBox_choropleth.SelectedIndexChanged += new System.EventHandler(this.comboBox_choropleth_SelectedIndexChanged);
            // 
            // trackBarYearSelecter
            // 
            this.trackBarYearSelecter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.trackBarYearSelecter.Location = new System.Drawing.Point(0, 268);
            this.trackBarYearSelecter.Name = "trackBarYearSelecter";
            this.trackBarYearSelecter.Size = new System.Drawing.Size(503, 45);
            this.trackBarYearSelecter.TabIndex = 0;
            this.trackBarYearSelecter.TabStop = false;
            this.trackBarYearSelecter.ValueChanged += new System.EventHandler(this.trackBarYearSelecter_ValueChanged);
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.BarGraphContainer);
            this.splitContainer3.Panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer3_Panel1_Paint);
            this.splitContainer3.Panel2Collapsed = true;
            this.splitContainer3.Size = new System.Drawing.Size(277, 562);
            this.splitContainer3.SplitterDistance = 140;
            this.splitContainer3.TabIndex = 0;
            // 
            // BarGraphContainer
            // 
            this.BarGraphContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BarGraphContainer.Location = new System.Drawing.Point(0, 0);
            this.BarGraphContainer.Name = "BarGraphContainer";
            // 
            // BarGraphContainer.Panel1
            // 
            this.BarGraphContainer.Panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer4_Panel1_Paint);
            this.BarGraphContainer.Size = new System.Drawing.Size(277, 562);
            this.BarGraphContainer.SplitterDistance = 135;
            this.BarGraphContainer.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 562);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarYearSelecter)).EndInit();
            this.splitContainer3.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.BarGraphContainer)).EndInit();
            this.BarGraphContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        
        
        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.ComboBox comboBox_choropleth;
        private System.Windows.Forms.TrackBar trackBarYearSelecter;
        private System.Windows.Forms.TextBox textBoxYear;
        private System.Windows.Forms.ToolTip toolTipChoropleth;
        private System.Windows.Forms.Label startYearLabel;
        private System.Windows.Forms.Label endYearLabel;
        private System.Windows.Forms.SplitContainer BarGraphContainer;
        

    }
}

