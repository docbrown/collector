using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace LabelPrinter;

internal class SlpApiException : Exception
{
    public int ErrorCode { get; private set; }

    public SlpApiException()
    {
    }

    public SlpApiException(int errorCode, string? message = null) : base(message)
    {
        ErrorCode = errorCode;
    }

    public SlpApiException(string? message) : base(message)
    {
    }

    public SlpApiException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

internal static class SlpApi
{
    [DllImport("SlpApi7x64/SlpApi7x64.dll", EntryPoint = "SlpOpenPrinter", CharSet = CharSet.Unicode)]
    public static extern bool _SlpOpenPrinter(string strPrinterName, int nID, bool fPortrait);
    public static void SlpOpenPrinter(string strPrinterName, int nID, bool fPortrait)
    {
        if (!_SlpOpenPrinter(strPrinterName, nID, fPortrait))
        {
            throw new SlpApiException(SlpGetErrorCode());
        }
    }

    [DllImport("SlpApi7x64/SlpApi7x64.dll", EntryPoint = "SlpCreateFont", CharSet = CharSet.Unicode)]
    public static extern IntPtr _SlpCreateFont(string lpName, int nPoints, int nAttributes);
    public static IntPtr SlpCreateFont(string lpName, int nPoints, int nAttributes)
    {
        IntPtr font = _SlpCreateFont(lpName, nPoints, nAttributes);
        if (font == IntPtr.Zero)
        {
            throw new SlpApiException(SlpGetErrorCode());
        }
        return font;
    }

    [DllImport("SlpApi7x64/SlpApi7x64.dll", EntryPoint = "SlpGetErrorCode")]
    public static extern int SlpGetErrorCode();

    [DllImport("SlpApi7x64/SlpApi7x64.dll", EntryPoint = "SlpDeleteFont")]
    public static extern bool SlpDeleteFont(IntPtr iFont);

    [DllImport("SlpApi7x64/SlpApi7x64.dll", EntryPoint = "SlpGetTextWidth")]
    public static extern int SlpGetTextWidth(int iFont, string lpText);

    [DllImport("SlpApi7x64/SlpApi7x64.dll", EntryPoint = "SlpGetLabelHeight")]
    public static extern int SlpGetLabelHeight();

    [DllImport("SlpApi7x64/SlpApi7x64.dll", EntryPoint = "SlpGetLabelWidth")]
    public static extern int SlpGetLabelWidth();

    [DllImport("SlpApi7x64/SlpApi7x64.dll", EntryPoint = "SlpStartLabel")]
    public static extern bool _SlpStartLabel();
    public static void SlpStartLabel()
    {
        if (!_SlpStartLabel())
        {
            throw new SlpApiException(SlpGetErrorCode());
        }
    }

    [DllImport("SlpApi7x64/SlpApi7x64.dll", EntryPoint = "SlpEndLabel")]
    public static extern bool SlpEndLabel();

    [DllImport("SlpApi7x64/SlpApi7x64.dll", EntryPoint = "SlpSetRotation")]
    public static extern void SlpSetRotation(int nAngle);

    [DllImport("SlpApi7x64/SlpApi7x64.dll", EntryPoint = "SlpDrawTextXY", CharSet = CharSet.Unicode)]
    public static extern void SlpDrawTextXY(int x, int y, IntPtr iFont, string lpText);

    [DllImport("SlpApi7x64/SlpApi7x64.dll", EntryPoint = "SlpClosePrinter")]
    public static extern void SlpClosePrinter();

    [DllImport("SlpApi7x64/SlpApi7x64.dll", EntryPoint = "SlpDrawRectangle")]
    public static extern void SlpDrawRectangle(int x, int y, int nWidth, int nHeight, int nThickness);

    [DllImport("SlpApi7x64/SlpApi7x64.dll", EntryPoint = "SlpDrawLine")]
    public static extern void SlpDrawLine(int xStart, int yStart, int xEnd, int yEnd, int nThickness);

    [DllImport("SlpApi7x64/SlpApi7x64.dll", EntryPoint = "SlpDrawPicture", CharSet = CharSet.Unicode)]
    public static extern int SlpDrawPicture(int nLeft, int nTop, int nRight, int nBottom, string strPath);

    [DllImport("SlpApi7x64/SlpApi7x64.dll", EntryPoint = "SlpDrawBarCode", CharSet = CharSet.Unicode)]
    public static extern void SlpDrawBarCode(int nLeft, int nTop, int nRight, int nBottom, string lpText);

    [DllImport("SlpApi7x64/SlpApi7x64.dll", EntryPoint = "SlpSetBarCodeStyle", CharSet = CharSet.Unicode)]
    public static extern void SlpSetBarCodeStyle(int nSymbology, int nRatio, int nMode, int nSecurity, int bReadableText, int nFontHeight, int nFontAttributes, string strFaceName);
    
    [DllImport("SlpApi7x64/SlpApi7x64.dll", EntryPoint = "SlpGetVersion", CharSet = CharSet.Unicode)]
    public static extern int SlpGetVersion(string lpText);
    
    [DllImport("SlpApi7x64/SlpApi7x64.dll", EntryPoint = "SlpFindPrinters")]
    public static extern int SlpFindPrinters(bool bAllPrinters);
    
    [DllImport("SlpApi7x64/SlpApi7x64.dll", EntryPoint = "SlpGetPrinterName", CharSet = CharSet.Unicode)]
    public static extern int SlpGetPrinterName(int nIndex, string strPrinterName, int nMaxChars);
    
    [DllImport("SlpApi7x64/SlpApi7x64.dll", EntryPoint = "SlpDebugMode")]
    public static extern void SlpDebugMode(int nMode);
    
    [DllImport("SlpApi7x64/SlpApi7x64.dll", EntryPoint = "SlpCopyLabelToClipboard")]
    public static extern void SlpCopyLabelToClipboard();

    public static void SlpCheckError()
    {
        int code = SlpGetErrorCode();
        if (code != 0)
        {
            throw new SlpApiException(code);
        }
    }

    public static void SlpThrowLastError()
    {
        throw new SlpApiException(SlpGetErrorCode());
    }

    public static Image? SlpRenderLabel()
    {
        try
        {
            IDataObject savedObject = Clipboard.GetDataObject();
            try
            {
                SlpCopyLabelToClipboard();
                SlpCheckError();
                return Clipboard.GetImage();
            }
            finally
            {
                Clipboard.SetDataObject(savedObject);
            }
        }
        catch (Exception)
        {
            return null;
        }
    }
}
