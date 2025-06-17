
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;



string data = "";

Console.WriteLine("START");

for (int i = 0; i < 500; i++)
{
    bool capsLock = Console.CapsLock;
    bool numLock = Console.NumberLock;

    if( capsLock ){
        data += "1";
    } else {
        data += "0";
    }
    Thread.Sleep(5);
}

Console.WriteLine("END");

Console.WriteLine(data);
