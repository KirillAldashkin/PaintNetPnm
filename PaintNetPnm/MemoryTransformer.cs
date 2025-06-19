namespace PaintNetPnm;

// TODO: try using SIMD
internal unsafe static class MemoryTransformer
{
    public static void InPlace__A__To__A_A_A_255(byte* data, nuint size)
    {
        var to = (uint*)data;

        // go backwards since we're using the same buffer
        do
        {
            uint val = data[--size];
            if (BitConverter.IsLittleEndian)
                to[size] = 0xFF000000 | val << 16 | val << 8 | val;
            else
                to[size] = val << 24 | val << 16 | val << 8 | 0xFF;
        } while (size != 0);
    }

    public static void InPlace__A_B_C__To__B_C_A_255(byte* data, nuint size)
    {
        var to = (uint*)data;

        // go backwards since we're using the same buffer
        do
        {
            --size;
            if (BitConverter.IsLittleEndian)
                to[size] = (uint)(0xFF000000 | data[size * 3] << 16 | data[size * 3 + 1] << 8 | data[size * 3 + 2]);
            else
                to[size] = (uint)(data[size * 3 + 2] << 24 | data[size * 3 + 1] << 16 | data[size * 3] << 8 | 0xFF);
        } while (size != 0);
    }

    public static void InPlace__A_B_C_D__To__A(byte* data, nuint size)
    {
        for (nuint i = 1; i < size; ++i)
            data[i] = data[4 * i];
    }

    public static void InPlace__A_B_C_D__To__B_C_A(byte* data, nuint size)
    {
        for (nuint i = 0; i < size; i++)
        {
            if (BitConverter.IsLittleEndian)
            {
                data[3 * i + 0] = data[4 * i + 2];
                data[3 * i + 1] = data[4 * i + 1];
                data[3 * i + 2] = data[4 * i + 0];
            }
            else
            {
                data[3 * i + 0] = data[4 * i + 0];
                data[3 * i + 1] = data[4 * i + 1];
                data[3 * i + 2] = data[4 * i + 2];
            }
        }
    }
}