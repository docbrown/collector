import argparse
import csv
from dataclasses import dataclass
import sys
from typing import Union
import cv2
import numpy as np
import pytesseract
from pprint import pprint
import re
import os

WINDOWS_TESSERACT_CMD = 'C:\\Program Files\\Tesseract-OCR\\tesseract.exe'
if os.path.exists(WINDOWS_TESSERACT_CMD):
    pytesseract.pytesseract.tesseract_cmd = WINDOWS_TESSERACT_CMD

TESSERACT_CHAR_ALLOWLIST = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-.,& '

@dataclass
class Label:
    image: cv2.Mat
    rectangle: np.ndarray
    def x(self) -> int:
        return int(self.rectangle[0][0])
    def y(self) -> int:
        return int(self.rectangle[0][1])

@dataclass
class Series:
    start_year: int
    end_year: int
    description: str

@dataclass
class Roll:
    number: int
    series: list[Series]

def parse_label(text: str) -> Roll:
    roll_number = 0
    years = []
    description_lines = []
    series_list = []
    text = text.strip()

    # Split into lines, removing empty ones
    lines = [line for line in (line.strip() for line in text.splitlines()) if line]

    # Find the roll number.
    i = 0
    while i < len(lines):
        line = lines[i]
        i += 1
        m = re.search('(\d+) BENTON', line)
        if m:
            roll_number = int(m.group(1))
            break
    else:
        # We couldn't find the roll number, so start from the top
        # and just parse what we can.
        i = 0

    def finish_series():
        nonlocal years
        nonlocal description_lines
        nonlocal series_list
        if len(years) == 0 and len(description_lines) == 0:
            return
        desc = ' '.join(description_lines)
        desc = desc.replace('REAL\nEST', 'REAL EST')
        desc = desc.replace('PERSONAL\nPROP', 'PERSONAL PROP')
        for series in expand_series((years, desc)):
            series_list.append(Series(series[0][0], series[0][1], series[1]))
        years = []
        description_lines = []
    
    def fix_year(year: str) -> str:
        # Tesseract sometimes misreads a '1' as an 'L'
        year = year.replace('L', '1')

        # Try to convert to an integer.
        year_int = 0
        try:
            year_int = int(year)
        except:
            # We can't do anything further.
            return year

        # If the first digit isn't recognized, we can make a guess.
        if year_int < 1000:
            if year_int > 50:
                year_int += 1000
            else:
                year_int += 2000

        return str(year_int)

    # i now contains the index of the first series line.
    # Attempt to parse each series.
    while i < len(lines):
        line = lines[i]
        parts = line.split(' ', 1)
        for ch in parts[0]:
            if ch.isdigit():
                # We have something that looks like a year, so start a new series.
                finish_series()
                # Parse the year range.
                years = [fix_year(year) for year in parts[0].split('-', 1)]
                if len(years) == 1:
                    years.append(years[0])
                # Add the description if we have one.
                if len(parts) > 1:
                    description_lines.append(parts[1])
                break
        else:
            # This doesn't look like the start of a new series, so
            # append this line to the current series.
            description_lines.append(line)
        i += 1
    
    # We've reached the end of the label, so finalize the current series.
    finish_series()

    return Roll(roll_number, series_list)

def expand_series(series):
    items = series[1].split(',')
    if len(items) == 1:
        return [series]
    series_list = []
    for item in items:
        series_list.append((series[0], item.strip()))
    return series_list

def extract_labels(filename, debug=False) -> list[Label]:
    # Load the image
    image = cv2.imread(filename)

    debug_prefix = ''
    if debug:
        base = os.path.basename(filename)
        root, _ = os.path.splitext(base)
        debug_prefix = os.path.join(os.path.abspath('.'), root)

    def save(img: cv2.Mat, suffix: str):
        if not debug:
            return
        cv2.imwrite(debug_prefix + suffix + '.jpg', img)

    # Step 1:
    # Extract the pink DUPLICATE stamps into a mask that can be used
    # to remove them from the image. In the HSV color space, this is
    # a simple range operation.
    hsv_image = cv2.cvtColor(image, cv2.COLOR_BGR2HSV)
    hsv_lower = np.array([130,50,100])
    hsv_upper = np.array([180,255,255])
    stamp_mask = cv2.inRange(hsv_image, hsv_lower, hsv_upper)
    save(stamp_mask, '_step1_stamps')

    # Step 2:
    # Convert to grayscale and remove the pink DUPLICATE stamps by
    # applying the mask we created previously.
    gray_image = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
    gray_image = cv2.add(gray_image, stamp_mask)
    save(gray_image, '_step2_grayscale')

    # Step 3:
    # Apply thresholding to get a binary back and white image.
    # This is required for contour detection, but also improves
    # the accuracy of OCR later on.
    _, binary_image = cv2.threshold(gray_image, 0, 255, cv2.THRESH_BINARY + cv2.THRESH_OTSU)
    save(binary_image, '_step3_threshold')

    # Step 4:
    # Find rectangular contours that are large enough to be a label.
    rectangles = []
    candidates = []
    contours, _ = cv2.findContours(binary_image, cv2.RETR_LIST, cv2.CHAIN_APPROX_SIMPLE)
    for i in range(len(contours)):
        contour = contours[i]
        if cv2.contourArea(contour) > 10_000:
            candidates.append(contour)
            # Simplify the contour
            peri = cv2.arcLength(contour, True)
            approx = cv2.approxPolyDP(contour, 0.05 * peri, True)
            # Only take contours with four points
            if len(approx) == 4:
                rectangles.append(approx.reshape(4, 2))
    if debug:
        contour_image = np.ones_like(binary_image)
        contour_image = cv2.cvtColor(contour_image, cv2.COLOR_GRAY2BGR)
        cv2.drawContours(contour_image, contours, -1, (0, 255, 0), 2, cv2.LINE_AA)
        cv2.drawContours(contour_image, candidates, -1, (0, 0, 255), 2, cv2.LINE_AA)
        cv2.drawContours(contour_image, rectangles, -1, (255, 0, 0), 2, cv2.LINE_AA)
        save(contour_image, '_step4_contours')

    # Step 5:
    # Apply perspective correction to straighten the labels.
    labels: list[Label] = []
    for rectangle in rectangles:
        label_image, rectangle = four_point_transform(binary_image, rectangle)
        labels.append(Label(label_image, rectangle))
    if debug:
        corrected_image = np.ones_like(binary_image)
        for label in labels:
            x = label.x()
            y = label.y()
            corrected_image[y:y+label.image.shape[0], x:x+label.image.shape[1]] = label.image
        save(corrected_image, '_step5_corrected')
    
    return labels

def order_points(points: np.ndarray) -> np.ndarray:
    # Initialize a list of coordinates that will be ordered
    # such that the first entry in the list is the top-left,
    # the second entry is the top-right, the third is the
    # bottom-right, and the fourth is the bottom-left.
    rectangle = np.zeros((4, 2), dtype = "float32")
    # The top-left point will have the smallest sum, whereas
    # the bottom-right point will have the largest sum.
    s = points.sum(axis = 1)
    rectangle[0] = points[np.argmin(s)]
    rectangle[2] = points[np.argmax(s)]
    # Now, compute the difference between the points, the
    # top-right point will have the smallest difference,
    # whereas the bottom-left will have the largest difference.
    diff = np.diff(points, axis = 1)
    rectangle[1] = points[np.argmin(diff)]
    rectangle[3] = points[np.argmax(diff)]
    # Return the ordered coordinates.
    return rectangle

def four_point_transform(image: cv2.Mat, points: np.ndarray) -> tuple[cv2.Mat, np.ndarray]:
    # Obtain a consistent order of the points and unpack them.
    rectangle = order_points(points)
    (tl, tr, br, bl) = rectangle
    # Compute the width of the new image, which will be the
    # maximum distance between bottom-right and bottom-left
    # x-coordiates or the top-right and top-left x-coordinates.
    width_a = np.sqrt(((br[0] - bl[0]) ** 2) + ((br[1] - bl[1]) ** 2))
    width_b = np.sqrt(((tr[0] - tl[0]) ** 2) + ((tr[1] - tl[1]) ** 2))
    max_width = max(int(width_a), int(width_b))
    # Compute the height of the new image, which will be the
    # maximum distance between the top-right and bottom-right
    # y-coordinates or the top-left and bottom-left y-coordinates.
    height_a = np.sqrt(((tr[0] - br[0]) ** 2) + ((tr[1] - br[1]) ** 2))
    height_b = np.sqrt(((tl[0] - bl[0]) ** 2) + ((tl[1] - bl[1]) ** 2))
    max_height = max(int(height_a), int(height_b))
    # Now that we have the dimensions of the new image, construct
    # the set of destination points to obtain a "birds eye view",
    # (i.e. top-down view) of the image, again specifying points
    # in the top-left, top-right, bottom-right, and bottom-left
    # order.
    dst = np.array([
        [0, 0],
        [max_width - 1, 0],
        [max_width - 1, max_height - 1],
        [0, max_height - 1]], dtype = "float32")
    # compute the perspective transform matrix and then apply it
    M = cv2.getPerspectiveTransform(rectangle, dst)
    warped = cv2.warpPerspective(image, M, (max_width, max_height))
    # return the warped image
    return warped, rectangle

if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('filename')
    parser.add_argument('-d', '--debug', action='store_true', help='Enable debugging output')
    args = parser.parse_args()

    writer = csv.writer(sys.stdout)
    writer.writerow(['ROLL', 'START', 'END', 'DESCRIPTION'])

    rolls = []

    labels = extract_labels(args.filename, debug=args.debug)
    for i, label in enumerate(labels):
        text = pytesseract.image_to_string(label.image, config='-c tessedit_char_whitelist="'+TESSERACT_CHAR_ALLOWLIST+'"')
        if args.debug:
            sys.stderr.write('* Label {} ({}, {}) *************************\n'.format(i + 1, label.x(), label.y()))
            sys.stderr.write(text.strip())
            sys.stderr.write('\n')
        rolls.append(parse_label(text))

    rolls.sort(key=lambda roll: roll.number)

    for roll in rolls:
        for series in roll.series:
            writer.writerow([roll.number, series.start_year, series.end_year, series.description])
        
