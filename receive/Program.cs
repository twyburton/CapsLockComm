
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using System.Text;


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

    bool stateReceivingLength = true;

    List<int> messageLengthBits = new List<int>();
    int messageLength = -1;

    List<int> messageBits = new List<int>();

    bool lastCapsLockState = true;
    while( true )
    {
        bool isCapsLockOn = Control.IsKeyLocked(Keys.CapsLock);
        bool isNumLockOn = Control.IsKeyLocked(Keys.NumLock);
        bool isScrollLockOn = Control.IsKeyLocked(Keys.Scroll);


        if( isCapsLockOn && lastCapsLockState == false){
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
        }

        lastCapsLockState = isCapsLockOn;

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


    public static int BitArrayToNumber(int[] bits)
    {
        if (bits == null || bits.Length == 0)
            throw new ArgumentException("Bit array cannot be null or empty.");

        string binary = string.Join("", bits);
        return Convert.ToInt32(binary, 2);
    }
}



