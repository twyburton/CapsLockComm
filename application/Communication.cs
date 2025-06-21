
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using System.Text;
using System.Diagnostics;
using System.IO;
using CapComm.Utilities;


/*
    This namespace has classes for handling messages and transfers using the lock keys.

    Messages are used as the basic communication type. A message transfers bits (encoded as an int[]).

    Transfers are used to send different types of data.

*/
namespace CapComm.Communication
{

    public class CapsLockTransfer
    {

        public static byte TYPE_BYTES = 0x01;
        public static byte TYPE_STRING = 0x02;
        public static byte TYPE_FILE = 0x03;

        public static byte[] ReceiveTransfer()
        {

            byte[] message = CapsLockMessage.ReceiveMessage();

            byte messageType = message[0];
            Console.WriteLine($"Message Type: {messageType}");

            byte[] messageData = new byte[message.Length - 1];
            Array.Copy(message, 1, messageData, 0, message.Length - 1);
            // Console.WriteLine(string.Join(", ", messageData));

            if (messageType == TYPE_STRING)
            {

                Console.WriteLine(Encoding.UTF8.GetString(messageData));

            }
            else if (messageType == TYPE_BYTES)
            {

                Console.WriteLine("Received Binary Message");
                Console.WriteLine(string.Join(", ", messageData));

            }
            else if (messageType == TYPE_FILE)
            {

                byte[] fileNameLengthBytes = new byte[4];
                Array.Copy(messageData, 0, fileNameLengthBytes, 0, 4);
                int fileNameLength = BitConverter.ToInt32(fileNameLengthBytes, 0);
                // Console.WriteLine($"Filename Length: {fileNameLength}");

                byte[] fileNameBytes = new byte[fileNameLength];
                Array.Copy(messageData, 4, fileNameBytes, 0, fileNameLength);
                string fileName = Encoding.UTF8.GetString(fileNameBytes);
                // Console.WriteLine($"Filename: {fileName}");

                byte[] fileDataLengthBytes = new byte[4];
                Array.Copy(messageData, 4 + fileNameLength, fileDataLengthBytes, 0, 4);
                int fileDataLength = BitConverter.ToInt32(fileDataLengthBytes, 0);
                // Console.WriteLine($"File Data Length: {fileDataLength}");

                byte[] fileDataBytes = new byte[fileDataLength];
                Array.Copy(messageData, 4 + fileNameLength + 4, fileDataBytes, 0, fileDataLength);


                // Check if file already exists and ask for confirmation if it does
                bool writeFile = true;
                if (File.Exists(fileName))
                {
                    Console.Write($"{fileName} already exists. Confirm overwrite (y/N)");
                    string confirm = (string)Console.ReadLine();
                    if (confirm.ToLower() != "y")
                    {
                        writeFile = false;
                    }
                }

                if (writeFile)
                {
                    Console.WriteLine($"Saving to {fileName}...");
                    File.WriteAllBytes(fileName, fileDataBytes);
                }

            }

            return messageData;

        }

        public static void SendBytes(byte[] data)
        {

            byte[] transfer = new byte[data.Length + 1];
            transfer[0] = TYPE_BYTES; // Type encoding
            Array.Copy(data, 0, transfer, 1, data.Length);

            CapsLockMessage.SendMessage(transfer);

        }

        public static void SendString(string msg)
        {

            byte[] data = Encoding.UTF8.GetBytes(msg);
            Console.WriteLine("Sending Message...");

            byte[] transfer = new byte[data.Length + 1];
            transfer[0] = TYPE_STRING; // Type encoding
            Array.Copy(data, 0, transfer, 1, data.Length);

            CapsLockMessage.SendMessage(transfer);

        }

        public static void SendFile(string filepath)
        {
            // Format:
            // Filename Length (2 bytes); filename; FileData Length (8 bytes); fileData


            string[] filepathSplit = filepath.Split("\\");
            string fileName = filepathSplit[filepathSplit.Length - 1];

            byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName); 
            byte[] fileNameLengthBytes = BitConverter.GetBytes(fileNameBytes.Length); // 4 bytes

            byte[] fileDataBytes = File.ReadAllBytes(filepath);
            byte[] fileDataLengthBytes = BitConverter.GetBytes(fileDataBytes.Length); // 4 bytes

            byte[] combined = new byte[1 + fileNameBytes.Length + fileNameLengthBytes.Length + fileDataBytes.Length + fileDataLengthBytes.Length];

            combined[0] = TYPE_FILE; // Type encoding
            int counter = 1;
            Array.Copy(fileNameLengthBytes, 0, combined, counter, fileNameLengthBytes.Length);
            counter += fileNameLengthBytes.Length;
            Array.Copy(fileNameBytes, 0, combined, counter, fileNameBytes.Length);
            counter += fileNameBytes.Length;
            Array.Copy(fileDataLengthBytes, 0, combined, counter, fileDataLengthBytes.Length);
            counter += fileDataLengthBytes.Length;
            Array.Copy(fileDataBytes, 0, combined, counter, fileDataBytes.Length);
            

            Console.WriteLine($"Sending {fileName}...");
            Console.WriteLine($"Total Transfer Length: {combined.Length}");
            CapsLockMessage.SendMessage(combined);
        }

    }


    public class CapsLockMessage
    {

        public static byte[] ReceiveMessage()
        {

            KeyActions.SetCapsLock(false);
            Thread.Sleep(1);

            bool stateReceivingLength = true;

            List<bool> messageLengthBits = new List<bool>();
            long messageLength = -1;

            List<bool> messageBits = new List<bool>();

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
                        messageLengthBits.Add(isNumLockOn);
                        messageLengthBits.Add(isScrollLockOn);

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
                            messageBits.Add(isNumLockOn);
                        }
                        if (messageBits.Count < messageLength)
                        {
                            messageBits.Add(isScrollLockOn);
                        }

                        if (messageBits.Count == messageLength)
                        {

                            KeyActions.SetCapsLock(false);
                            KeyActions.SetNumLock(false);
                            KeyActions.SetScrollLock(false);

                            byte[] messageBytes = BitConverterUtil.ConvertBoolBitArrayToBytes(messageBits.ToArray());

                            return messageBytes;
                        }
                    }

                    KeyActions.SetCapsLock(false);
                    // while( Control.IsKeyLocked(Keys.CapsLock) ){
                    //     Thread.Sleep(1);
                    // }
                }

            }

        }

        public static void SendMessage(byte[] message)
        {
            bool[] messageBits = BitConverterUtil.ConvertToBoolBitArray(message);

            KeyActions.SetCapsLock(false);
            KeyActions.SetNumLock(false);
            KeyActions.SetScrollLock(false);
            Thread.Sleep(10);

            static void sendData(bool[] data)
            {
                for (int i = 0; i < data.Length; i += 2)
                {

                    if (data.Length > 1000 && i % 500 == 0)
                    {
                        Console.WriteLine($"{Math.Round((float)i / (float)data.Length * 100.0, 2)}%");
                    }

                    while (Control.IsKeyLocked(Keys.CapsLock))
                    {
                        Thread.Sleep(1);
                    }
                    Thread.Sleep(1);

                    bool value1 = data[i];
                    bool value2 = false;
                    if (i + 1 <= data.Length - 1)
                    {
                        value2 = data[i + 1];
                    }

                    KeyActions.SetNumLock(value1);
                    KeyActions.SetScrollLock(value2);
                    KeyActions.SetCapsLock(true);
                }
            }

            // Send message length as 64 bit number in bits followed by message
            Console.WriteLine($"Message Length: {messageBits.Length}");
            bool[] lengthBits = BitConverterUtil.NumberToBitArray(messageBits.Length, 64);  // 65 = 'A' = 01000001

            sendData(lengthBits); // Send length

            sendData(messageBits); // Send Message

        }
    }
}