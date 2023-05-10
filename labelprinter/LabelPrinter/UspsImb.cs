using System;
using System.Runtime.InteropServices;
using System.Text;

namespace LabelPrinter;

internal static class UspsImb
{
    private const int USPS_FSB_ENCODER_API_SUCCESS = 0;
    private const int USPS_FSB_ENCODER_API_SELFTEST_FAILED = 1;
    private const int USPS_FSB_ENCODER_API_BAR_STRING_IS_NULL = 2;
    private const int USPS_FSB_ENCODER_API_BYTE_CONVERSION_FAILED = 3;
    private const int USPS_FSB_ENCODER_API_RETRIEVE_TABLE_FAILED = 4;
    private const int USPS_FSB_ENCODER_API_CODEWORD_CONVERSION_FAILED = 5;
    private const int USPS_FSB_ENCODER_API_CHARACTER_RANGE_ERROR = 6;
    private const int USPS_FSB_ENCODER_API_TRACK_STRING_IS_NULL = 7;
    private const int USPS_FSB_ENCODER_API_ROUTE_STRING_IS_NULL = 8;
    private const int USPS_FSB_ENCODER_API_TRACK_STRING_BAD_LENGTH = 9;
    private const int USPS_FSB_ENCODER_API_TRACK_STRING_HAS_INVALID_DATA = 10;
    private const int USPS_FSB_ENCODER_API_TRACK_STRING_HAS_INVALID_DIGIT2 = 11;
    private const int USPS_FSB_ENCODER_API_ROUTE_STRING_BAD_LENGTH = 12;
    private const int USPS_FSB_ENCODER_API_ROUTE_STRING_HAS_INVALID_DATA = 13;

    [DllImport("usps4cb_windows_x64")]
    private static unsafe extern int USPS4CB(
        [MarshalAs(UnmanagedType.LPStr), In] string TrackPtr, // Buffer of 20 chars + null char
        [MarshalAs(UnmanagedType.LPStr), In] string RoutePtr, // Buffer of 11 chars + null char
        byte* BarPtr); // Buffer of 65 chars + null char

    public static string Encode(string trackingCode, string? routingCode = null)
    {
        ArgumentNullException.ThrowIfNull(trackingCode);
        if (routingCode is null)
        {
            routingCode = "";
        }

        int result;
        byte[] bars = new byte[66];
        Array.Fill(bars, (byte)' ', 0, 65);
        bars[65] = 0;

        unsafe
        {
            fixed (byte* bytes = &bars[0])
            {
                result = USPS4CB(trackingCode, routingCode, bytes);
            }
        }

        return result switch
        {
            USPS_FSB_ENCODER_API_SUCCESS => Encoding.ASCII.GetString(bars, 0, 65),
            USPS_FSB_ENCODER_API_TRACK_STRING_BAD_LENGTH =>
                throw new ArgumentException("Tracking Code must have 20 digits", nameof(trackingCode)),
            USPS_FSB_ENCODER_API_TRACK_STRING_HAS_INVALID_DATA =>
                throw new ArgumentException("Tracking Code must contain digits 0-9", nameof(trackingCode)),
            USPS_FSB_ENCODER_API_TRACK_STRING_HAS_INVALID_DIGIT2 =>
                throw new ArgumentException("Tracking Code second digit must be 0-4", nameof(trackingCode)),
            USPS_FSB_ENCODER_API_ROUTE_STRING_BAD_LENGTH =>
                throw new ArgumentException("Routing Code must be 0, 5, 9, or 11 digits", nameof(routingCode)),
            USPS_FSB_ENCODER_API_ROUTE_STRING_HAS_INVALID_DATA =>
                throw new ArgumentException("Routing Code must contain digits 0-9", nameof(routingCode)),
            _ => throw new Exception($"USPS4CB returned error code {result}."),
        };
    }
}
