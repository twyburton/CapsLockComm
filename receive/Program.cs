
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using System.Text;

// Import keybd_event function from user32.dll
[DllImport("user32.dll", SetLastError = true)]
static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

// Constants for virtual key codes and event flags
const int VK_CAPITAL = 0x14;
const int VK_NUMLOCK = 0x90;
const int VK_SCROLL = 0x91;
const uint KEYEVENTF_EXTENDEDKEY = 0x1;
const uint KEYEVENTF_KEYUP = 0x2;

static void setCapsLock( bool capsLockOn )
{
    // Check current state
    bool isCapsLockOn = Control.IsKeyLocked(Keys.CapsLock);

    // Console.WriteLine((capsLockOn?"ON":"OFF"));

    if( capsLockOn != isCapsLockOn ){
        // Press Caps Lock key
        keybd_event((byte)VK_CAPITAL, 0x45, KEYEVENTF_EXTENDEDKEY, UIntPtr.Zero);

        // Release Caps Lock key
        keybd_event((byte)VK_CAPITAL, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, UIntPtr.Zero);

        // System.Windows.Forms.SendKeys.SendWait("{CAPSLOCK}");
    }
}

static void setNumLock( bool numLockOn )
{
    // Check current state
    bool isNumLockOn = Control.IsKeyLocked(Keys.NumLock);

    if( numLockOn != isNumLockOn ){
        // Press Caps Lock key
        keybd_event((byte)VK_NUMLOCK, 0x45, KEYEVENTF_EXTENDEDKEY, UIntPtr.Zero);

        // Release Caps Lock key
        keybd_event((byte)VK_NUMLOCK, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, UIntPtr.Zero);

        // System.Windows.Forms.SendKeys.SendWait("{CAPSLOCK}");
    }
}

static void setScrollLock( bool scrollLockOn )
{
    // Check current state
    bool isScrollLockOn = Control.IsKeyLocked(Keys.Scroll);

    if( scrollLockOn != isScrollLockOn ){
        // Press Caps Lock key
        keybd_event((byte)VK_SCROLL, 0x45, KEYEVENTF_EXTENDEDKEY, UIntPtr.Zero);

        // Release Caps Lock key
        keybd_event((byte)VK_SCROLL, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, UIntPtr.Zero);

        // System.Windows.Forms.SendKeys.SendWait("{CAPSLOCK}");
    }
}


static string BinaryArrayToString(int[] binary)
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



Console.WriteLine("START");

static int[] receiveMessage(){

    setCapsLock(false);
    Thread.Sleep(1);

    bool stateReceivingLength = true;

    List<int> messageLengthBits = new List<int>();
    long messageLength = -1;

    List<int> messageBits = new List<int>();

    // bool lastCapsLockState = true;
    while( true )
    {
        bool isCapsLockOn = Control.IsKeyLocked(Keys.CapsLock);
        bool isNumLockOn = Control.IsKeyLocked(Keys.NumLock);
        bool isScrollLockOn = Control.IsKeyLocked(Keys.Scroll);


        if( isCapsLockOn ){
            Thread.Sleep(1); // Account for weird issue where the keypress hangs
            // Console.WriteLine($"{(isNumLockOn?1:0)} {(isScrollLockOn?1:0)}");

            // If we are still receiving length then add each received bit to messageLengthBits list
            if( stateReceivingLength ){
                messageLengthBits.Add((isNumLockOn?1:0));
                messageLengthBits.Add((isScrollLockOn?1:0));

                // If length is 64 then we have whole number
                if( messageLengthBits.Count == 64 ){
                    stateReceivingLength = false; // Change mode to receiving message
                    messageLength = BitConverterUtil.BitArrayToNumber(messageLengthBits.ToArray());
                    Console.WriteLine($"Message Length: {messageLength}");
                }
            } else {
                messageBits.Add((isNumLockOn?1:0));
                messageBits.Add((isScrollLockOn?1:0));

                if( messageLength == messageBits.Count){
                    return messageBits.ToArray();
                }
            }

            setCapsLock(false);
            while( Control.IsKeyLocked(Keys.CapsLock) ){
                Thread.Sleep(1);
            }
        }

        // lastCapsLockState = isCapsLockOn;

        // if( capsLock ){
        //     data += "1";
        // } else {
        //     data += "0";
        // }
        // Thread.Sleep(0);
    }

}

int[] message = receiveMessage();
Console.WriteLine(BinaryArrayToString(message));

Console.WriteLine("END");





public class BitConverterUtil
{
    public static int[] NumberToBitArray(int number, int bitCount = 8)
    {
        string binary = Convert.ToString(number, 2).PadLeft(bitCount, '0');

        int[] bits = new int[bitCount];
        for (int i = 0; i < bitCount; i++)
        {
            bits[i] = binary[i] == '1' ? 1 : 0;
        }

        return bits;
    }


    public static long BitArrayToNumber(int[] bits)
    {
        if (bits == null || bits.Length == 0)
            throw new ArgumentException("Bit array cannot be null or empty.");

        string binary = string.Join("", bits);
        return Convert.ToInt64(binary, 2);
    }
}



