using System;
using System.IO;

namespace Monitor
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                Console.WriteLine($"{drive.Name}: available free size, bytes {drive.AvailableFreeSpace}");
            }
        }
    }
}