using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Windows.Win32.Foundation;
using static LabelPrinter.SlpApi;

namespace LabelPrinter;

public partial class LabelForm : Form
{
    private static readonly DomesticAddress DomesticReturnAddress = new()
    {
        Name = "BENTON COUNTY COLLECTOR",
        Line1 = "PO BOX 428",
        City = "WARSAW",
        State = "MO",
        FullZip = "65355-0428"
    };

    private static readonly InternationalAddress InternationalReturnAddress = DomesticReturnAddress.ToInternationalAddress();

    private struct PrintOptions
    {
        public bool PreviewOnly = false;
        public bool UpdatePreview = true;
        public bool ShowMetadata = true;
        public string AncillaryEndorsement = "";
        public int Copies = 1;

        public PrintOptions() { }
    }

    public LabelForm()
    {
        InitializeComponent();

        this.Icon = Program.ExecutableIcon;

        foreach (Control c in GetAllControls(Controls))
        {
            if (c is TextBox tb)
            {
                tb.TextChanged += (sender, args) =>
                {
                    RefreshTimer.Start();
                };
            }
            else if (c is ComboBox cb)
            {
                cb.SelectedIndexChanged += (sender, args) =>
                {
                    RefreshTimer.Start();
                };
            }
        }

        using (var reader = new StreamReader(Assembly.GetCallingAssembly().GetManifestResourceStream("LabelPrinter.Resources.Countries.txt")))
        {
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                InternationalCountryComboBox.Items.Add(line.ToUpper());
            }
        }
    }

    private void SetAddress(Address address)
    {
        if (address is DomesticAddress domestic)
        {
            DomesticNameTextBox.Text = domestic.Name;
            DomesticAddress1TextBox.Text = domestic.Line1;
            DomesticAddress2TextBox.Text = domestic.Line2;
            DomesticCityTextBox.Text = domestic.City;
            DomesticStateTextBox.Text = domestic.State;
            DomesticZipTextBox.Text = domestic.FullZip;
            LabelTabControl.SelectedTab = DomesticTabPage;
        }
        else if (address is InternationalAddress international)
        {
            InternationalAddressTextBox.Text = international.Text;
            InternationalCountryComboBox.SelectedValue = international.Country;
            LabelTabControl.SelectedTab = InternationalTabPage;
        }
    }

    private Address BuildAddress()
    {
        if (LabelTabControl.SelectedTab == DomesticTabPage)
        {
            return new DomesticAddress
            {
                Name = DomesticNameTextBox.Text,
                Line1 = DomesticAddress1TextBox.Text,
                Line2 = DomesticAddress2TextBox.Text,
                City = DomesticCityTextBox.Text,
                State = DomesticStateTextBox.Text,
                FullZip = DomesticZipTextBox.Text
            };
        }
        else if (LabelTabControl.SelectedTab == InternationalTabPage)
        {
            return new InternationalAddress
            {
                Text = InternationalAddressTextBox.Text,
                Country = InternationalCountryComboBox.Text
            };
        }
        else
        {
            throw new InvalidOperationException("Can't build an address for this tab page!");
        }
    }

    private void PrintLabelButton_Click(object sender, EventArgs e)
    {
        PrintLabel(BuildAddress(), new PrintOptions
        {
            Copies = (int)CopiesNumericUpDown.Value
        });
    }

    private void Clear()
    {
        DomesticNameTextBox.Clear();
        DomesticAddress1TextBox.Clear();
        DomesticAddress2TextBox.Clear();
        DomesticCityTextBox.Clear();
        DomesticStateTextBox.Clear();
        DomesticZipTextBox.Clear();
        InternationalAddressTextBox.Clear();
    }

    private void PrintLabel(Address address, PrintOptions options)
    {
        int copies = 1;
        if (!options.PreviewOnly)
        {
            copies = options.Copies;
        }
        if (copies > 9)
        {
            if (MessageBox.Show(this, "That's a lot of labels. Are you sure?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
            {
                return;
            }
        }

        var lines = string.Join('\n', address.Lines);
        var machine = Environment.MachineName.ToUpper();
        var today = DateTime.Today.ToShortDateString();

        SlpOpenPrinter("Smart Label Printer 620", 1, false);

        for (int i = 0; i < copies; i++)
        {
            SlpStartLabel();
            var font = SlpCreateFont("Arial", 14, 1);
            SlpDrawTextXY(10, 10, font, lines);
            if (!string.IsNullOrWhiteSpace(options.AncillaryEndorsement))
            {
                var smallFont = SlpCreateFont("Arial", 12, 0);
                SlpDrawTextXY(10, 145, smallFont, options.AncillaryEndorsement);
                SlpDeleteFont(smallFont);
            }
            if (options.ShowMetadata)
            {
                var verySmallFont = SlpCreateFont("Arial", 8, 0);
                SlpDrawTextXY(490, 160, verySmallFont, $"{machine} {today}");
                SlpDeleteFont(verySmallFont);
            }
            if (options.UpdatePreview)
            {
                LabelPictureBox.Image = SlpRenderLabel();
            }
            if (!options.PreviewOnly)
            {
                SlpEndLabel();
            }
        }

        SlpClosePrinter();

        if (ClearAfterPrintMenuItem.Checked)
        {
            Clear();
        }
    }

    private static IEnumerable<Control> GetAllControls(Control.ControlCollection root)
    {
        foreach (Control child in root)
        {
            foreach (Control grandChild in GetAllControls(child.Controls))
            {
                yield return grandChild;
            }
            yield return child;
        }
    }

    private void RefreshTimer_Tick(object sender, EventArgs e)
    {
        RefreshTimer.Stop();
        PrintLabel(BuildAddress(), new PrintOptions
        {
            PreviewOnly = true
        });
    }

    private void LabelTabControl_SelectedIndexChanged(object sender, EventArgs e)
    {
        RefreshTimer.Start();
    }

    private void ClearButton_Click(object sender, EventArgs e)
    {
        Clear();
    }

    private void ClearMenuItem_Click(object sender, EventArgs e)
    {
        Clear();
    }

    private void PrintLabelMenuItem_Click(object sender, EventArgs e)
    {
        PrintLabel(BuildAddress(), new PrintOptions
        {
            Copies = (int)CopiesNumericUpDown.Value
        });
    }

    private void PrintReturnLabelMenuItem_Click(object sender, EventArgs e)
    {
        PrintLabel(DomesticReturnAddress, new PrintOptions
        {
            UpdatePreview = false,
            ShowMetadata = false,
            AncillaryEndorsement = "Address Service Requested",
            Copies = (int)CopiesNumericUpDown.Value
        });
    }

    private void PrintInternationalReturnLabelMenuItem_Click(object sender, EventArgs e)
    {
        PrintLabel(InternationalReturnAddress, new PrintOptions
        {
            UpdatePreview = false,
            ShowMetadata = false,
            Copies = (int)CopiesNumericUpDown.Value
        });
    }

    private async void ValidateDomesticButton_Click(object sender, EventArgs e)
    {
        var validated = await UspsWebTools.ValidateAddressAsync((DomesticAddress)BuildAddress());
        var form = new ValidatedDomesticAddressReportForm();
        form.ValidatedAddress = validated;
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            var addr = form.SelectedAddress;
            DomesticNameTextBox.Text = addr.Name;
            DomesticAddress1TextBox.Text = addr.Line1;
            DomesticAddress2TextBox.Text = addr.Line2;
            DomesticCityTextBox.Text = addr.City;
            DomesticStateTextBox.Text = addr.State;
            DomesticZipTextBox.Text = addr.FullZip;
        }
    }

    private void PrintFromUsiMenuItem_Click(object sender, EventArgs e)
    {
        var result = Usi.GetCurrentRecordAddress();
        if (result == null)
        {
            MessageBox.Show(this, "Sorry, I could not locate an address on the screen.\nMake sure you have queried for a record!",
                "Oops.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        PrintLabel(result, new PrintOptions
        {
            Copies = (int)CopiesNumericUpDown.Value,
            UpdatePreview = false
        });
        
        //Usi.DrawTextRectangles(bitmap, result);
        //Debug.WriteLine(result.Text);

        /*var form = new Form();
        form.AutoSize = true;
        form.AutoSizeMode = AutoSizeMode.GrowOnly;
        var image = new PictureBox();
        image.MouseMove += (sender, args) =>
        {
            form.Text = $"X: {args.X}, Y: {args.Y}";
        };
        image.SizeMode = PictureBoxSizeMode.AutoSize;
        image.Image = bitmap;
        form.Controls.Add(image);
        form.ShowDialog();*/
    }

    private void ConvertToInternationalButton_Click(object sender, EventArgs e)
    {
        var address = BuildAddress() as DomesticAddress;
        SetAddress(address.ToInternationalAddress());
    }
}
