
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace CapComm.Utilities
{


    
    public class KeyActions
    {
        public static void SetCapsLock( bool capsLockOn )
        {

            // Check current state
            bool isCapsLockOn = Control.IsKeyLocked(Keys.CapsLock);

            if( capsLockOn != isCapsLockOn ){

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

        public static void SetNumLock( bool numLockOn )
        {
            // Check current state
            bool isNumLockOn = Control.IsKeyLocked(Keys.NumLock);

            if( numLockOn != isNumLockOn ){

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

        public static void SetScrollLock( bool scrollLockOn )
        {
            // Check current state
            bool isScrollLockOn = Control.IsKeyLocked(Keys.Scroll);

            if( scrollLockOn != isScrollLockOn ){

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
                bitN[i] = (byte) (bits[i] ? 1 : 0);
            }

            string binary = string.Join("", bitN);
            return Convert.ToInt64(binary, 2);
        }

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