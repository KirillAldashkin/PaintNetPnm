using PaintDotNet;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace PaintNetPnm;

internal static class PnmDecoder
{
    public static bool TryRead(Stream from, [NotNullWhen(true)] out Surface? result, [NotNullWhen(false)] out string? error)
    {
        Span<byte> header = stackalloc byte[3];
        if (!from.TryReadExactly(header))
        {
            result = null;
            error = "Could not read header";
            return false;
        }

        if (header.SequenceEqual("P5\n"u8)) return TryReadP5(from, out result, out error);
        if (header.SequenceEqual("P6\n"u8)) return TryReadP6(from, out result, out error);
        result = null;
        error = $"Invalid PNM header: \"{Encoding.UTF8.GetString(header)}\"";
        return false;
    }

    private static bool TryReadHeader(Stream from, out int width, out int height, out byte max)
    {
        if (!from.TryParseAscii((byte)' ', out width))
        {
            height = default;
            max = default;
            return false;
        }
        if (!from.TryParseAscii((byte)'\n', out height))
        {
            max = default;
            return false;
        }
        return from.TryParseAscii((byte)'\n', out max);
    }

    private static unsafe bool TryReadP5(Stream from, [NotNullWhen(true)] out Surface? result, [NotNullWhen(false)] out string? error)
    {
        if(!TryReadHeader(from, out var width, out var height, out var max))
            return InvalidHeader(out result, out error);

        bool dispose = true;
        var surface = new Surface(width, height);
        try
        {
            for (var y = 0; y < height; y++)
            {
                var buf = (byte*)surface.GetRowPointerUnchecked(y);
                if (!from.TryReadExactly(new Span<byte>(buf, width)))
                    return EofData(out result, out error);

                MemoryTransformer.InPlace__A__To__A_A_A_255(buf, (nuint)width);
            }

            dispose = false;
            error = null;
            result = surface;
            return true;
        }
        finally
        {
            if (dispose) surface.Dispose();
        }
    }

    private static unsafe bool TryReadP6(Stream from, [NotNullWhen(true)] out Surface? result, [NotNullWhen(false)] out string? error)
    {
        if (!TryReadHeader(from, out var width, out var height, out var max))
            return InvalidHeader(out result, out error);

        bool dispose = true;
        var surface = new Surface(width, height);
        try
        {
            for (var y = 0; y < height; y++)
            {
                var buf = (byte*)surface.GetRowPointerUnchecked(y);
                if (!from.TryReadExactly(new Span<byte>(buf, width * 3))) 
                    return EofData(out result, out error);

                MemoryTransformer.InPlace__A_B_C__To__B_C_A_255(buf, (nuint)width);
            }

            dispose = false;
            result = surface;
            error = null;
            return true;
        }
        finally
        {
            if (dispose) surface.Dispose();
        }
    }

    private static bool InvalidHeader(out Surface? result, out string error)
    {
        result = null;
        error = "Invalid PNM header";
        return false;
    }

    private static bool EofData(out Surface? result, out string error)
    {
        result = null;
        error = "Unexpected end of stream when reading PNM data";
        return false;
    }
}
