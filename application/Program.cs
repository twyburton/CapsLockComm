
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using System.Text;
using System.Diagnostics;
using System.IO;
using CapComm.Utilities;
using CapComm.Communication;


static void main()
{

    Console.WriteLine("Caps Lock Communication!");

    while (true)
    {

        Console.Write("> ");
        string cmd = (string)Console.ReadLine();
        string[] splitCmd = cmd.Split(' ');

        if (cmd == "help")
        {

            Console.WriteLine("Use to transfer data using lock keys.");
            Console.WriteLine("");
            Console.WriteLine("receive = put into receiving mode and wait for transfer");
            Console.WriteLine("send = send a text message");
            Console.WriteLine("send-file = send a file");
            Console.WriteLine("");


        }
        else if (cmd == "receive")
        {

            Console.WriteLine("Waiting...");
            CapsLockTransfer.ReceiveTransfer();

        }
        else if (splitCmd.Length == 1 && splitCmd[0] == "send")
        {

            Console.Write("Message: ");
            string msg = (string)Console.ReadLine();
            CapsLockTransfer.SendString(msg);

        }
        else if (splitCmd.Length == 1 && splitCmd[0] == "send-file")
        {

            Console.Write("File: ");
            string filepath = (string)Console.ReadLine();

            CapsLockTransfer.SendFile(filepath);

        }
        else if (cmd == "exit")
        {
            break;
        }
    }
}

main();






