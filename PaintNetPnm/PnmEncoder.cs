using PaintDotNet;
using System.Buffers.Text;
using System.IO;

namespace PaintNetPnm;

internal static class PnmEncoder
{
    private static void WriteHeader(Stream to, ReadOnlySpan<byte> format, int width, int height)
    {
        Span<byte> header = stackalloc byte[64];
        format.CopyTo(header);
        var used = format.Length;
        Utf8Formatter.TryFormat(width, header[used..], out var curUsed);
        used += curUsed;
        header[used++] = (byte)' ';
        Utf8Formatter.TryFormat(height, header[used..], out curUsed);
        used += curUsed;
        "\n255\n"u8.CopyTo(header[used..]);
        used += 5;

        to.Write(header[..used]);
    }

    public static unsafe void WriteP6(Stream to, Surface data, Action<int, int> progress)
    {
        WriteHeader(to, "P6\n"u8, data.Width, data.Height);
        for (var y = 0; y < data.Height; y++)
        {
            progress(y, data.Height);
            var buf = (byte*)data.GetRowPointerUnchecked(y);
            MemoryTransformer.InPlace__A_B_C_D__To__B_C_A(buf, (nuint)data.Width);
            to.Write(new(buf, data.Width * 3));
        }
    }

    internal static unsafe void WriteP5(Stream to, Surface data, Channel channel, Action<int, int> progress)
    {
        var offset = channel switch
        {
            Channel.B => 0,
            Channel.G => 1,
            Channel.R => 2,
            Channel.A => 3,
            _ => throw new("impossible")
        };
        if (!BitConverter.IsLittleEndian) offset = 3 - offset;

        WriteHeader(to, "P5\n"u8, data.Width, data.Height);
        for (var y = 0; y < data.Height; y++)
        {
            progress(y, data.Height);
            var buf = offset + (byte*)data.GetRowPointerUnchecked(y);
            MemoryTransformer.InPlace__A_B_C_D__To__A(buf, (nuint)data.Width);
            to.Write(new(buf, data.Width));
        }
    }
}
