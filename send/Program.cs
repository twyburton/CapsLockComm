
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using System.Text;
using System.Diagnostics;


// Import keybd_event function from user32.dll
[DllImport("user32.dll", SetLastError = true)]
static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

// Constants for virtual key codes and event flags
const int VK_CAPITAL = 0x14;
const int VK_NUMLOCK = 0x90;
const int VK_SCROLL = 0x91;
const uint KEYEVENTF_EXTENDEDKEY = 0x1;
const uint KEYEVENTF_KEYUP = 0x2;

const int INTERVAL = 10;



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

// See https://aka.ms/new-console-template for more information
Console.WriteLine("START");





static void sendMessage( int[] message )
{

    setCapsLock(false);
    setNumLock(false);
    setScrollLock(false);
    Thread.Sleep(10);

    static void sendData( int[] data ){
        for (int i = 0; i < data.Length; i+=2)
        {
            while( Control.IsKeyLocked(Keys.CapsLock) ){
                Thread.Sleep(1);
            }
            Thread.Sleep(1);

            int value1 = data[i];
            int value2 = data[i+1];
            // Console.WriteLine($"{value1} {value2}");

            setNumLock(value1==1);
            setScrollLock(value2==1);
            setCapsLock(true);
            
            // Thread.Sleep(100);
            // setCapsLock(false);
            // Thread.Sleep(INTERVAL);
        }
    }

    // Send message length as 64 bit number in bits followed by message
    Console.WriteLine($"Message Length: {message.Length}");
    int[] lengthBits = BitConverterUtil.NumberToBitArray( message.Length, 64);  // 65 = 'A' = 01000001
    
    sendData(lengthBits); // Send length

    // Console.WriteLine($"Time to send message: {(message.Length/2*(INTERVAL*2))}ms");
    sendData(message); // Send Message
}


string dstring = "Hello World! Followed by longer message to test resilience!";
// string dstring = "HE";
int[] data = stringToBinaryArray(dstring);

Stopwatch stopwatch = new Stopwatch();
stopwatch.Start();
sendMessage(data);
stopwatch.Stop();
Console.WriteLine($"Time since start: {stopwatch.ElapsedMilliseconds} ms");
Console.WriteLine($"Bitrate: {((float)data.Length*1000.0)/(float)stopwatch.ElapsedMilliseconds} b/s");

Console.WriteLine("END");



static int[] stringToBinaryArray( string str){

    List<int> binaryBits = new List<int>();
    foreach (char c in str)
    {
        byte ascii = (byte)c;
        string binary = Convert.ToString(ascii, 2).PadLeft(8, '0');

        foreach (char bit in binary)
        {
            binaryBits.Add(bit == '1' ? 1 : 0);
        }
    }

    return binaryBits.ToArray();
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