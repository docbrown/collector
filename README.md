# Benton County Missouri Collector of Revenue

This repository contains code and data created for use in the Office of the
Collector of Revenue in Benton County Missouri.

## Projects

### [Microfilm Label Reader](scripts/microfilm2csv.py)

This is a Python script created to generate an inventory of microfilm from
photos of multiple containers. It utilitizes OpenCV to detect and isolate
the label from each container, then performs OCR with Tesseract to generate
a CSV listing of record series on each roll.

Run it with one of the sample images:

    python3 scripts/microfilm2csv.py testdata/microfilm/1.jpg

You can also pass the `-d` flag to get images of each preprocessing step and
the raw OCR results from Tesseract.

![](docs/images/microfilm2csv.png)

### [Tax Book PDF to CSV Converter](scripts/book2csv.py)

A Python script that uses MuPDF to read our PDF tax books and convert them to
CSV format.

Run it with one of the sample PDF files:

    python3 scripts/book2csv.py testdata/books/real_current.pdf

![](docs/images/book2csv.png)

### [Tax Statement Preprocessor](scripts/statements2pdf.py)

This is a Python script that preprocesses tax statements generated by our tax
collection system using GhostScript and MuPDF. It currently:

1. Converts the PostScript output to PDF
2. Crops pages from A4 to US Letter
3. Reverses the print order of the pages so that they are in the correct order
   after going through the automatic folding machine
4. Splits pages into batches
5. Adds sequencing information to the corner of each page
6. Detects statements with prior years due and adds an above-the-fold indicator
   that tells employees to insert a letter with the statement

Run it with one of the sample files:

    python3 scripts/statements2pdf.py testdata/statements/current.prn

### [Digital Signage Channel for Roku](roku/)

This is a sideloaded Roku channel that displays notices and events on the TV
in the hallway outside of our office.

![](docs/images/roku.png)

### [Address Label Printer](labelprinter/)

This is a C#/WinForms application for printing address labels. It supports
address validation via the USPS WebTools API. It can also read addresses from
our tax collection system. The version of Qt used by the system does not
support the Windows UI Automation API, so we use the WinRT OCR API to extract
an address from a screenshot of the active window.

This program is designed specifically for the Seiko Smart Label Printer 620
and SLP-2RLH thermal address labels.

![](docs/images/labelprinter.png)

## License

This software is the work of a government agency in the United States. To the
extent possible under law, the author(s) have dedicated all copyright and
related and neighboring rights to this software to the public domain worldwide.
This software is distributed without any warranty.

You should have received a copy of the CC0 Public Domain Dedication along with
this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.

## Privacy

A subset of local property tax data, including names and addresses of taxpayers,
is provided for testing purposes. This data is public information and available
from a variety of other sources.