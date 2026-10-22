using System.Text;

namespace ProgrammingLabs.Experiments;

internal static class DesCipher
{
    private static readonly int[] IpTable =
    {
        58,50,42,34,26,18,10,2,60,52,44,36,28,20,12,4,
        62,54,46,38,30,22,14,6,64,56,48,40,32,24,16,8,
        57,49,41,33,25,17,9,1,59,51,43,35,27,19,11,3,
        61,53,45,37,29,21,13,5,63,55,47,39,31,23,15,7
    };

    private static readonly int[] IpReverseTable =
    {
        40,8,48,16,56,24,64,32,39,7,47,15,55,23,63,31,
        38,6,46,14,54,22,62,30,37,5,45,13,53,21,61,29,
        36,4,44,12,52,20,60,28,35,3,43,11,51,19,59,27,
        34,2,42,10,50,18,58,26,33,1,41,9,49,17,57,25
    };

    private static readonly int[] ETable =
    {
        32,1,2,3,4,5,4,5,6,7,8,9,
        8,9,10,11,12,13,12,13,14,15,16,17,
        16,17,18,19,20,21,20,21,22,23,24,25,
        24,25,26,27,28,29,28,29,30,31,32,1
    };

    private static readonly int[] PTable =
    {
        16,7,20,21,29,12,28,17,1,15,23,26,5,18,31,10,
        2,8,24,14,32,27,3,9,19,13,30,6,22,11,4,25
    };

    private static readonly int[] Pc1Table =
    {
        57,49,41,33,25,17,9,1,58,50,42,34,26,18,
        10,2,59,51,43,35,27,19,11,3,60,52,44,36,
        63,55,47,39,31,23,15,7,62,54,46,38,30,22,
        14,6,61,53,45,37,29,21,13,5,28,20,12,4
    };

    private static readonly int[] Pc2Table =
    {
        14,17,11,24,1,5,3,28,15,6,21,10,
        23,19,12,4,26,8,16,7,27,20,13,2,
        41,52,31,37,47,55,30,40,51,45,33,48,
        44,49,39,56,34,53,46,42,50,36,29,32
    };

    private static readonly int[] LoopTable = { 1,1,2,2,2,2,2,2,1,2,2,2,2,2,2,1 };

    private static readonly int[,,] SBoxTable = new int[8, 4, 16]
    {
        {
            { 14,4,13,1,2,15,11,8,3,10,6,12,5,9,0,7 },
            { 0,15,7,4,14,2,13,1,10,6,12,11,9,5,3,8 },
            { 4,1,14,8,13,6,2,11,15,12,9,7,3,10,5,0 },
            { 15,12,8,2,4,9,1,7,5,11,3,14,10,0,6,13 }
        },
        {
            { 15,1,8,14,6,11,3,4,9,7,2,13,12,0,5,10 },
            { 3,13,4,7,15,2,8,14,12,0,1,10,6,9,11,5 },
            { 0,14,7,11,10,4,13,1,5,8,12,6,9,3,2,15 },
            { 13,8,10,1,3,15,4,2,11,6,7,12,0,5,14,9 }
        },
        {
            { 10,0,9,14,6,3,15,5,1,13,12,7,11,4,2,8 },
            { 13,7,0,9,3,4,6,10,2,8,5,14,12,11,15,1 },
            { 13,6,4,9,8,15,3,0,11,1,2,12,5,10,14,7 },
            { 1,10,13,0,6,9,8,7,4,15,14,3,11,5,2,12 }
        },
        {
            { 7,13,14,3,0,6,9,10,1,2,8,5,11,12,4,15 },
            { 13,8,11,5,6,15,0,3,4,7,2,12,1,10,14,9 },
            { 10,6,9,0,12,11,7,13,15,1,3,14,5,2,8,4 },
            { 3,15,0,6,10,1,13,8,9,4,5,11,12,7,2,14 }
        },
        {
            { 2,12,4,1,7,10,11,6,8,5,3,15,13,0,14,9 },
            { 14,11,2,12,4,7,13,1,5,0,15,10,3,9,8,6 },
            { 4,2,1,11,10,13,7,8,15,9,12,5,6,3,0,14 },
            { 11,8,12,7,1,14,2,13,6,15,0,9,10,4,5,3 }
        },
        {
            { 12,1,10,15,9,2,6,8,0,13,3,4,14,7,5,11 },
            { 10,15,4,2,7,12,9,5,6,1,13,14,0,11,3,8 },
            { 9,14,15,5,2,8,12,3,7,0,4,10,1,13,11,6 },
            { 4,3,2,12,9,5,15,10,11,14,1,7,6,0,8,13 }
        },
        {
            { 4,11,2,14,15,0,8,13,3,12,9,7,5,10,6,1 },
            { 13,0,11,7,4,9,1,10,14,3,5,12,2,15,8,6 },
            { 1,4,11,13,12,3,7,14,10,15,6,8,0,5,9,2 },
            { 6,11,13,8,1,4,10,7,9,5,0,15,14,2,3,12 }
        },
        {
            { 13,2,8,4,6,15,11,1,10,9,3,14,5,0,12,7 },
            { 1,15,13,8,10,3,7,4,12,5,6,11,0,14,9,2 },
            { 7,11,4,1,9,12,14,2,0,6,10,13,15,3,5,8 },
            { 2,1,14,7,4,10,8,13,15,12,9,0,3,5,6,11 }
        }
    };

    public static string Encrypt(string key, string plaintext)
    {
        return EncryptBytes(ToFixedBytes(key), ToFixedBytes(plaintext));
    }

    public static string EncryptBytes(byte[] key, byte[] plaintext)
    {
        bool[][] roundKeys = GenerateRoundKeys(FixedBytes(key));
        bool[] bits = Permute(BytesToBits(FixedBytes(plaintext)), IpTable, 64);

        bool[] left = bits[..32];
        bool[] right = bits[32..];

        for (int round = 0; round < 16; round++)
        {
            bool[] previousRight = (bool[])right.Clone();
            right = Feistel(right, roundKeys[round]);
            Xor(right, left);
            left = previousRight;
        }

        var preOutput = new bool[64];
        Array.Copy(right, 0, preOutput, 0, 32);
        Array.Copy(left, 0, preOutput, 32, 32);
        return BitsToHex(Permute(preOutput, IpReverseTable, 64));
    }

    private static bool[][] GenerateRoundKeys(byte[] keyBytes)
    {
        bool[] bits = Permute(BytesToBits(keyBytes), Pc1Table, 56);
        bool[] c = bits[..28];
        bool[] d = bits[28..];
        var keys = new bool[16][];

        for (int round = 0; round < 16; round++)
        {
            c = RotateLeft(c, LoopTable[round]);
            d = RotateLeft(d, LoopTable[round]);

            var combined = new bool[56];
            Array.Copy(c, 0, combined, 0, 28);
            Array.Copy(d, 0, combined, 28, 28);
            keys[round] = Permute(combined, Pc2Table, 48);
        }

        return keys;
    }

    private static bool[] Feistel(bool[] right, bool[] roundKey)
    {
        bool[] expanded = Permute(right, ETable, 48);
        Xor(expanded, roundKey);
        bool[] substituted = Substitute(expanded);
        return Permute(substituted, PTable, 32);
    }

    private static bool[] Substitute(bool[] input)
    {
        var output = new bool[32];
        for (int box = 0; box < 8; box++)
        {
            int offset = 6 * box;
            int row = (input[offset] ? 2 : 0) + (input[offset + 5] ? 1 : 0);
            int column =
                (input[offset + 1] ? 8 : 0) +
                (input[offset + 2] ? 4 : 0) +
                (input[offset + 3] ? 2 : 0) +
                (input[offset + 4] ? 1 : 0);

            int value = SBoxTable[box, row, column];
            for (int bit = 0; bit < 4; bit++)
                output[4 * box + bit] = ((value << bit) & 0x8) != 0;
        }
        return output;
    }

    private static bool[] Permute(bool[] input, int[] table, int length)
    {
        var output = new bool[length];
        for (int i = 0; i < length; i++)
            output[i] = input[table[i] - 1];
        return output;
    }

    private static void Xor(bool[] target, bool[] mask)
    {
        for (int i = 0; i < target.Length; i++)
            target[i] ^= mask[i];
    }

    private static bool[] RotateLeft(bool[] input, int count)
    {
        var output = new bool[input.Length];
        for (int i = 0; i < input.Length; i++)
            output[i] = input[(i + count) % input.Length];
        return output;
    }

    private static bool[] BytesToBits(byte[] bytes)
    {
        var bits = new bool[64];
        for (int i = 0; i < 64; i++)
            bits[i] = ((bytes[i / 8] << (i % 8)) & 0x80) != 0;
        return bits;
    }

    private static string BitsToHex(bool[] bits)
    {
        const string digits = "0123456789abcdef";
        var result = new char[16];
        for (int i = 0; i < 16; i++)
        {
            int value =
                (bits[4 * i] ? 8 : 0) +
                (bits[4 * i + 1] ? 4 : 0) +
                (bits[4 * i + 2] ? 2 : 0) +
                (bits[4 * i + 3] ? 1 : 0);
            result[i] = digits[value];
        }
        return new string(result);
    }

    private static byte[] ToFixedBytes(string text)
    {
        return FixedBytes(Encoding.ASCII.GetBytes(text));
    }

    private static byte[] FixedBytes(byte[] source)
    {
        byte[] bytes = new byte[8];
        Array.Copy(source, bytes, Math.Min(8, source.Length));
        return bytes;
    }
}