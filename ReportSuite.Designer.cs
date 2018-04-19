namespace TBDispl
{
    partial class ReportSuite
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
            if (disposing && (components != null)) {
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
            this.btnCopy = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.lblGridSize = new System.Windows.Forms.Label();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnReportSave = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btnDesAll = new System.Windows.Forms.Button();
            this.btnSelAll = new System.Windows.Forms.Button();
            this.clbFlds = new System.Windows.Forms.CheckedListBox();
            this.lblFlds = new System.Windows.Forms.Label();
            this.cbxColSel = new System.Windows.Forms.CheckBox();
            this.srtNone = new System.Windows.Forms.RadioButton();
            this.srtDesc = new System.Windows.Forms.RadioButton();
            this.srtAsc = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.btnRangeStart = new System.Windows.Forms.Button();
            this.cbxFullTableFltrs = new System.Windows.Forms.CheckBox();
            this.btnAddFltr = new System.Windows.Forms.Button();
            this.txtRange = new System.Windows.Forms.TextBox();
            this.clbFltrs = new System.Windows.Forms.CheckedListBox();
            this.btnReportRemove = new System.Windows.Forms.Button();
            this.cmbReportList = new System.Windows.Forms.ComboBox();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCopy
            // 
            this.btnCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCopy.Location = new System.Drawing.Point(65, 3);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(56, 23);
            this.btnCopy.TabIndex = 1;
            this.btnCopy.Text = "&Copy";
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // btnExit
            // 
            this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExit.Location = new System.Drawing.Point(376, 3);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(56, 23);
            this.btnExit.TabIndex = 2;
            this.btnExit.Text = "E&xit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // dgvData
            // 
            this.dgvData.AllowUserToAddRows = false;
            this.dgvData.AllowUserToDeleteRows = false;
            this.dgvData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvData.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
            this.dgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvData.Location = new System.Drawing.Point(196, 12);
            this.dgvData.Name = "dgvData";
            this.dgvData.ReadOnly = true;
            this.dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvData.Size = new System.Drawing.Size(542, 432);
            this.dgvData.TabIndex = 3;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRefresh.Location = new System.Drawing.Point(3, 3);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(56, 23);
            this.btnRefresh.TabIndex = 9;
            this.btnRefresh.Text = "&Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // lblGridSize
            // 
            this.lblGridSize.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblGridSize.Location = new System.Drawing.Point(189, 1);
            this.lblGridSize.Name = "lblGridSize";
            this.lblGridSize.Size = new System.Drawing.Size(181, 26);
            this.lblGridSize.TabIndex = 11;
            this.lblGridSize.Text = "You are trying to view too many rows. Please narrow your search terms.";
            this.lblGridSize.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnExport
            // 
            this.btnExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnExport.Location = new System.Drawing.Point(127, 3);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(56, 23);
            this.btnExport.TabIndex = 21;
            this.btnExport.Text = "&Export";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnReportSave
            // 
            this.btnReportSave.Location = new System.Drawing.Point(4, 3);
            this.btnReportSave.Name = "btnReportSave";
            this.btnReportSave.Size = new System.Drawing.Size(55, 23);
            this.btnReportSave.TabIndex = 22;
            this.btnReportSave.Text = "&Save";
            this.btnReportSave.UseVisualStyleBackColor = true;
            this.btnReportSave.Click += new System.EventHandler(this.btnReportSave_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer1.Location = new System.Drawing.Point(12, 12);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.btnDesAll);
            this.splitContainer1.Panel1.Controls.Add(this.btnSelAll);
            this.splitContainer1.Panel1.Controls.Add(this.clbFlds);
            this.splitContainer1.Panel1.Controls.Add(this.lblFlds);
            this.splitContainer1.Panel1.Controls.Add(this.cbxColSel);
            this.splitContainer1.Panel1.Controls.Add(this.srtNone);
            this.splitContainer1.Panel1.Controls.Add(this.srtDesc);
            this.splitContainer1.Panel1.Controls.Add(this.srtAsc);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.btnRangeStart);
            this.splitContainer1.Panel2.Controls.Add(this.cbxFullTableFltrs);
            this.splitContainer1.Panel2.Controls.Add(this.btnAddFltr);
            this.splitContainer1.Panel2.Controls.Add(this.txtRange);
            this.splitContainer1.Panel2.Controls.Add(this.clbFltrs);
            this.splitContainer1.Size = new System.Drawing.Size(178, 432);
            this.splitContainer1.SplitterDistance = 296;
            this.splitContainer1.TabIndex = 23;
            // 
            // btnDesAll
            // 
            this.btnDesAll.Enabled = false;
            this.btnDesAll.Location = new System.Drawing.Point(131, 29);
            this.btnDesAll.Name = "btnDesAll";
            this.btnDesAll.Size = new System.Drawing.Size(41, 23);
            this.btnDesAll.TabIndex = 27;
            this.btnDesAll.Text = "&None";
            this.btnDesAll.UseVisualStyleBackColor = true;
            this.btnDesAll.Click += new System.EventHandler(this.btnDesAll_Click);
            // 
            // btnSelAll
            // 
            this.btnSelAll.Enabled = false;
            this.btnSelAll.Location = new System.Drawing.Point(84, 29);
            this.btnSelAll.Name = "btnSelAll";
            this.btnSelAll.Size = new System.Drawing.Size(41, 23);
            this.btnSelAll.TabIndex = 26;
            this.btnSelAll.Text = "&All";
            this.btnSelAll.UseVisualStyleBackColor = true;
            this.btnSelAll.Click += new System.EventHandler(this.btnSelAll_Click);
            // 
            // clbFlds
            // 
            this.clbFlds.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.clbFlds.FormattingEnabled = true;
            this.clbFlds.Location = new System.Drawing.Point(3, 56);
            this.clbFlds.Name = "clbFlds";
            this.clbFlds.Size = new System.Drawing.Size(170, 199);
            this.clbFlds.Sorted = true;
            this.clbFlds.TabIndex = 20;
            this.clbFlds.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.clbFlds_ItemCheck);
            this.clbFlds.SelectedIndexChanged += new System.EventHandler(this.clbFlds_SelectedIndexChanged);
            // 
            // lblFlds
            // 
            this.lblFlds.Location = new System.Drawing.Point(3, 0);
            this.lblFlds.Name = "lblFlds";
            this.lblFlds.Size = new System.Drawing.Size(168, 26);
            this.lblFlds.TabIndex = 21;
            this.lblFlds.Text = "Select a field below to create or edit a filter or control sorting.";
            // 
            // cbxColSel
            // 
            this.cbxColSel.AutoSize = true;
            this.cbxColSel.Location = new System.Drawing.Point(6, 26);
            this.cbxColSel.Name = "cbxColSel";
            this.cbxColSel.Size = new System.Drawing.Size(61, 30);
            this.cbxColSel.TabIndex = 28;
            this.cbxColSel.Text = "Column\r\nSelect";
            this.cbxColSel.UseVisualStyleBackColor = true;
            this.cbxColSel.CheckedChanged += new System.EventHandler(this.cbxColSel_CheckedChanged);
            // 
            // srtNone
            // 
            this.srtNone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.srtNone.AutoSize = true;
            this.srtNone.Enabled = false;
            this.srtNone.Location = new System.Drawing.Point(117, 278);
            this.srtNone.Name = "srtNone";
            this.srtNone.Size = new System.Drawing.Size(51, 17);
            this.srtNone.TabIndex = 24;
            this.srtNone.TabStop = true;
            this.srtNone.Text = "&None";
            this.srtNone.UseVisualStyleBackColor = true;
            this.srtNone.Click += new System.EventHandler(this.srtNone_CheckedChanged);
            // 
            // srtDesc
            // 
            this.srtDesc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.srtDesc.AutoSize = true;
            this.srtDesc.Enabled = false;
            this.srtDesc.Location = new System.Drawing.Point(58, 278);
            this.srtDesc.Name = "srtDesc";
            this.srtDesc.Size = new System.Drawing.Size(53, 17);
            this.srtDesc.TabIndex = 23;
            this.srtDesc.TabStop = true;
            this.srtDesc.Text = "&Desc.";
            this.srtDesc.UseVisualStyleBackColor = true;
            this.srtDesc.Click += new System.EventHandler(this.srtDesc_CheckedChanged);
            // 
            // srtAsc
            // 
            this.srtAsc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.srtAsc.AutoSize = true;
            this.srtAsc.Enabled = false;
            this.srtAsc.Location = new System.Drawing.Point(6, 278);
            this.srtAsc.Name = "srtAsc";
            this.srtAsc.Size = new System.Drawing.Size(46, 17);
            this.srtAsc.TabIndex = 22;
            this.srtAsc.TabStop = true;
            this.srtAsc.Text = "&Asc.";
            this.srtAsc.UseVisualStyleBackColor = true;
            this.srtAsc.Click += new System.EventHandler(this.srtAsc_CheckedChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 266);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 13);
            this.label1.TabIndex = 25;
            this.label1.Text = "Choose Sort Order:";
            // 
            // btnRangeStart
            // 
            this.btnRangeStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRangeStart.Enabled = false;
            this.btnRangeStart.Location = new System.Drawing.Point(122, 81);
            this.btnRangeStart.Name = "btnRangeStart";
            this.btnRangeStart.Size = new System.Drawing.Size(49, 21);
            this.btnRangeStart.TabIndex = 26;
            this.btnRangeStart.Text = "Ran&ge";
            this.btnRangeStart.UseVisualStyleBackColor = true;
            this.btnRangeStart.Click += new System.EventHandler(this.btnRangeStart_Click);
            // 
            // cbxFullTableFltrs
            // 
            this.cbxFullTableFltrs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbxFullTableFltrs.AutoSize = true;
            this.cbxFullTableFltrs.Location = new System.Drawing.Point(3, 84);
            this.cbxFullTableFltrs.Name = "cbxFullTableFltrs";
            this.cbxFullTableFltrs.Size = new System.Drawing.Size(107, 17);
            this.cbxFullTableFltrs.TabIndex = 24;
            this.cbxFullTableFltrs.Text = "Grid choices only";
            this.cbxFullTableFltrs.UseVisualStyleBackColor = true;
            this.cbxFullTableFltrs.CheckedChanged += new System.EventHandler(this.cbxFullTableFltrs_CheckedChanged);
            // 
            // btnAddFltr
            // 
            this.btnAddFltr.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAddFltr.Enabled = false;
            this.btnAddFltr.Location = new System.Drawing.Point(110, 104);
            this.btnAddFltr.Name = "btnAddFltr";
            this.btnAddFltr.Size = new System.Drawing.Size(61, 21);
            this.btnAddFltr.TabIndex = 23;
            this.btnAddFltr.Text = "Add &Fltr";
            this.btnAddFltr.UseVisualStyleBackColor = true;
            this.btnAddFltr.Click += new System.EventHandler(this.btnAddFltr_Click);
            // 
            // txtRange
            // 
            this.txtRange.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtRange.Location = new System.Drawing.Point(3, 104);
            this.txtRange.Name = "txtRange";
            this.txtRange.Size = new System.Drawing.Size(101, 20);
            this.txtRange.TabIndex = 22;
            this.txtRange.Click += new System.EventHandler(this.txtRange_Click);
            this.txtRange.TextChanged += new System.EventHandler(this.txtRange_TextChanged);
            // 
            // clbFltrs
            // 
            this.clbFltrs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.clbFltrs.CheckOnClick = true;
            this.clbFltrs.Enabled = false;
            this.clbFltrs.FormattingEnabled = true;
            this.clbFltrs.Location = new System.Drawing.Point(3, 3);
            this.clbFltrs.Name = "clbFltrs";
            this.clbFltrs.Size = new System.Drawing.Size(170, 64);
            this.clbFltrs.Sorted = true;
            this.clbFltrs.TabIndex = 21;
            this.clbFltrs.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.clbFltrs_ItemCheck);
            this.clbFltrs.SelectedIndexChanged += new System.EventHandler(this.clbRanges_SelectedIndexChanged);
            // 
            // btnReportRemove
            // 
            this.btnReportRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReportRemove.Location = new System.Drawing.Point(225, 3);
            this.btnReportRemove.Name = "btnReportRemove";
            this.btnReportRemove.Size = new System.Drawing.Size(55, 23);
            this.btnReportRemove.TabIndex = 24;
            this.btnReportRemove.Text = "Remo&ve";
            this.btnReportRemove.UseVisualStyleBackColor = true;
            this.btnReportRemove.Click += new System.EventHandler(this.btnReportRemove_Click);
            // 
            // cmbReportList
            // 
            this.cmbReportList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbReportList.DropDownWidth = 200;
            this.cmbReportList.FormattingEnabled = true;
            this.cmbReportList.Location = new System.Drawing.Point(65, 5);
            this.cmbReportList.Name = "cmbReportList";
            this.cmbReportList.Size = new System.Drawing.Size(154, 21);
            this.cmbReportList.Sorted = true;
            this.cmbReportList.TabIndex = 25;
            this.cmbReportList.SelectedIndexChanged += new System.EventHandler(this.cmbReportList_SelectedIndexChanged);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.Location = new System.Drawing.Point(12, 450);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.btnReportSave);
            this.splitContainer2.Panel1.Controls.Add(this.btnReportRemove);
            this.splitContainer2.Panel1.Controls.Add(this.cmbReportList);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.btnRefresh);
            this.splitContainer2.Panel2.Controls.Add(this.btnCopy);
            this.splitContainer2.Panel2.Controls.Add(this.btnExport);
            this.splitContainer2.Panel2.Controls.Add(this.btnExit);
            this.splitContainer2.Panel2.Controls.Add(this.lblGridSize);
            this.splitContainer2.Size = new System.Drawing.Size(726, 30);
            this.splitContainer2.SplitterDistance = 283;
            this.splitContainer2.TabIndex = 26;
            // 
            // ReportSuite
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(750, 485);
            this.Controls.Add(this.splitContainer2);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.dgvData);
            this.Name = "ReportSuite";
            this.Text = "Table Display Suite";
            this.Load += new System.EventHandler(this.ReportSuite_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCopy;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Label lblGridSize;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnReportSave;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnDesAll;
        private System.Windows.Forms.Button btnSelAll;
        private System.Windows.Forms.CheckedListBox clbFlds;
        private System.Windows.Forms.Label lblFlds;
        private System.Windows.Forms.CheckBox cbxColSel;
        private System.Windows.Forms.RadioButton srtNone;
        private System.Windows.Forms.RadioButton srtDesc;
        private System.Windows.Forms.RadioButton srtAsc;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbxFullTableFltrs;
        private System.Windows.Forms.Button btnAddFltr;
        private System.Windows.Forms.TextBox txtRange;
        private System.Windows.Forms.CheckedListBox clbFltrs;
        private System.Windows.Forms.Button btnReportRemove;
        private System.Windows.Forms.ComboBox cmbReportList;
        private System.Windows.Forms.Button btnRangeStart;
        private System.Windows.Forms.SplitContainer splitContainer2;
    }
}

