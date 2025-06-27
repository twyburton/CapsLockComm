using System;
using System.Runtime.InteropServices;
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
        // There are three types of transfers (more can be added later)
        // These are: 1) Bytes which just transfers an array of bytes; 2) String which 
        // transfers a string (This is just a byte transfer but does the string encoding 
        // and decoding at each end); 3) File which transfers a file and writes the file
        // to the receivers local directory. 

        // This class has a generic receiveTransfer method that will receive transfers
        // of any type and handle appropriatly. There are send methods for each individual
        // transfer type.

        public static byte TYPE_BYTES = 0x01;
        public static byte TYPE_STRING = 0x02;
        public static byte TYPE_FILE = 0x03;

        public static byte[] ReceiveTransfer()
        {

            byte[] message = CapsLockMessage.ReceiveMessage();

            byte messageType = message[0];
            Console.WriteLine($"Message Type: {messageType}");

            // Extract message data
            byte[] messageData = new byte[message.Length - 1];
            Array.Copy(message, 1, messageData, 0, message.Length - 1);

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


    // Class used for reliable message sending.
    public class CapsLockMessage
    {

        // When this method is called the program will wait for a message to be received.
        public static byte[] ReceiveMessage()
        {

            KeyActions.SetCapsLock(false);
            Thread.Sleep(1);

            // We can be in a recceiving length state or not. If true then we are reading 
            // the message length bits. If false, then we are reading the messasge data
            bool stateReceivingLength = true;

            List<bool> messageLengthBits = new List<bool>();
            long messageLength = -1;

            List<bool> messageBits = new List<bool>();

            // Loop until a message is read and then return the message content
            while (true)
            {
                bool isCapsLockOn = KeyboardState.IsCapsLockOn();
                bool isNumLockOn = KeyboardState.IsNumLockOn();
                bool isScrollLockOn = KeyboardState.IsScrollLockOn();

                // If caps lock in on then message is ready to be read.
                if (isCapsLockOn)
                {
                    Thread.Sleep(1); // Pause to account for weird issue discovered in testing where the keypress hangs

                    // If we are still receiving length then add each received bit to messageLengthBits list
                    if (stateReceivingLength)
                    {
                        messageLengthBits.Add(isNumLockOn);
                        messageLengthBits.Add(isScrollLockOn);

                        // If length is 32 then we have whole number
                        if (messageLengthBits.Count == 32)
                        {
                            stateReceivingLength = false; // Change mode to receiving message
                            messageLength = BitConverterUtil.BitArrayToNumber(messageLengthBits.ToArray());
                            Console.WriteLine($"Message Length: {messageLength} bits");
                        }
                    }
                    else
                    {
                        // If we are not in the receiving length state then we should read the message

                        // If we have not reached the message length then read bits.
                        if (messageBits.Count < messageLength)
                        {
                            messageBits.Add(isNumLockOn);
                        }
                        if (messageBits.Count < messageLength)
                        {
                            messageBits.Add(isScrollLockOn);
                        }

                        // If we have ready the correct number of bits then we can stop and return the message
                        if (messageBits.Count == messageLength)
                        {

                            KeyActions.SetCapsLock(false);
                            KeyActions.SetNumLock(false);
                            KeyActions.SetScrollLock(false);

                            // Error correction
                            // var decoder = new ReedSolomonDecoder(10);
                            // messageBytes = decoder.Decode(messageBytes);
                            bool[] messageBitsCorrected = ErrorCorrection.encode(messageBits.ToArray());

                            byte[] messageBytes = BitConverterUtil.ConvertBoolBitArrayToBytes(messageBitsCorrected);

                            

                            return messageBytes;
                        }
                    }

                    KeyActions.SetCapsLock(false);
                }

            }

        }

        // This is used to send a message
        public static void SendMessage(byte[] message)
        {
            

            bool[] messageBits = BitConverterUtil.ConvertToBoolBitArray(message);

            // Error correction handling
            messageBits = ErrorCorrection.encode(messageBits);

            KeyActions.SetCapsLock(false);
            KeyActions.SetNumLock(false);
            KeyActions.SetScrollLock(false);
            Thread.Sleep(10);

            // Send a data encoded as boolean array
            static void sendData(bool[] data)
            {
                // Iterate over data in pairs of two (To be encoded as Num lock & Scroll Lock)
                for (int i = 0; i < data.Length; i += 2)
                {

                    // Progress tracking for large data transfers
                    if (data.Length > 1000 && i % 500 == 0)
                    {
                        Console.WriteLine($"{Math.Round((float)i / (float)data.Length * 100.0, 2)}%");
                    }

                    // Block until the caps lock key is disabled on which indicates that data is ready to be written
                    while (KeyboardState.IsCapsLockOn())
                    {
                        Thread.Sleep(1);
                    }
                    Thread.Sleep(1);

                    // Get values for bit 0 and 1. If the data is an odd number in length then just send bit 1 to false.
                    bool value1 = data[i];
                    bool value2 = false;
                    if (i + 1 <= data.Length - 1)
                    {
                        value2 = data[i + 1];
                    }

                    // Set lock key status based on data and then turn caps lock on to indicate ready to write.
                    KeyActions.SetNumLock(value1);
                    KeyActions.SetScrollLock(value2);
                    KeyActions.SetCapsLock(true);
                }
            }

            // Send message length as 32 bit number in bits followed by message
            Console.WriteLine($"Message Length: {messageBits.Length} bits");
            bool[] lengthBits = BitConverterUtil.NumberToBitArray(messageBits.Length, 32);

            sendData(lengthBits); // Send length

            sendData(messageBits); // Send Message

        }
    }
}


