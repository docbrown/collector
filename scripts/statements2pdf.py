# This script prepares tax statements for printing.
#
# Step 1: Convert PostScript files to single-page PDFs.
# Step 2: Adjust crop boxes from A4 to US Letter size.
# Step 3: Combine pages in reverse order.

import argparse
import fitz
import glob
import os
import subprocess
import sys

parser = argparse.ArgumentParser(description='Preprocess tax statements')
parser.add_argument('directory', help='input directory')
parser.add_argument('--pages-per-batch', metavar='N', type=int, default=2000,
                    dest='batchsize',
                    help='number of pages per output file (default: 2000)')
args = parser.parse_args()

# Collect input PostScript files
psfiles = glob.glob(os.path.join(args.directory, '*.prn'))
psfiles.sort(reverse=True) # We need to process in reverse order

stmtno = 1  # Statement number
batchno = 1 # Batch number
pageno = 1  # Page in batch

letter = fitz.paper_rect('letter')
doc = None

for psfile in psfiles:
    basename = os.path.basename(psfile)
    print(basename)
    dirname = os.path.splitext(basename)[0]
    os.makedirs(dirname, exist_ok=True)
    subprocess.run(['gs', '-sDEVICE=pdfwrite', '-dNOPAUSE', '-dBATCH', '-dSAFER',
        '-sOutputFile=' + os.path.join(dirname, 'page%05d.pdf'), psfile])
    
    pdf_files = glob.glob(os.path.join(dirname, '*.pdf'))
    pdf_files.sort(reverse=True)

    for pdf_file in pdf_files:
        if doc is None:
            doc = fitz.open()
        src = fitz.open(pdf_file)
        try:
            page = doc.new_page(width=letter.width, height=letter.height)
            page.show_pdf_page(page.rect, src, 0, clip=page.rect)
            page.insert_text(fitz.Point(530, 19), '{:02d} {:04d} {:06d}'.format(batchno, pageno, stmtno), fontsize=9)
            if page.search_for('Delinquent Real Estate Statements are due'):
                page.insert_text(fitz.Point(40, 176), '*', fontname='helvetica-bold', fontsize=18)
        finally:
            src.close()
            src = None
        stmtno += 1
        if pageno == args.batchsize:
            doc.save(str(batchno) + '.pdf')
            doc.close()
            doc = None
            batchno += 1
            pageno = 1
            continue
        pageno += 1

if doc is not None:
    doc.save(str(batchno) + '.pdf')
    doc.close()
