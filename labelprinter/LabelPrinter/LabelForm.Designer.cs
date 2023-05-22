namespace LabelPrinter;

partial class LabelForm
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
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.LabelTabControl = new System.Windows.Forms.TabControl();
            this.DomesticTabPage = new System.Windows.Forms.TabPage();
            this.ConvertToInternationalButton = new System.Windows.Forms.Button();
            this.ValidateDomesticButton = new System.Windows.Forms.Button();
            this.DomesticZipTextBox = new System.Windows.Forms.TextBox();
            this.DomesticStateTextBox = new System.Windows.Forms.TextBox();
            this.DomesticCityTextBox = new System.Windows.Forms.TextBox();
            this.DomesticAddress2TextBox = new System.Windows.Forms.TextBox();
            this.DomesticAddress1TextBox = new System.Windows.Forms.TextBox();
            this.DomesticNameTextBox = new System.Windows.Forms.TextBox();
            this.InternationalTabPage = new System.Windows.Forms.TabPage();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.InternationalCountryComboBox = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.InternationalAddressTextBox = new System.Windows.Forms.TextBox();
            this.PrintLabelButton = new System.Windows.Forms.Button();
            this.LabelPictureBox = new System.Windows.Forms.PictureBox();
            this.RefreshTimer = new System.Windows.Forms.Timer(this.components);
            this.ClearButton = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.CopiesNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.PrintLabelMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.PrintReturnLabelMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.PrintInternationalReturnLabelMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.PrintFromUsiMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CopyFromUsiMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.ClearMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ClearAfterPrintMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ShowImbMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ShowDataMatrixMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.ExitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.LabelTabControl.SuspendLayout();
            this.DomesticTabPage.SuspendLayout();
            this.InternationalTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LabelPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CopiesNumericUpDown)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "Address:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 96);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 15);
            this.label3.TabIndex = 2;
            this.label3.Text = "City/St/Zip:";
            // 
            // LabelTabControl
            // 
            this.LabelTabControl.Controls.Add(this.DomesticTabPage);
            this.LabelTabControl.Controls.Add(this.InternationalTabPage);
            this.LabelTabControl.Location = new System.Drawing.Point(11, 145);
            this.LabelTabControl.Name = "LabelTabControl";
            this.LabelTabControl.SelectedIndex = 0;
            this.LabelTabControl.Size = new System.Drawing.Size(389, 184);
            this.LabelTabControl.TabIndex = 0;
            this.LabelTabControl.SelectedIndexChanged += new System.EventHandler(this.LabelTabControl_SelectedIndexChanged);
            // 
            // DomesticTabPage
            // 
            this.DomesticTabPage.Controls.Add(this.ConvertToInternationalButton);
            this.DomesticTabPage.Controls.Add(this.ValidateDomesticButton);
            this.DomesticTabPage.Controls.Add(this.DomesticZipTextBox);
            this.DomesticTabPage.Controls.Add(this.DomesticStateTextBox);
            this.DomesticTabPage.Controls.Add(this.DomesticCityTextBox);
            this.DomesticTabPage.Controls.Add(this.DomesticAddress2TextBox);
            this.DomesticTabPage.Controls.Add(this.DomesticAddress1TextBox);
            this.DomesticTabPage.Controls.Add(this.DomesticNameTextBox);
            this.DomesticTabPage.Controls.Add(this.label3);
            this.DomesticTabPage.Controls.Add(this.label2);
            this.DomesticTabPage.Controls.Add(this.label1);
            this.DomesticTabPage.Location = new System.Drawing.Point(4, 24);
            this.DomesticTabPage.Name = "DomesticTabPage";
            this.DomesticTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.DomesticTabPage.Size = new System.Drawing.Size(381, 156);
            this.DomesticTabPage.TabIndex = 0;
            this.DomesticTabPage.Text = "Domestic";
            this.DomesticTabPage.UseVisualStyleBackColor = true;
            // 
            // ConvertToInternationalButton
            // 
            this.ConvertToInternationalButton.Location = new System.Drawing.Point(6, 122);
            this.ConvertToInternationalButton.Name = "ConvertToInternationalButton";
            this.ConvertToInternationalButton.Size = new System.Drawing.Size(115, 28);
            this.ConvertToInternationalButton.TabIndex = 10;
            this.ConvertToInternationalButton.Text = "To International >";
            this.ConvertToInternationalButton.UseVisualStyleBackColor = true;
            this.ConvertToInternationalButton.Click += new System.EventHandler(this.ConvertToInternationalButton_Click);
            // 
            // ValidateDomesticButton
            // 
            this.ValidateDomesticButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ValidateDomesticButton.Location = new System.Drawing.Point(247, 122);
            this.ValidateDomesticButton.Name = "ValidateDomesticButton";
            this.ValidateDomesticButton.Size = new System.Drawing.Size(128, 28);
            this.ValidateDomesticButton.TabIndex = 9;
            this.ValidateDomesticButton.Text = "Validate Address";
            this.ValidateDomesticButton.UseVisualStyleBackColor = true;
            this.ValidateDomesticButton.Click += new System.EventHandler(this.ValidateDomesticButton_Click);
            // 
            // DomesticZipTextBox
            // 
            this.DomesticZipTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DomesticZipTextBox.Location = new System.Drawing.Point(296, 93);
            this.DomesticZipTextBox.Name = "DomesticZipTextBox";
            this.DomesticZipTextBox.Size = new System.Drawing.Size(79, 23);
            this.DomesticZipTextBox.TabIndex = 8;
            // 
            // DomesticStateTextBox
            // 
            this.DomesticStateTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DomesticStateTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.DomesticStateTextBox.Location = new System.Drawing.Point(248, 93);
            this.DomesticStateTextBox.Name = "DomesticStateTextBox";
            this.DomesticStateTextBox.Size = new System.Drawing.Size(42, 23);
            this.DomesticStateTextBox.TabIndex = 7;
            // 
            // DomesticCityTextBox
            // 
            this.DomesticCityTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DomesticCityTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.DomesticCityTextBox.Location = new System.Drawing.Point(82, 93);
            this.DomesticCityTextBox.Name = "DomesticCityTextBox";
            this.DomesticCityTextBox.Size = new System.Drawing.Size(160, 23);
            this.DomesticCityTextBox.TabIndex = 6;
            // 
            // DomesticAddress2TextBox
            // 
            this.DomesticAddress2TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DomesticAddress2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.DomesticAddress2TextBox.Location = new System.Drawing.Point(82, 64);
            this.DomesticAddress2TextBox.Name = "DomesticAddress2TextBox";
            this.DomesticAddress2TextBox.Size = new System.Drawing.Size(293, 23);
            this.DomesticAddress2TextBox.TabIndex = 5;
            // 
            // DomesticAddress1TextBox
            // 
            this.DomesticAddress1TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DomesticAddress1TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.DomesticAddress1TextBox.Location = new System.Drawing.Point(82, 35);
            this.DomesticAddress1TextBox.Name = "DomesticAddress1TextBox";
            this.DomesticAddress1TextBox.Size = new System.Drawing.Size(293, 23);
            this.DomesticAddress1TextBox.TabIndex = 4;
            // 
            // DomesticNameTextBox
            // 
            this.DomesticNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DomesticNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.DomesticNameTextBox.Location = new System.Drawing.Point(82, 6);
            this.DomesticNameTextBox.Name = "DomesticNameTextBox";
            this.DomesticNameTextBox.Size = new System.Drawing.Size(293, 23);
            this.DomesticNameTextBox.TabIndex = 3;
            // 
            // InternationalTabPage
            // 
            this.InternationalTabPage.Controls.Add(this.label8);
            this.InternationalTabPage.Controls.Add(this.label7);
            this.InternationalTabPage.Controls.Add(this.InternationalCountryComboBox);
            this.InternationalTabPage.Controls.Add(this.label5);
            this.InternationalTabPage.Controls.Add(this.label4);
            this.InternationalTabPage.Controls.Add(this.InternationalAddressTextBox);
            this.InternationalTabPage.Location = new System.Drawing.Point(4, 24);
            this.InternationalTabPage.Name = "InternationalTabPage";
            this.InternationalTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.InternationalTabPage.Size = new System.Drawing.Size(381, 156);
            this.InternationalTabPage.TabIndex = 1;
            this.InternationalTabPage.Text = "International";
            this.InternationalTabPage.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 130);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(53, 15);
            this.label8.TabIndex = 5;
            this.label8.Text = "Country:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 9);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(52, 15);
            this.label7.TabIndex = 4;
            this.label7.Text = "Address:";
            // 
            // InternationalCountryComboBox
            // 
            this.InternationalCountryComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.InternationalCountryComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.InternationalCountryComboBox.FormattingEnabled = true;
            this.InternationalCountryComboBox.Location = new System.Drawing.Point(82, 127);
            this.InternationalCountryComboBox.Name = "InternationalCountryComboBox";
            this.InternationalCountryComboBox.Size = new System.Drawing.Size(293, 23);
            this.InternationalCountryComboBox.TabIndex = 3;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(0, 15);
            this.label5.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 129);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(0, 15);
            this.label4.TabIndex = 1;
            // 
            // InternationalAddressTextBox
            // 
            this.InternationalAddressTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.InternationalAddressTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.InternationalAddressTextBox.Location = new System.Drawing.Point(82, 6);
            this.InternationalAddressTextBox.Multiline = true;
            this.InternationalAddressTextBox.Name = "InternationalAddressTextBox";
            this.InternationalAddressTextBox.Size = new System.Drawing.Size(293, 115);
            this.InternationalAddressTextBox.TabIndex = 0;
            // 
            // PrintLabelButton
            // 
            this.PrintLabelButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.PrintLabelButton.Location = new System.Drawing.Point(298, 335);
            this.PrintLabelButton.Name = "PrintLabelButton";
            this.PrintLabelButton.Size = new System.Drawing.Size(102, 28);
            this.PrintLabelButton.TabIndex = 1;
            this.PrintLabelButton.Text = "Print Label(s)";
            this.PrintLabelButton.UseVisualStyleBackColor = true;
            this.PrintLabelButton.Click += new System.EventHandler(this.PrintLabelButton_Click);
            // 
            // LabelPictureBox
            // 
            this.LabelPictureBox.BackColor = System.Drawing.SystemColors.ControlDark;
            this.LabelPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LabelPictureBox.Location = new System.Drawing.Point(11, 35);
            this.LabelPictureBox.Name = "LabelPictureBox";
            this.LabelPictureBox.Size = new System.Drawing.Size(389, 104);
            this.LabelPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.LabelPictureBox.TabIndex = 3;
            this.LabelPictureBox.TabStop = false;
            // 
            // RefreshTimer
            // 
            this.RefreshTimer.Interval = 1000;
            this.RefreshTimer.Tick += new System.EventHandler(this.RefreshTimer_Tick);
            // 
            // ClearButton
            // 
            this.ClearButton.Location = new System.Drawing.Point(11, 335);
            this.ClearButton.Name = "ClearButton";
            this.ClearButton.Size = new System.Drawing.Size(76, 28);
            this.ClearButton.TabIndex = 4;
            this.ClearButton.Text = "Clear";
            this.ClearButton.UseVisualStyleBackColor = true;
            this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(190, 340);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(46, 15);
            this.label6.TabIndex = 6;
            this.label6.Text = "Copies:";
            // 
            // CopiesNumericUpDown
            // 
            this.CopiesNumericUpDown.Location = new System.Drawing.Point(242, 338);
            this.CopiesNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.CopiesNumericUpDown.Name = "CopiesNumericUpDown";
            this.CopiesNumericUpDown.Size = new System.Drawing.Size(50, 23);
            this.CopiesNumericUpDown.TabIndex = 7;
            this.CopiesNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.CopiesNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(412, 24);
            this.menuStrip1.TabIndex = 10;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.PrintLabelMenuItem,
            this.PrintReturnLabelMenuItem,
            this.PrintInternationalReturnLabelMenuItem,
            this.PrintFromUsiMenuItem,
            this.CopyFromUsiMenuItem,
            this.toolStripMenuItem3,
            this.ClearMenuItem,
            this.optionsToolStripMenuItem,
            this.toolStripMenuItem1,
            this.ExitMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(80, 20);
            this.toolsToolStripMenuItem.Text = "Application";
            // 
            // PrintLabelMenuItem
            // 
            this.PrintLabelMenuItem.Name = "PrintLabelMenuItem";
            this.PrintLabelMenuItem.Size = new System.Drawing.Size(251, 22);
            this.PrintLabelMenuItem.Text = "Print Label(s)";
            this.PrintLabelMenuItem.Click += new System.EventHandler(this.PrintLabelMenuItem_Click);
            // 
            // PrintReturnLabelMenuItem
            // 
            this.PrintReturnLabelMenuItem.Name = "PrintReturnLabelMenuItem";
            this.PrintReturnLabelMenuItem.Size = new System.Drawing.Size(251, 22);
            this.PrintReturnLabelMenuItem.Text = "Print Return Label(s)";
            this.PrintReturnLabelMenuItem.Click += new System.EventHandler(this.PrintReturnLabelMenuItem_Click);
            // 
            // PrintInternationalReturnLabelMenuItem
            // 
            this.PrintInternationalReturnLabelMenuItem.Name = "PrintInternationalReturnLabelMenuItem";
            this.PrintInternationalReturnLabelMenuItem.Size = new System.Drawing.Size(251, 22);
            this.PrintInternationalReturnLabelMenuItem.Text = "Print International Return Label(s)";
            this.PrintInternationalReturnLabelMenuItem.Click += new System.EventHandler(this.PrintInternationalReturnLabelMenuItem_Click);
            // 
            // PrintFromUsiMenuItem
            // 
            this.PrintFromUsiMenuItem.Name = "PrintFromUsiMenuItem";
            this.PrintFromUsiMenuItem.Size = new System.Drawing.Size(251, 22);
            this.PrintFromUsiMenuItem.Text = "Print from USI";
            this.PrintFromUsiMenuItem.Click += new System.EventHandler(this.PrintFromUsiMenuItem_Click);
            // 
            // CopyFromUsiMenuItem
            // 
            this.CopyFromUsiMenuItem.Name = "CopyFromUsiMenuItem";
            this.CopyFromUsiMenuItem.Size = new System.Drawing.Size(251, 22);
            this.CopyFromUsiMenuItem.Text = "Copy from USI";
            this.CopyFromUsiMenuItem.Click += new System.EventHandler(this.CopyFromUsiMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(248, 6);
            // 
            // ClearMenuItem
            // 
            this.ClearMenuItem.Name = "ClearMenuItem";
            this.ClearMenuItem.Size = new System.Drawing.Size(251, 22);
            this.ClearMenuItem.Text = "Clear";
            this.ClearMenuItem.Click += new System.EventHandler(this.ClearMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ClearAfterPrintMenuItem,
            this.ShowImbMenuItem,
            this.ShowDataMatrixMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(251, 22);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // ClearAfterPrintMenuItem
            // 
            this.ClearAfterPrintMenuItem.CheckOnClick = true;
            this.ClearAfterPrintMenuItem.Name = "ClearAfterPrintMenuItem";
            this.ClearAfterPrintMenuItem.Size = new System.Drawing.Size(231, 22);
            this.ClearAfterPrintMenuItem.Text = "Clear address after printing";
            // 
            // ShowImbMenuItem
            // 
            this.ShowImbMenuItem.CheckOnClick = true;
            this.ShowImbMenuItem.Enabled = false;
            this.ShowImbMenuItem.Name = "ShowImbMenuItem";
            this.ShowImbMenuItem.Size = new System.Drawing.Size(231, 22);
            this.ShowImbMenuItem.Text = "Show Intelligent Mail barcode";
            // 
            // ShowDataMatrixMenuItem
            // 
            this.ShowDataMatrixMenuItem.CheckOnClick = true;
            this.ShowDataMatrixMenuItem.Enabled = false;
            this.ShowDataMatrixMenuItem.Name = "ShowDataMatrixMenuItem";
            this.ShowDataMatrixMenuItem.Size = new System.Drawing.Size(231, 22);
            this.ShowDataMatrixMenuItem.Text = "Show DataMatrix barcode";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(248, 6);
            // 
            // ExitMenuItem
            // 
            this.ExitMenuItem.Name = "ExitMenuItem";
            this.ExitMenuItem.Size = new System.Drawing.Size(251, 22);
            this.ExitMenuItem.Text = "Exit";
            // 
            // LabelForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(412, 373);
            this.Controls.Add(this.CopiesNumericUpDown);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.ClearButton);
            this.Controls.Add(this.LabelPictureBox);
            this.Controls.Add(this.PrintLabelButton);
            this.Controls.Add(this.LabelTabControl);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "LabelForm";
            this.Text = "Print Address Label";
            this.LabelTabControl.ResumeLayout(false);
            this.DomesticTabPage.ResumeLayout(false);
            this.DomesticTabPage.PerformLayout();
            this.InternationalTabPage.ResumeLayout(false);
            this.InternationalTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LabelPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CopiesNumericUpDown)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TabControl LabelTabControl;
    private System.Windows.Forms.TabPage DomesticTabPage;
    private System.Windows.Forms.Button ValidateDomesticButton;
    private System.Windows.Forms.TextBox DomesticZipTextBox;
    private System.Windows.Forms.TextBox DomesticStateTextBox;
    private System.Windows.Forms.TextBox DomesticCityTextBox;
    private System.Windows.Forms.TextBox DomesticAddress2TextBox;
    private System.Windows.Forms.TextBox DomesticAddress1TextBox;
    private System.Windows.Forms.TextBox DomesticNameTextBox;
    private System.Windows.Forms.Button PrintLabelButton;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.PictureBox LabelPictureBox;
    private System.Windows.Forms.Timer RefreshTimer;
    private System.Windows.Forms.Button ClearButton;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.NumericUpDown CopiesNumericUpDown;
    private System.Windows.Forms.MenuStrip menuStrip1;
    private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem PrintLabelMenuItem;
    private System.Windows.Forms.ToolStripMenuItem PrintFromUsiMenuItem;
    private System.Windows.Forms.ToolStripMenuItem CopyFromUsiMenuItem;
    private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
    private System.Windows.Forms.ToolStripMenuItem ClearMenuItem;
    private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
    private System.Windows.Forms.ToolStripMenuItem ExitMenuItem;
    private System.Windows.Forms.ToolStripMenuItem PrintReturnLabelMenuItem;
    private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem ClearAfterPrintMenuItem;
    private System.Windows.Forms.ToolStripMenuItem ShowImbMenuItem;
    private System.Windows.Forms.ToolStripMenuItem ShowDataMatrixMenuItem;
    private System.Windows.Forms.TabPage InternationalTabPage;
    private System.Windows.Forms.ComboBox InternationalCountryComboBox;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.TextBox InternationalAddressTextBox;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.ToolStripMenuItem PrintInternationalReturnLabelMenuItem;
    private System.Windows.Forms.Button ConvertToInternationalButton;
}
