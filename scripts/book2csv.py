from collections import namedtuple
import csv
from decimal import Decimal
import fitz
import math
import re
import sys

Field = namedtuple('Field', ['name', 'start', 'length', 'transform'], defaults=[None, 0, 0, None])

def stripcomma(x):
    return x.replace(',', '')

def fixval(x):
    if x == '':
        return '0'
    return x.replace(',', '')

def fixtax(x):
    if x == '':
        return '0.00'
    elif x.startswith('.'):
        return '0' + x
    return x.replace(',', '')

def fixdist(x):
    if x == '':
        return 'NONE'
    return x

def collapsews(x):
    return re.sub(r'\s+', ' ', x)

REAL_FIELDS = [
    [
        Field('NAME', 16, 36, collapsews),
        Field('ADDRESS1', 53, 31, collapsews),
        Field('ADDRESS2', 85, 26, collapsews),
        Field('CITYSTZIP', 112, 0, collapsews)
    ],
    [
        Field('PARCELNO', 0, 25),
        Field('TOTVAL', 26, 41, fixval),
        Field('SCHLCODE', 73, 6, fixdist),
        Field('ROADCODE', 83, 6, fixdist),
        Field('CITYCODE', 93, 6, fixdist),
        Field('FIRECODE', 103, 6, fixdist),
        Field('TAG', 112)
    ],
    [], # Nothing useful on this line
    [
        Field('RESVAL', 9, 20, fixval),
        Field('AGRVAL', 42, 20, fixval),
        Field('COMVAL', 75, 20, fixval),
        Field('ACRES', 98, 8, stripcomma),
        Field('SEC', 107, 2),
        Field('TWN', 112, 2),
        Field('RNG', 115, 2),
        Field('LANDTYPE', 119, 2),
        Field('DEEDBKPG', 122)
    ],
    [ Field('DESCRIPTION', 0, 0, collapsews) ],
    [
        Field('SCHLTAX', 1, 12, fixtax),
        Field('ROADTAX', 14, 12, fixtax),
        Field('CITYTAX', 27, 12, fixtax),
        Field('FIRETAX', 40, 12, fixtax),
        Field('SURTAX', 53, 12, fixtax),
        Field('STATTAX', 66, 12, fixtax),
        Field('COUNTAX', 79, 12, fixtax),
        Field('LIBRTAX', 92, 12, fixtax),
        Field('HLTHTAX', 105, 12, fixtax),
        Field('SCSFTAX', 118, 12, fixtax)
    ],
    [
        Field('WAMBTAX', 1, 12, fixtax),
        Field('CAMBTAX', 14, 12, fixtax),
        Field('GSNHTAX', 27, 12, fixtax),
        Field('LCNHTAX', 40, 12, fixtax),
        Field('JUCOTAX', 53, 12, fixtax),
        Field('WIAMTAX', 66, 12, fixtax),
        Field('TOTLTAX', 116, 14, fixtax)
    ]
]
REAL_FIELDS_ORDER = ['YEAR', 'TYPE', 'SUBTYPE', 'PARCELNO', 'NAME', 'ADDRESS1', 'ADDRESS2', 'CITY', 'STATE', 'ZIP5', 'ZIP4',
    'DESCRIPTION', 'ACRES', 'SEC', 'TWN', 'RNG', 'LANDTYPE', 'DEEDBOOK', 'DEEDPAGE', 'TAG',
    'RESVAL', 'AGRVAL', 'COMVAL', 'TOTVAL', 'SCHLCODE', 'SCHLTAX', 'ROADCODE', 'ROADTAX', 'CITYCODE',
    'CITYTAX', 'FIRECODE', 'FIRETAX', 'AMBCODE', 'AMBTAX', 'NURSCODE', 'NURSTAX', 'SURTAX',
    'STATTAX', 'COUNTAX', 'LIBRTAX', 'HLTHTAX', 'SCSFTAX', 'JUCOTAX', 'TOTLTAX']

PERSONAL_FIELDS = [
    [
        Field('ACCOUNT',     0, 15),
        Field('NAME',       16, 36, collapsews),
        Field('ADDRESS1',   53, 31, collapsews),
        Field('ADDRESS2',   85, 26, collapsews),
        Field('CITYSTZIP', 112,  0, collapsews)
    ],
    [
        Field('TOTVAL',    10, 12, fixval),
        Field('SCHLCODE', 51,  6, fixdist),
        Field('ROADCODE', 60,  6, fixdist),
        Field('CITYCODE', 69,  6, fixdist),
        Field('FIRECODE', 78,  6, fixdist)
    ],
    [],
    [],
    [],
    [],
    [],
    [],
    [],
    [],
    [
        Field('SCHLTAX',   0, 12, fixtax),
        Field('ROADTAX',  12, 12, fixtax),
        Field('CITYTAX',  24, 12, fixtax),
        Field('FIRETAX',  36, 12, fixtax),
        Field('STATTAX',  48, 12, fixtax),
        Field('COUNTAX',  60, 12, fixtax),
        Field('LIBRTAX',  72, 12, fixtax),
        Field('HLTHTAX',  84, 12, fixtax),
        Field('SCSFTAX',  96, 12, fixtax),
        Field('WAMBTAX', 108, 12, fixtax)
    ],
    [
        Field('CAMBTAX',   0, 12, fixtax),
        Field('GSNHTAX',  12, 12, fixtax),
        Field('LCNHTAX',  24, 12, fixtax),
        Field('JUCOTAX',  36, 12, fixtax),
        Field('WIAMTAX',  48, 12, fixtax),
        Field('LATEFEE',  60, 12, fixtax),
        Field('TOTLTAX', 115, 15, fixtax)
    ]
]
PERSONAL_FIELDS_ORDER = ['YEAR', 'TYPE', 'SUBTYPE', 'ACCOUNT', 'NAME', 'ADDRESS1', 'ADDRESS2', 'CITY', 'STATE', 'ZIP5', 'ZIP4',
    'TOTVAL', 'SCHLCODE', 'SCHLTAX', 'ROADCODE', 'ROADTAX', 'CITYCODE',
    'CITYTAX', 'FIRECODE', 'FIRETAX', 'AMBCODE', 'AMBTAX', 'NURSCODE', 'NURSTAX',
    'STATTAX', 'COUNTAX', 'LIBRTAX', 'HLTHTAX', 'SCSFTAX', 'JUCOTAX', 'TOTLTAX']

def process_record(csvwriter, record, order):
    # Split the city, state, and zip code into separate fields
    citystzip = record['CITYSTZIP'].split(' ')
    del record['CITYSTZIP']
    record['CITY'] = ' '.join(citystzip[:-2])
    record['STATE'] = citystzip[-2]
    zipcode = citystzip[-1].split('-')
    record['ZIP5'] = zipcode[0]
    record['ZIP4'] = zipcode[1] if len(zipcode) > 1 and zipcode[1] != '0000' else ''

    # Split the deed book and page into separate fields.
    if 'DEEDBKPG' in record:
        deedbkpg = record['DEEDBKPG'].split('-')
        del record['DEEDBKPG']
        record['DEEDBOOK'] = deedbkpg[0].lstrip('0')
        record['DEEDPAGE'] = deedbkpg[1].lstrip('0')
    
    # The PDF reports break out each ambulance district even though an account
    # can only have one. Treat it the same as school, road, fire, etc.
    if 'WIAMTAX' in record:
        if record['WAMBTAX'] != '0.00':
            ambcode = 'WAMB'
            ambtax = record['WAMBTAX']
        elif record['CAMBTAX'] != '0.00':
            ambcode = 'CAMB'
            ambtax = record['CAMBTAX']
        elif record['WIAMTAX'] != '0.00':
            ambcode = 'WIAM'
            ambtax = record['WIAMTAX']
        else:
            ambcode = 'NONE'
            ambtax = '0.00'
        del record['WAMBTAX']
        del record['CAMBTAX']
        del record['WIAMTAX']
        record['AMBCODE'] = ambcode
        record['AMBTAX'] = ambtax
    
    # The PDF reports break out each nursing home district, even though an
    # account can only have one. Treat it the same as school, road, fire, etc.
    if 'GSNHTAX' in record:
        if record['GSNHTAX'] != '0.00':
            nurscode = 'GSNH'
            nurstax = record['GSNHTAX']
        elif record['LCNHTAX'] != '0.00':
            nurscode = 'LCNH'
            nurstax = record['LCNHTAX']
        else:
            nurscode = 'NONE'
            nurstax = '0.00'
        del record['GSNHTAX']
        del record['LCNHTAX']
        record['NURSCODE'] = nurscode
        record['NURSTAX'] = nurstax
    
    # Correct the FIRE7 code to be consistent with the other fire districts
    if 'FIRECODE' in record and record['FIRECODE'] == 'FIRE7':
        record['FIRECODE'] = 'FIR7'
    
    # The NEEDS MANUAL PROCESSING tag was renamed to REVIEW.
    if 'TAG' in record and record['TAG'].startswith('NEEDS MANUAL'):
        record['TAG'] = 'REVIEW'

    row = []
    for field in order:
        row.append(record[field])
    csvwriter.writerow(row)

def process_page(csvwriter, year, _type, subtype, lines, fields, order):
    record = None
    n = 0
    i = 0
    for line in lines:
        if line.startswith('***') or line.startswith('...'):
            if record:
                process_record(csvwriter, record, order)
                n += 1
            if line[0] == '*':
                break # end of page
            else:
                i = 0
                record = {
                    'YEAR': year,
                    'TYPE': _type,
                    'SUBTYPE': subtype
                }
                continue
        elif record is None:
            pass # skipping header lines
        elif i >= len(fields):
            pass # no more fields in this record
        else:
            for field in fields[i]:
                if field.length == 0:
                    value = line[field.start:]
                else:
                    value = line[field.start:field.start+field.length]
                value = value.strip()
                if field.transform is not None:
                    value = field.transform(value)
                record[field.name] = value
        i += 1
    return n

def read_book(doc, csvwriter):
    pageno = 1
    recordcnt = 0
    for page in doc:
        #print('Page ' + str(pageno))
        text = page.get_text()
        # PyMuPDF returns some garbage at the beginning and end of the
        # page and the end of each line, so just strip out everything
        # except printable ASCII characters.
        text = ''.join(c for c in text if (ord(c) > 31 and ord(c) < 127) or ord(c) == 10)
        lines = text.split('\n')
        if pageno == 1:
            year = int(lines[0][5:9])
            if 'REAL' in lines[0]:
                _type = 'REAL'
                fields = REAL_FIELDS
                order = REAL_FIELDS_ORDER
            elif 'PERSONAL' in lines[0]:
                _type = 'PERS'
                fields = PERSONAL_FIELDS
                order = PERSONAL_FIELDS_ORDER
            else:
                print('Unknown book type.')
                return
            subtype = 'CURR' if 'CURRENT' in lines[0] else 'DELQ'
            csvwriter.writerow(order)
        recordcnt += process_page(csvwriter, year, _type, subtype, lines, fields, order)
        pageno += 1
    # Calculate the cost of the data
    # Each page is 10 cents and a page has 32 records (the number that can fit
    # on an 8.5"-high sheet of paper in 12pt font with 0.75" margins).
    equivpgs = int(math.ceil(recordcnt / 32))
    cost = equivpgs * Decimal('0.10')
    sys.stderr.write('{0} total records\n{1} equivalent pages @ 10 cents/page = ${2}\n'.format(recordcnt, equivpgs, cost))

with fitz.open(sys.argv[1]) as doc:
    csvwriter = csv.writer(sys.stdout, quoting=csv.QUOTE_MINIMAL, lineterminator='\n')
    read_book(doc, csvwriter)
