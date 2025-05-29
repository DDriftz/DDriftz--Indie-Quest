using System;
using System.Runtime.InteropServices;

class Program
{
    static void Main()
    {
        bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        Console.WriteLine("You are running the game on Windows: " + isWindows);
    }
}
