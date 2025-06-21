
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using System.Text;
using System.Diagnostics;
using System.IO;
using CapComm.Utilities;

namespace CapComm.Communication
{

    public class CapsLockTransfer {

        public static int TYPE_BITS = 0x01;
        public static int TYPE_STRING = 0x02;
        public static int TYPE_FILE = 0x03;

        public static int[] ReceiveTransfer(){

            int[] message = CapsLockMessage.ReceiveMessage();

            int[] messageTypeBits = new int[4];
            Array.Copy(message, 0, messageTypeBits, 0, 4);
            int messageType = (int) BitConverterUtil.BitArrayToNumber(messageTypeBits);
            Console.WriteLine($"Message Type: {messageType}");

            int[] messageData = new int[message.Length-4];
            Array.Copy(message, 4, messageData, 0, message.Length-4);

            if( messageType == TYPE_STRING ){

                Console.WriteLine(BitConverterUtil.BinaryArrayToString(messageData));

            } else if( messageType == TYPE_BITS ){

                Console.WriteLine("Received Binary Message");

            } else if( messageType == TYPE_FILE ){

                int[] fileNameLengthBits = new int[16];
                Array.Copy(messageData, 0, fileNameLengthBits, 0, 16);
                long fileNameLength = BitConverterUtil.BitArrayToNumber(fileNameLengthBits);
                // Console.WriteLine($"Filename Length: {fileNameLength}");

                int[] fileNameBits = new int[fileNameLength];
                Array.Copy(messageData, 16, fileNameBits, 0, fileNameLength);
                string fileName = BitConverterUtil.BinaryArrayToString(fileNameBits);
                // Console.WriteLine($"Filename: {fileName}");

                int[] fileDataLengthBits = new int[64];
                Array.Copy(messageData, 16 + fileNameLength, fileDataLengthBits, 0, 64);
                long fileDataLength = BitConverterUtil.BitArrayToNumber(fileDataLengthBits);
                // Console.WriteLine($"File Data Length: {fileDataLength}");

                int[] fileDataBits = new int[fileDataLength];
                Array.Copy(messageData, 16 + fileNameLength + 64, fileDataBits, 0, fileDataLength);

                static byte[] BitArrayToByteArray(int[] bits)
                {
                    int byteCount = (bits.Length + 7) / 8; // Round up
                    byte[] bytes = new byte[byteCount];

                    for (int i = 0; i < bits.Length; i++)
                    {
                        int byteIndex = i / 8;
                        int bitIndex = 7 - (i % 8); // Big-endian (MSB first)
                        bytes[byteIndex] |= (byte)(bits[i] << bitIndex);
                    }

                    return bytes;
                }
                byte[] fileDataBytes = BitArrayToByteArray(fileDataBits);


                // Check if file already exists and ask for confirmation if it does
                bool writeFile = true;
                if (File.Exists(fileName))
                {
                    Console.Write($"{fileName} already exists. Confirm overwrite (y/N)");
                    string confirm = (string) Console.ReadLine();
                    if( confirm.ToLower() != "y"){
                        writeFile = false;
                    }
                }
                
                if( writeFile){
                    Console.WriteLine($"Saving to {fileName}...");
                    File.WriteAllBytes(fileName, fileDataBytes);
                }

            }

            return messageData;

        }

        public static void SendBits(int[] data){

            int[] typeEncoding = BitConverterUtil.NumberToBitArray( TYPE_BITS, 4);

            Console.WriteLine("Sending Message...");

            int[] combined = typeEncoding.Concat(data).ToArray();
            CapsLockMessage.SendMessage(combined);

        }

        public static void SendString(string msg){

            int[] typeEncoding = BitConverterUtil.NumberToBitArray( TYPE_STRING, 4);

            int[] data = BitConverterUtil.stringToBinaryArray(msg);
            Console.WriteLine("Sending Message...");

            int[] combined = typeEncoding.Concat(data).ToArray();
            CapsLockMessage.SendMessage(combined);

        }

        public static void SendFile(string filepath ){
            // Format:
            // Filename Length (2 bytes); filename; FileData Length (8 bytes); fileData

            int[] typeEncoding = BitConverterUtil.NumberToBitArray( TYPE_FILE, 4);


            string[] filepathSplit = filepath.Split("\\");
            string fileName = filepathSplit[filepathSplit.Length-1];

            int[] fileNameData = BitConverterUtil.stringToBinaryArray(fileName);
            int[] fileNameLength = BitConverterUtil.NumberToBitArray( fileNameData.Length, 16);
            int[] fileNameComponents = fileNameLength.Concat(fileNameData).ToArray();

            static int[] ByteArrayToBitArray(byte[] bytes)
            {
                int[] bits = new int[bytes.Length * 8];
                for (int i = 0; i < bytes.Length; i++)
                {
                    for (int bit = 0; bit < 8; bit++)
                    {
                        // Extract bit from most significant to least significant
                        bits[i * 8 + bit] = (bytes[i] >> (7 - bit)) & 1;
                    }
                }
                return bits;
            }
            
            byte[] fileDataBytes = File.ReadAllBytes(filepath);
            int[] fileDataBits = ByteArrayToBitArray(fileDataBytes);
            int[] fileDataLength = BitConverterUtil.NumberToBitArray( fileDataBits.Length, 64);
            int[] fileDataComponents = fileDataLength.Concat(fileDataBits).ToArray();

            int[] combined = fileNameComponents.Concat(fileDataComponents).ToArray();
            combined = typeEncoding.Concat(combined).ToArray();
            Console.WriteLine($"Sending {fileName}...");
            Console.WriteLine($"Total Transfer Length: {combined.Length}");
            CapsLockMessage.SendMessage(combined);
        }

    }


    public class CapsLockMessage {

        public static int[] ReceiveMessage(){

            KeyActions.SetCapsLock(false);
            Thread.Sleep(1);

            bool stateReceivingLength = true;

            List<int> messageLengthBits = new List<int>();
            long messageLength = -1;

            List<int> messageBits = new List<int>();

            // bool lastCapsLockState = true;
            while (true)
            {
                bool isCapsLockOn = Control.IsKeyLocked(Keys.CapsLock);
                bool isNumLockOn = Control.IsKeyLocked(Keys.NumLock);
                bool isScrollLockOn = Control.IsKeyLocked(Keys.Scroll);


                if (isCapsLockOn)
                {
                    Thread.Sleep(1); // Account for weird issue where the keypress hangs
                    // Console.WriteLine($"{(isNumLockOn?1:0)} {(isScrollLockOn?1:0)}");

                    // If we are still receiving length then add each received bit to messageLengthBits list
                    if (stateReceivingLength)
                    {
                        messageLengthBits.Add((isNumLockOn ? 1 : 0));
                        messageLengthBits.Add((isScrollLockOn ? 1 : 0));

                        // If length is 64 then we have whole number
                        if (messageLengthBits.Count == 64)
                        {
                            stateReceivingLength = false; // Change mode to receiving message
                            messageLength = BitConverterUtil.BitArrayToNumber(messageLengthBits.ToArray());
                            Console.WriteLine($"Message Length: {messageLength}");
                        }
                    }
                    else
                    {
                        if (messageBits.Count < messageLength)
                        {
                            messageBits.Add((isNumLockOn ? 1 : 0));
                        }
                        if (messageBits.Count < messageLength)
                        {
                            messageBits.Add((isScrollLockOn ? 1 : 0));
                        }

                        if (messageBits.Count == messageLength)
                        {

                            KeyActions.SetCapsLock(false);
                            KeyActions.SetNumLock(false);
                            KeyActions.SetScrollLock(false);

                            return messageBits.ToArray();
                        }
                    }

                    KeyActions.SetCapsLock(false);
                    // while( Control.IsKeyLocked(Keys.CapsLock) ){
                    //     Thread.Sleep(1);
                    // }
                }

            }

        }

        public static void SendMessage( int[] message )
        {

            KeyActions.SetCapsLock(false);
            KeyActions.SetNumLock(false);
            KeyActions.SetScrollLock(false);
            Thread.Sleep(10);

            static void sendData( int[] data ){
                for (int i = 0; i < data.Length; i+=2)
                {

                    if( data.Length > 1000 && i%500==0){
                        Console.WriteLine($"{Math.Round((float)i/(float)data.Length*100.0,2)}%");
                    }

                    while( Control.IsKeyLocked(Keys.CapsLock) ){
                        Thread.Sleep(1);
                    }
                    Thread.Sleep(1);

                    int value1 = data[i];
                    int value2 = 0;
                    if( i+1 <= data.Length - 1){
                        value2 = data[i+1];
                    }

                    KeyActions.SetNumLock(value1==1);
                    KeyActions.SetScrollLock(value2==1);
                    KeyActions.SetCapsLock(true);
                }
            }

            // Send message length as 64 bit number in bits followed by message
            Console.WriteLine($"Message Length: {message.Length}");
            int[] lengthBits = BitConverterUtil.NumberToBitArray( message.Length, 64);  // 65 = 'A' = 01000001
            
            sendData(lengthBits); // Send length

            sendData(message); // Send Message

        }
    }
}