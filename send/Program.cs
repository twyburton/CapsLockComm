
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;


// Import keybd_event function from user32.dll
[DllImport("user32.dll", SetLastError = true)]
static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

// Constants for virtual key codes and event flags
const int VK_CAPITAL = 0x14;
const uint KEYEVENTF_EXTENDEDKEY = 0x1;
const uint KEYEVENTF_KEYUP = 0x2;



static void ToggleCapsLock()
{
    // Check current state
    // bool isCapsLockOn = Control.IsKeyLocked(Keys.CapsLock);

    // Press Caps Lock key
    keybd_event((byte)VK_CAPITAL, 0x45, KEYEVENTF_EXTENDEDKEY, UIntPtr.Zero);

    // Release Caps Lock key
    keybd_event((byte)VK_CAPITAL, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, UIntPtr.Zero);

    // System.Windows.Forms.SendKeys.SendWait("{CAPSLOCK}");
}

// See https://aka.ms/new-console-template for more information
Console.WriteLine("START");


for (int i = 0; i < 10; i++)
{
    // Console.WriteLine(Console.CapsLock);
    // System.Windows.Forms.SendKeys.SendWait("{CAPSLOCK}")
    ToggleCapsLock();
    // Console.WriteLine(Console.CapsLock);
    Thread.Sleep(100);
}

Console.WriteLine("END");