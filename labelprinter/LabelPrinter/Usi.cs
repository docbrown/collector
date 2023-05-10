using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.Storage.Xps;

using static Windows.Win32.PInvoke;

namespace LabelPrinter;

internal static class Usi
{
    public enum RecordType
    {
        Unknown,
        Real,
        Personal,
        Merchant,
        MortgageCompany,
    }

    public struct Field
    {
        public Rectangle Rectangle;
        public string Name;
        public Field(Rectangle rectangle, string name)
        {
            Rectangle = rectangle;
            Name = name;
        }
    }

    public static readonly Field[] RealFields = new[]
    {
        new Field(new Rectangle(113, 57, 407, 15),  "NAME"),
        new Field(new Rectangle(113, 80, 407, 15),  "ADDRESS1"),
        new Field(new Rectangle(113, 103, 407, 15), "ADDRESS2"),
        new Field(new Rectangle(113, 126, 280, 15), "CITY"),
        new Field(new Rectangle(500, 126, 20, 15),  "ST"),
        new Field(new Rectangle(620, 126, 100, 15), "ZIP"),
    };

    private static readonly IDictionary<string, RecordType> WindowTitleToRecordTypeMap = new Dictionary<string, RecordType>
    {
        { "BENTON COUNTY COLLECTOR - INQUIRY", RecordType.Unknown },
        { "REAL BILL SCREEN", RecordType.Real },
        { "PERSONAL BILL SCREEN", RecordType.Personal },
        { "MERCHANT LICENSE MAINTENANCE", RecordType.Merchant },
        { "BENTON COUNTY - MORTGAGOR MAINTENANCE", RecordType.MortgageCompany }
    };

    private static unsafe string GetWindowTitle(HWND hWnd)
    {
        char[] title = new char[512];
        fixed (char* p = title)
        {
            SendMessage(hWnd, WM_GETTEXT, new WPARAM((nuint)title.Length), new LPARAM((nint)p));
            return new string(p);
        }
    }

    public static (HWND, RecordType) FindRecordWindow()
    {
        HWND hWndFound = HWND.Null;
        RecordType type = RecordType.Unknown;

        var processes = Process.GetProcessesByName("gdc");

        foreach (var process in processes)
        {
            foreach (ProcessThread thread in process.Threads)
            {
                EnumThreadWindows((uint)thread.Id, (HWND hWnd, LPARAM lParam) =>
                {
                    string title = GetWindowTitle(hWnd);
                    if (WindowTitleToRecordTypeMap.TryGetValue(title, out type))
                    {
                        hWndFound = hWnd;
                        return false;
                    }
                    return true;
                }, 0);
                if (hWndFound != HWND.Null)
                {
                    break;
                }
            }
            if (hWndFound != HWND.Null)
            {
                break;
            }
        }

        return (hWndFound, type);
    }

    public static Bitmap CaptureWindow(HWND hWnd)
    {
        GetClientRect(hWnd, out RECT clientRect);
        Bitmap bitmap = new(clientRect.Width, clientRect.Height);
        using (var g = Graphics.FromImage(bitmap))
        {
            PrintWindow(hWnd, new HDC(g.GetHdc()), PRINT_WINDOW_FLAGS.PW_CLIENTONLY);
        }
        return bitmap;
    }

    public static unsafe Bitmap Preprocess(Bitmap bitmap)
    {
        var scaled = new Bitmap(bitmap, new System.Drawing.Size(bitmap.Width * 2, bitmap.Height * 2));
        BitmapData data = scaled.LockBits(new Rectangle(0, 0, scaled.Width, scaled.Height),
            ImageLockMode.ReadOnly, bitmap.PixelFormat);
        try
        {
            using var image = SixLabors.ImageSharp.Image.WrapMemory<Rgba32>((void*)data.Scan0, bitmap.Width, bitmap.Height);
            image.Mutate(x =>
            {
                x.BinaryThreshold(0.6f);
            });
        }
        finally
        {
            scaled.UnlockBits(data);
        }
        return scaled;
    }

    public static Bitmap IsolateFields(Bitmap src, Field[] fields)
    {
        var dest = new Bitmap(src.Width * 2, src.Height * 2);
        using var g = Graphics.FromImage(dest);
        g.FillRectangle(SystemBrushes.Control, 0, 0, dest.Width, dest.Height);
        foreach (var field in fields)
        {
            var rect = field.Rectangle;
            var scaled = new Rectangle(rect.X * 2, rect.Y * 2, rect.Width * 2, rect.Height * 2);
            g.DrawImage(src, scaled, rect, GraphicsUnit.Pixel);
        }
        return dest;
    }

    public static void DrawTextRectangles(Bitmap bitmap, OcrResult result)
    {
        using var g = Graphics.FromImage(bitmap);
        using var pen = new Pen(Color.Red);
        foreach (var line in result.Lines)
        {
            foreach (var word in line.Words)
            {
                var rect = word.BoundingRect;
                g.DrawRectangle(pen, (int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
            }
        }
    }

    private static SoftwareBitmap ToSoftwareBitmap(Bitmap bitmap)
    {
        using var writer = new DataWriter();
        BitmapData? data = null;
        try
        {
            data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, bitmap.PixelFormat);
            var cb = data.Stride * data.Height;
            var bytes = new byte[cb];
            Marshal.Copy(data.Scan0, bytes, 0, cb);
            writer.WriteBytes(bytes);
        }
        finally
        {
            if (data != null)
            {
                bitmap.UnlockBits(data);
            }
        }

        var buffer = writer.DetachBuffer();

        var softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Rgba8, bitmap.Width, bitmap.Height);
        softwareBitmap.CopyFromBuffer(buffer);
        return softwareBitmap;
    }

    public static OcrResult RecognizeText(Bitmap bitmap)
    {
        using var softwareBitmap = ToSoftwareBitmap(bitmap);
        var engine = OcrEngine.TryCreateFromUserProfileLanguages();
        return engine.RecognizeAsync(softwareBitmap).AsTask().Result;
    }

    public static IDictionary<string, string> ExtractFields(OcrResult result, Field[] fields)
    {
        var dict = new Dictionary<string, string>(fields.Length);
        foreach (var line in result.Lines)
        {
            foreach (var word in line.Words)
            {
                var wordRect = word.BoundingRect;
                foreach (var field in fields)
                {
                    var scaledField = new Rectangle(
                        field.Rectangle.X * 2,
                        field.Rectangle.Y * 2,
                        field.Rectangle.Width * 2,
                        field.Rectangle.Height * 2);
                    if (scaledField.Contains((int)wordRect.X, (int)wordRect.Y))
                    {
                        if (dict.ContainsKey(field.Name))
                        {
                            dict[field.Name] += " " + word.Text;
                        }
                        else
                        {
                            dict[field.Name] = word.Text;
                        }
                    }
                }
            }
        }
        return dict;
    }

    public static void ShowDebugDialog()
    {
        var (hWnd, type) = FindRecordWindow();
        if (hWnd == HWND.Null)
        {
            return;
        }

        using var bitmap = CaptureWindow(hWnd);


    }

    public static DomesticAddress? GetCurrentRecordAddress()
    {
        // Find the current record window.
        var (hWnd, type) = FindRecordWindow();
        if (hWnd == HWND.Null)
        {
            return null;
        }

        // Take a screenshot of the window.
        using var bitmap = CaptureWindow(hWnd);

        // Preprocess the image to enhance OCR accuracy.
        using var preprocessedBitmap = Preprocess(bitmap);

        // Recognize text in the window.
        var ocrResult = RecognizeText(preprocessedBitmap);

        // If we couldn't determine the record type from the window title,
        // try to figure it out from the extracted text.
        if (type == RecordType.Unknown)
        {
            if (ocrResult.Text.Contains("REAL PROPERTY ACCOUNT DETAILS"))
            {
                type = RecordType.Real;
            }
            else if (ocrResult.Text.Contains("PERSONAL PROPERTY ACCOUNT DETAILS"))
            {
                type = RecordType.Personal;
            }
        }

        // Select a field layout for the current record screen.
        Field[] fields;
        switch (type)
        {
            case RecordType.Real:
                fields = RealFields;
                break;
            default:
                return null;
        }

        // Extract fields from the record screen.
        var dict = ExtractFields(ocrResult, fields);

        if (dict.Count > 0)
        {
            dict.TryGetValue("NAME", out string? name);
            dict.TryGetValue("ADDRESS1", out string? address1);
            dict.TryGetValue("ADDRESS2", out string? address2);
            dict.TryGetValue("CITY", out string? city);
            dict.TryGetValue("ST", out string? state);
            dict.TryGetValue("ZIP", out string? zip);
            
            var addr = new DomesticAddress
            {
                Name = name ?? "",
                Line1 = address1 ?? "",
                Line2 = address2 ?? "",
                City = city ?? "",
                State = state ?? "",
                FullZip = zip ?? ""
            };

            addr.Line1 = Regex.Replace(addr.Line1, "^0/0", "C/O");

            return addr;
        }

        return null;
    }
}
