
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace CapComm.Utilities
{



    public class KeyboardState
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern short GetKeyState(int keyCode);

        public static bool IsCapsLockOn()
        {
            return Convert.ToBoolean(GetKeyState(0x14) & 0x0001); // 0x14 = VK_CAPITAL
        }

        public static bool IsNumLockOn()
        {
            return Convert.ToBoolean(GetKeyState(0x90) & 0x0001); // 0x90 = VK_NUMLOCK
        }

        public static bool IsScrollLockOn()
        {
            return Convert.ToBoolean(GetKeyState(0x91) & 0x0001); // 0x91 = VK_SCROLL
        }
    }


    public class KeyActions
    {
        public static void SetCapsLock(bool capsLockOn)
        {

            // Check current state
            bool isCapsLockOn = KeyboardState.IsCapsLockOn();

            if (capsLockOn != isCapsLockOn)
            {

                // Import keybd_event function from user32.dll
                [DllImport("user32.dll", SetLastError = true)]
                static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

                // Constants for virtual key codes and event flags
                const int VK_CAPITAL = 0x14;
                const uint KEYEVENTF_EXTENDEDKEY = 0x1;
                const uint KEYEVENTF_KEYUP = 0x2;

                // Press Caps Lock key
                keybd_event((byte)VK_CAPITAL, 0x45, KEYEVENTF_EXTENDEDKEY, UIntPtr.Zero);

                // Release Caps Lock key
                keybd_event((byte)VK_CAPITAL, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, UIntPtr.Zero);

                // System.Windows.Forms.SendKeys.SendWait("{CAPSLOCK}");
            }
        }

        public static void SetNumLock(bool numLockOn)
        {
            // Check current state
            bool isNumLockOn = KeyboardState.IsNumLockOn();

            if (numLockOn != isNumLockOn)
            {

                // Import keybd_event function from user32.dll
                [DllImport("user32.dll", SetLastError = true)]
                static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

                // Constants for virtual key codes and event flags
                const int VK_NUMLOCK = 0x90;
                const uint KEYEVENTF_EXTENDEDKEY = 0x1;
                const uint KEYEVENTF_KEYUP = 0x2;

                // Press Caps Lock key
                keybd_event((byte)VK_NUMLOCK, 0x45, KEYEVENTF_EXTENDEDKEY, UIntPtr.Zero);

                // Release Caps Lock key
                keybd_event((byte)VK_NUMLOCK, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, UIntPtr.Zero);

                // System.Windows.Forms.SendKeys.SendWait("{CAPSLOCK}");
            }
        }

        public static void SetScrollLock(bool scrollLockOn)
        {
            // Check current state
            bool isScrollLockOn = KeyboardState.IsScrollLockOn();

            if (scrollLockOn != isScrollLockOn)
            {

                // Import keybd_event function from user32.dll
                [DllImport("user32.dll", SetLastError = true)]
                static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

                // Constants for virtual key codes and event flags
                const int VK_SCROLL = 0x91;
                const uint KEYEVENTF_EXTENDEDKEY = 0x1;
                const uint KEYEVENTF_KEYUP = 0x2;

                // Press Caps Lock key
                keybd_event((byte)VK_SCROLL, 0x45, KEYEVENTF_EXTENDEDKEY, UIntPtr.Zero);

                // Release Caps Lock key
                keybd_event((byte)VK_SCROLL, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, UIntPtr.Zero);

                // System.Windows.Forms.SendKeys.SendWait("{CAPSLOCK}");
            }
        }
    }



    public class BitConverterUtil
    {
        public static bool[] NumberToBitArray(int number, int bitCount = 8)
        {
            string binary = Convert.ToString(number, 2).PadLeft(bitCount, '0');

            bool[] bits = new bool[bitCount];
            for (int i = 0; i < bitCount; i++)
            {
                bits[i] = binary[i] == '1';
            }

            return bits;
        }


        public static long BitArrayToNumber(bool[] bits)
        {
            if (bits == null || bits.Length == 0)
                throw new ArgumentException("Bit array cannot be null or empty.");

            byte[] bitN = new byte[bits.Length];
            for (int i = 0; i < bits.Length; i++)
            {
                bitN[i] = (byte)(bits[i] ? 1 : 0);
            }

            string binary = string.Join("", bitN);
            return Convert.ToInt64(binary, 2);
        }


        // Convert a byte[] into a bool[] representing each bit
        public static bool[] ConvertToBoolBitArray(byte[] input)
        {
            bool[] bitArray = new bool[input.Length * 8];

            for (int i = 0; i < input.Length; i++)
            {
                for (int bit = 0; bit < 8; bit++)
                {
                    // Big-endian bit order (MSB first)
                    bitArray[i * 8 + (7 - bit)] = ((input[i] >> bit) & 1) == 1;
                }
            }

            return bitArray;
        }

        // Convert a bool[] that represents bits back into a byte[]
        public static byte[] ConvertBoolBitArrayToBytes(bool[] bits)
        {
            int byteLength = (bits.Length + 7) / 8; // Round up to full bytes
            byte[] output = new byte[byteLength];

            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                {
                    int byteIndex = i / 8;
                    int bitIndex = 7 - (i % 8); // Big-endian bit order
                    output[byteIndex] |= (byte)(1 << bitIndex);
                }
            }

            return output;
        }


        public static string BinaryArrayToString(int[] binary)
        {
            if (binary.Length % 8 != 0)
                throw new ArgumentException("Binary array length must be a multiple of 8.");

            StringBuilder result = new StringBuilder();

            for (int i = 0; i < binary.Length; i += 8)
            {
                string byteString = "";
                for (int j = 0; j < 8; j++)
                {
                    byteString += binary[i + j];
                }

                int ascii = Convert.ToInt32(byteString, 2);
                result.Append((char)ascii);
            }

            return result.ToString();
        }

    }
}



public static class Hamming3126
{
    private const int DataBits = 26;
    private const int CodeBits = 31;
    private static readonly int[] ParityPositions = { 0, 1, 3, 7, 15 }; // 1-based: 1,2,4,8,16 (0-based indexing)

    public static bool[] Encode(bool[] data)
    {

        // Format: 5 bits representing padding length + n bits of data + m bits of padding to make total multiple of 26bits long

        var result = new List<bool>();
        var dataToEncode = new List<bool>();

        // Calculate the padding required based on the data length + 5 bits to indicate padding length
        int paddingLength = (DataBits - ((data.Length + 5) % DataBits)) % DataBits;
        // Console.WriteLine($"Padding Length {paddingLength}");

        // Create padding bits
        bool[] paddingMetadata = new bool[5];
        for (int j = 0; j < 5; j++)
            paddingMetadata[j] = (paddingLength & (1 << j)) != 0;

        // Add padding metadata and padding together
        dataToEncode.AddRange(paddingMetadata);
        dataToEncode.AddRange(data);
        if (paddingLength > 0)
        {
            dataToEncode.AddRange(new bool[paddingLength]);
        }

        // Encode each block
        for (int i = 0; i < dataToEncode.Count; i += 26)
        {
            bool[] block = new bool[DataBits];
            Array.Copy(dataToEncode.ToArray(), i, block, 0, DataBits);

            result.AddRange(EncodeBlock(block));
        }

        return result.ToArray();
    }

    public static bool[] Decode(bool[] code)
    {

        if (code.Length % CodeBits != 0)
        {
            return null;
        }

        var decodedBlocks = new List<bool>();

        // Decode blocks
        int totalBlocks = code.Length / CodeBits;
        for (int i = 0; i < totalBlocks; i++)
        {
            bool[] block = new bool[CodeBits];
            Array.Copy(code, i * CodeBits, block, 0, CodeBits);
            bool[] decoded = DecodeBlock(block);
            decodedBlocks.AddRange(decoded);
        }

        static void PrintBits(string label, IEnumerable<bool> bits)
        {
            Console.Write($"{label}: ");
            foreach (var bit in bits)
                Console.Write(bit ? '1' : '0');
            Console.WriteLine();
        }

        // Remove padding
        // Calculate padding length
        int paddingLength = 0;
        for (int j = 0; j < 5; j++)
            if (decodedBlocks[j])
                paddingLength |= (1 << j);
        // Copy data not related to padding
        bool[] originalMessage = new bool[decodedBlocks.Count - 5 - paddingLength];
        Array.Copy(decodedBlocks.ToArray(), 5, originalMessage, 0, originalMessage.Length);

        return originalMessage;

    }

    public static bool[] EncodeBlock(bool[] dataBlock)
    {
        bool[] codeword = new bool[CodeBits];
        int dataIdx = 0;

        // Fill in data bits (skip parity positions)
        for (int i = 0; i < CodeBits; i++)
        {
            if (Array.IndexOf(ParityPositions, i) == -1)
            {
                codeword[i] = dataBlock[dataIdx++];
            }
        }

        // Calculate parity bits
        for (int i = 0; i < ParityPositions.Length; i++)
        {
            int p = ParityPositions[i];
            bool parity = false;
            for (int j = 0; j < CodeBits; j++)
            {
                if (j == p) continue;
                if (((j + 1) & (p + 1)) != 0)
                    parity ^= codeword[j];
            }
            codeword[p] = parity;
        }

        return codeword;
    }

    public static bool[] DecodeBlock(bool[] codeword)
    {
        int syndrome = 0;

        // Calculate syndrome
        for (int i = 0; i < ParityPositions.Length; i++)
        {
            int p = ParityPositions[i];
            bool parity = false;

            for (int j = 0; j < CodeBits; j++)
            {
                if (j == p) continue;
                if (((j + 1) & (p + 1)) != 0)
                    parity ^= codeword[j];
            }

            if (parity != codeword[p])
                syndrome |= (1 << i);
        }

        // Correct single-bit error
        if (syndrome != 0 && syndrome <= CodeBits)
        {
            codeword[syndrome - 1] ^= true;
        }

        // Extract data bits (skip parity positions)
        bool[] data = new bool[DataBits];
        int dataIdx = 0;
        for (int i = 0; i < CodeBits; i++)
        {
            if (Array.IndexOf(ParityPositions, i) == -1)
                data[dataIdx++] = codeword[i];
        }

        return data;
    }
}