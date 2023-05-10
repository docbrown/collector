using DiffPlex;
using DiffPlex.Chunkers;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace LabelPrinter;

internal class ValidatedDomesticAddressReportForm : Form
{
    private const string Template = @"<?xml version=""1.0""?>
<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN""
   ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">
<html>
<head>
<style type=""text/css"">
body {
    background-color: ButtonFace;
    font: 9pt 'Segoe UI';
}
.inserted {
    color: green;
}
.deleted {
    color: red;
    text-decoration: line-through;
}
</style>
</head>
<body>
<table border=""1"">
    <tr>
        <th>Orginal Address</th>
        <th>Corrected Address</th>
    </tr>
    <tr>
        <td id=""OriginalAddress""></td>
        <td id=""CorrectedAddress""></td>
    </tr>
    <tr>
        <td><button onclick=""location.href='result://original'"" type=""button"">Use this address</button></td>
        <td><button onclick=""location.href='result://corrected'"" type=""button"">Use this address</button></td>
    </tr>
</table>
</body>
</html>";

    private WebBrowser _browser;

    private ValidatedDomesticAddress? _validatedAddress;
    public ValidatedDomesticAddress? ValidatedAddress
    {
        get => _validatedAddress;
        set
        {
            _validatedAddress = value;
            RefreshReport();
        }
    }

    public DomesticAddress? SelectedAddress { get; set; }

    public ValidatedDomesticAddressReportForm()
    {
        Text = "Domestic Address Validation Report";
        Width = 800;
        Height = 600;
        StartPosition = FormStartPosition.CenterParent;
        _browser = new WebBrowser
        {
            Dock = DockStyle.Fill
        };
        _browser.Navigating += OnBrowserNavigating;
        Controls.Add(_browser);
    }

    private void OnBrowserNavigating(object? sender, WebBrowserNavigatingEventArgs e)
    {
        if (e.Url.Scheme == "result")
        {
            e.Cancel = true;
            if (e.Url.Host == "original")
            {
                SelectedAddress = _validatedAddress?.OriginalAddress;
                DialogResult = DialogResult.OK;
            }
            else if (e.Url.Host == "corrected")
            {
                SelectedAddress = _validatedAddress;
                DialogResult = DialogResult.OK;
            }
        }
    }

    private void RefreshReport()
    {
        if (_validatedAddress == null)
        {
            return;
        }

        var diff = SideBySideDiffBuilder.Diff(_validatedAddress.OriginalAddress.ToString(), _validatedAddress.ToString());
        
        var doc = XDocument.Parse(Template);
        doc.Descendants("td")
            .First(x => x.Attribute("id")?.Value == "OriginalAddress")
            .ReplaceAll(FormatDiff(diff.OldText).ToArray());
        doc.Descendants("td")
            .First(x => x.Attribute("id")?.Value == "CorrectedAddress")
            .ReplaceAll(FormatDiff(diff.NewText).ToArray());

        _browser.DocumentText = doc.ToString();
    }

    private static IEnumerable<XNode> FormatDiff(DiffPaneModel diff)
    {
        for (int i = 0; i < diff.Lines.Count; i++)
        {
            var line = diff.Lines[i];
            if (!string.IsNullOrEmpty(line.Text))
            {
                switch (line.Type)
                {
                    case ChangeType.Modified:
                        foreach (var piece in line.SubPieces)
                        {
                            if (piece.Type == ChangeType.Imaginary)
                            {
                                continue;
                            }
                            if (piece.Type == ChangeType.Unchanged)
                            {
                                yield return new XText(piece.Text);
                            }
                            else
                            {
                                yield return new XElement("span",
                                    new XAttribute("class", piece.Type.ToString().ToLower()),
                                    new XText(piece.Text));
                            }
                        }
                        break;
                    default:
                        yield return new XText(line.Text);
                        break;
                }
            }
            if (i != diff.Lines.Count)
            {
                yield return new XElement("br");
            }
        }
    }
}
