﻿namespace DailyWallpaper
{
    partial class CleanEmptyFoldersForm
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
            this.groupBoxTarget = new System.Windows.Forms.GroupBox();
            this.regexCheckBox = new System.Windows.Forms.CheckBox();
            this.filterExample = new System.Windows.Forms.TextBox();
            this.protectionFilterTextBox = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnSelectTargetFolder = new System.Windows.Forms.Button();
            this.tbTargetFolder = new System.Windows.Forms.TextBox();
            this.labelTarget = new System.Windows.Forms.Label();
            this.btnPrint = new System.Windows.Forms.Button();
            this.groupBoxConsole = new System.Windows.Forms.GroupBox();
            this.tbConsole = new System.Windows.Forms.TextBox();
            this.btnClean = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.deleteOrRecycleBin = new System.Windows.Forms.CheckBox();
            this.saveList2File = new System.Windows.Forms.Button();
            this.listOrLog = new System.Windows.Forms.CheckBox();
            this.groupBoxTarget.SuspendLayout();
            this.groupBoxConsole.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxTarget
            // 
            this.groupBoxTarget.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxTarget.Controls.Add(this.regexCheckBox);
            this.groupBoxTarget.Controls.Add(this.filterExample);
            this.groupBoxTarget.Controls.Add(this.protectionFilterTextBox);
            this.groupBoxTarget.Controls.Add(this.textBox1);
            this.groupBoxTarget.Controls.Add(this.btnSelectTargetFolder);
            this.groupBoxTarget.Controls.Add(this.tbTargetFolder);
            this.groupBoxTarget.Controls.Add(this.labelTarget);
            this.groupBoxTarget.Location = new System.Drawing.Point(9, 1);
            this.groupBoxTarget.Margin = new System.Windows.Forms.Padding(4);
            this.groupBoxTarget.Name = "groupBoxTarget";
            this.groupBoxTarget.Padding = new System.Windows.Forms.Padding(4);
            this.groupBoxTarget.Size = new System.Drawing.Size(1010, 80);
            this.groupBoxTarget.TabIndex = 1;
            this.groupBoxTarget.TabStop = false;
            this.groupBoxTarget.Text = "Paths";
            // 
            // regexCheckBox
            // 
            this.regexCheckBox.AutoSize = true;
            this.regexCheckBox.Location = new System.Drawing.Point(523, 51);
            this.regexCheckBox.Name = "regexCheckBox";
            this.regexCheckBox.Size = new System.Drawing.Size(52, 22);
            this.regexCheckBox.TabIndex = 9;
            this.regexCheckBox.Text = "RE";
            this.regexCheckBox.UseVisualStyleBackColor = true;
            this.regexCheckBox.CheckedChanged += new System.EventHandler(this.regexCheckBox_CheckedChanged);
            // 
            // filterExample
            // 
            this.filterExample.BackColor = System.Drawing.SystemColors.Control;
            this.filterExample.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.filterExample.Location = new System.Drawing.Point(577, 52);
            this.filterExample.Name = "filterExample";
            this.filterExample.ReadOnly = true;
            this.filterExample.Size = new System.Drawing.Size(297, 21);
            this.filterExample.TabIndex = 8;
            this.filterExample.Text = " Such as: equal,freedom,Pictures";
            this.filterExample.TextChanged += new System.EventHandler(this.filterExample_TextChanged);
            // 
            // protectionFilterTextBox
            // 
            this.protectionFilterTextBox.Location = new System.Drawing.Point(180, 45);
            this.protectionFilterTextBox.Name = "protectionFilterTextBox";
            this.protectionFilterTextBox.Size = new System.Drawing.Size(337, 28);
            this.protectionFilterTextBox.TabIndex = 7;
            this.protectionFilterTextBox.TextChanged += new System.EventHandler(this.protectionFilterTextBox_TextChanged);
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.Control;
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Location = new System.Drawing.Point(12, 52);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(177, 21);
            this.textBox1.TabIndex = 6;
            this.textBox1.Text = "Protection filter:";
            // 
            // btnSelectTargetFolder
            // 
            this.btnSelectTargetFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectTargetFolder.Location = new System.Drawing.Point(901, 20);
            this.btnSelectTargetFolder.Margin = new System.Windows.Forms.Padding(4);
            this.btnSelectTargetFolder.Name = "btnSelectTargetFolder";
            this.btnSelectTargetFolder.Size = new System.Drawing.Size(99, 47);
            this.btnSelectTargetFolder.TabIndex = 5;
            this.btnSelectTargetFolder.Text = "Select";
            this.btnSelectTargetFolder.UseVisualStyleBackColor = true;
            this.btnSelectTargetFolder.Click += new System.EventHandler(this.btnSelectOutFolder_Click);
            // 
            // tbTargetFolder
            // 
            this.tbTargetFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbTargetFolder.Location = new System.Drawing.Point(180, 14);
            this.tbTargetFolder.Margin = new System.Windows.Forms.Padding(4);
            this.tbTargetFolder.Name = "tbTargetFolder";
            this.tbTargetFolder.Size = new System.Drawing.Size(694, 28);
            this.tbTargetFolder.TabIndex = 3;
            this.tbTargetFolder.TabStop = false;
            this.tbTargetFolder.TextChanged += new System.EventHandler(this.tbTargetFolder_TextChanged);
            // 
            // labelTarget
            // 
            this.labelTarget.AutoSize = true;
            this.labelTarget.Location = new System.Drawing.Point(9, 25);
            this.labelTarget.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelTarget.Name = "labelTarget";
            this.labelTarget.Size = new System.Drawing.Size(134, 18);
            this.labelTarget.TabIndex = 1;
            this.labelTarget.Text = "Target Folder:";
            // 
            // btnPrint
            // 
            this.btnPrint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPrint.Location = new System.Drawing.Point(298, 649);
            this.btnPrint.Margin = new System.Windows.Forms.Padding(4);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(80, 32);
            this.btnPrint.TabIndex = 5;
            this.btnPrint.Text = "Print";
            this.btnPrint.UseVisualStyleBackColor = true;
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
            // 
            // groupBoxConsole
            // 
            this.groupBoxConsole.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxConsole.Controls.Add(this.tbConsole);
            this.groupBoxConsole.Location = new System.Drawing.Point(9, 100);
            this.groupBoxConsole.Margin = new System.Windows.Forms.Padding(4);
            this.groupBoxConsole.Name = "groupBoxConsole";
            this.groupBoxConsole.Padding = new System.Windows.Forms.Padding(4);
            this.groupBoxConsole.Size = new System.Drawing.Size(1010, 530);
            this.groupBoxConsole.TabIndex = 3;
            this.groupBoxConsole.TabStop = false;
            this.groupBoxConsole.Text = "Console";
            // 
            // tbConsole
            // 
            this.tbConsole.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbConsole.Location = new System.Drawing.Point(9, 28);
            this.tbConsole.Margin = new System.Windows.Forms.Padding(4);
            this.tbConsole.Multiline = true;
            this.tbConsole.Name = "tbConsole";
            this.tbConsole.ReadOnly = true;
            this.tbConsole.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbConsole.Size = new System.Drawing.Size(990, 485);
            this.tbConsole.TabIndex = 0;
            this.tbConsole.WordWrap = false;
            // 
            // btnClean
            // 
            this.btnClean.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClean.Location = new System.Drawing.Point(867, 638);
            this.btnClean.Margin = new System.Windows.Forms.Padding(4);
            this.btnClean.Name = "btnClean";
            this.btnClean.Size = new System.Drawing.Size(148, 56);
            this.btnClean.TabIndex = 4;
            this.btnClean.Text = "Delete";
            this.btnClean.UseVisualStyleBackColor = true;
            this.btnClean.Click += new System.EventHandler(this.btnClean_Click);
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClear.Location = new System.Drawing.Point(21, 649);
            this.btnClear.Margin = new System.Windows.Forms.Padding(4);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(112, 32);
            this.btnClear.TabIndex = 4;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnStop
            // 
            this.btnStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStop.Location = new System.Drawing.Point(582, 649);
            this.btnStop.Margin = new System.Windows.Forms.Padding(4);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(112, 32);
            this.btnStop.TabIndex = 4;
            this.btnStop.Text = "STOP";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // deleteOrRecycleBin
            // 
            this.deleteOrRecycleBin.AutoSize = true;
            this.deleteOrRecycleBin.Location = new System.Drawing.Point(817, 656);
            this.deleteOrRecycleBin.Name = "deleteOrRecycleBin";
            this.deleteOrRecycleBin.Size = new System.Drawing.Size(22, 21);
            this.deleteOrRecycleBin.TabIndex = 6;
            this.deleteOrRecycleBin.UseVisualStyleBackColor = true;
            this.deleteOrRecycleBin.CheckedChanged += new System.EventHandler(this.deleteOrRecycleBin_CheckedChanged);
            // 
            // saveList2File
            // 
            this.saveList2File.Location = new System.Drawing.Point(850, 78);
            this.saveList2File.Name = "saveList2File";
            this.saveList2File.Size = new System.Drawing.Size(169, 33);
            this.saveList2File.TabIndex = 7;
            this.saveList2File.Text = "Save list to File";
            this.saveList2File.UseVisualStyleBackColor = true;
            this.saveList2File.Click += new System.EventHandler(this.saveList2File_Click);
            // 
            // listOrLog
            // 
            this.listOrLog.AutoSize = true;
            this.listOrLog.Location = new System.Drawing.Point(807, 85);
            this.listOrLog.Name = "listOrLog";
            this.listOrLog.Size = new System.Drawing.Size(22, 21);
            this.listOrLog.TabIndex = 8;
            this.listOrLog.UseVisualStyleBackColor = true;
            this.listOrLog.CheckedChanged += new System.EventHandler(this.listOrLog_CheckedChanged);
            // 
            // CleanEmptyFoldersForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1028, 701);
            this.Controls.Add(this.listOrLog);
            this.Controls.Add(this.saveList2File);
            this.Controls.Add(this.btnClean);
            this.Controls.Add(this.deleteOrRecycleBin);
            this.Controls.Add(this.btnPrint);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.groupBoxConsole);
            this.Controls.Add(this.groupBoxTarget);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(754, 536);
            this.Name = "CleanEmptyFoldersForm";
            this.Text = "Clean Empty Folders";
            this.Load += new System.EventHandler(this.CleanEmptyFoldersForm_Load);
            this.groupBoxTarget.ResumeLayout(false);
            this.groupBoxTarget.PerformLayout();
            this.groupBoxConsole.ResumeLayout(false);
            this.groupBoxConsole.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBoxTarget;
        private System.Windows.Forms.TextBox tbTargetFolder;
        private System.Windows.Forms.Label labelTarget;
        private System.Windows.Forms.Button btnSelectTargetFolder;
        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.GroupBox groupBoxConsole;
        private System.Windows.Forms.Button btnClean;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnStop;
        public System.Windows.Forms.TextBox tbConsole;
        private System.Windows.Forms.CheckBox deleteOrRecycleBin;
        private System.Windows.Forms.Button saveList2File;
        private System.Windows.Forms.CheckBox listOrLog;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox protectionFilterTextBox;
        private System.Windows.Forms.TextBox filterExample;
        private System.Windows.Forms.CheckBox regexCheckBox;
    }
}