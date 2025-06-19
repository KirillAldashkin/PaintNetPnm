using System.IO;

namespace PaintNetPnm;

internal static class StreamUtil
{
    private static bool Failed<T>(out T value)
    {
        value = default!;
        return false;
    }

    public static bool TryParseAscii(this Stream from, byte until, out int value)
    {
        if (!from.TryReadByte(out var sym) || sym == until) return Failed(out value);

        if (!Ascii.TryDigit(sym, out sym)) return Failed(out value);
        value = sym;

        while (true)
        {
            if (!from.TryReadByte(out sym)) return false;
            if (sym == until) break;
            if (!Ascii.TryDigit(sym, out sym)) return false;
            // value * 10 + sym > MaxValue
            //       value * 10 > MaxValue - sym
            //            value > (MaxValue - sym) / 10
            if (value > (int.MaxValue - sym) / 10) return false;
            value = value * 10 + sym;
        }

        return true;
    }

    public static bool TryParseAscii(this Stream from, byte until, out byte value)
    {
        if (!from.TryReadByte(out var sym) || sym == until) return Failed(out value);

        if (!Ascii.TryDigit(sym, out sym)) return Failed(out value);
        value = sym;

        while (true)
        {
            if (!from.TryReadByte(out sym)) return false;
            if (sym == until) break;
            if (!Ascii.TryDigit(sym, out sym)) return false;
            // value * 10 + sym > MaxValue
            //       value * 10 > MaxValue - sym
            //            value > (MaxValue - sym) / 10
            if (value > (byte.MaxValue - sym) / 10) return false;
            value = (byte)(value * 10 + sym);
        }

        return true;
    }

    public static bool TryReadExactly(this Stream from, Span<byte> buffer)
    {
        while (!buffer.IsEmpty)
        {
            var read = from.Read(buffer);
            if (read == 0) return false;
            buffer = buffer[read..];
        }
        return true;
    }

    public static bool TryReadByte(this Stream from, out byte value)
    {
        var val = from.ReadByte();
        value = (byte)val;
        return val != -1;
    }
}

internal static class Ascii
{
    public static bool TryDigit(byte sym, out byte digit)
    {
        digit = (byte)(sym - '0');
        return sym >= '0' && sym <= '9';
    }
}