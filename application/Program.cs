
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using System.Text;
using System.Diagnostics;





Console.WriteLine("Caps Lock Communication!");

while ( true )
{

    Console.Write("> ");
    string cmd = (string) Console.ReadLine();
    string[] splitCmd = cmd.Split(' ');

    if( cmd == "receive" ){

        Console.WriteLine("Waiting...");
        CapsLockTransfer.receiveTransfer();

    } else if( splitCmd.Length == 1 && splitCmd[0] == "send"){

        Console.Write("Message: ");
        string msg = (string) Console.ReadLine();
        CapsLockTransfer.sendString(msg);

    } else if( splitCmd.Length == 1 && splitCmd[0] == "send-test"){

        int[] data = [0,1,1,0,0,1,1,0,1];
        Console.WriteLine("Sending Message...");
        CapsLockMessage.sendMessage(data);

    } else if( cmd == "exit" ){
        break;
    }
}



public class CapsLockTransfer {

    public static int TYPE_BITS = 0x01;
    public static int TYPE_STRING = 0x02;
    public static int TYPE_FILE = 0x03;

    public static int[] receiveTransfer(){

        int[] message = CapsLockMessage.receiveMessage();

        int[] messageTypeBits = new int[4];
        Array.Copy(message, 0, messageTypeBits, 0, 3);
        int messageType = (int) BitConverterUtil.BitArrayToNumber(messageTypeBits.ToArray());
        Console.WriteLine($"Message Type: {messageType}");

        int[] messageData = new int[message.Length-4];
        Array.Copy(message, 4, messageData, 0, message.Length-4);

        if( messageType == TYPE_STRING ){
            Console.WriteLine(BitConverterUtil.BinaryArrayToString(messageData));
        }

        return null;

    }

    public static void sendBits(int[] data){

    }

    public static void sendString(string msg){

        int[] typeEncoding = BitConverterUtil.NumberToBitArray( TYPE_STRING, 4);

        int[] data = BitConverterUtil.stringToBinaryArray(msg);
        Console.WriteLine("Sending Message...");

        int[] combined = typeEncoding.Concat(data).ToArray();
        CapsLockMessage.sendMessage(combined);

    }

    public static void sendFile(string filename){
        // NOT YET IMPLEMENTED
    }

}


public class CapsLockMessage {

    public static int[] receiveMessage(){

        KeyActions.setCapsLock(false);
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
                    if( messageBits.Count < messageLength ){
                        messageBits.Add((isNumLockOn?1:0));
                    }
                    if( messageBits.Count < messageLength ){
                        messageBits.Add((isScrollLockOn?1:0));
                    }

                    if( messageBits.Count == messageLength ){

                        KeyActions.setCapsLock(false);
                        KeyActions.setNumLock(false);
                        KeyActions.setScrollLock(false);

                        return messageBits.ToArray();
                    }
                }

                KeyActions.setCapsLock(false);
                // while( Control.IsKeyLocked(Keys.CapsLock) ){
                //     Thread.Sleep(1);
                // }
            }

        }

    }

    public static void sendMessage( int[] message )
    {

        KeyActions.setCapsLock(false);
        KeyActions.setNumLock(false);
        KeyActions.setScrollLock(false);
        Thread.Sleep(10);

        static void sendData( int[] data ){
            for (int i = 0; i < data.Length; i+=2)
            {
                while( Control.IsKeyLocked(Keys.CapsLock) ){
                    Thread.Sleep(1);
                }
                Thread.Sleep(1);

                int value1 = data[i];
                int value2 = 0;
                if( i+1 <= data.Length - 1){
                    value2 = data[i+1];
                }

                KeyActions.setNumLock(value1==1);
                KeyActions.setScrollLock(value2==1);
                KeyActions.setCapsLock(true);
            }
        }

        // Send message length as 64 bit number in bits followed by message
        Console.WriteLine($"Message Length: {message.Length}");
        int[] lengthBits = BitConverterUtil.NumberToBitArray( message.Length, 64);  // 65 = 'A' = 01000001
        
        sendData(lengthBits); // Send length

        sendData(message); // Send Message

    }
}



public class KeyActions
{
    public static void setCapsLock( bool capsLockOn )
    {

        // Check current state
        bool isCapsLockOn = Control.IsKeyLocked(Keys.CapsLock);

        // Console.WriteLine((capsLockOn?"ON":"OFF"));

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

    public static void setNumLock( bool numLockOn )
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

    public static void setScrollLock( bool scrollLockOn )
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


    public static int[] stringToBinaryArray( string str){

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
}



