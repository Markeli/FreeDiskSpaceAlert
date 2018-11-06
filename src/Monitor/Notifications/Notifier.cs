using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Monitor.Notifications
{
    public class Notifier : INotifier
    {
        public Task NotifyAsync(
            TriggerMode mode, 
            DriveInfo driveInfo, 
            MeasurementUnit unit, 
            double thresholdValueInBytes,
            string machineName,
            CancellationToken token)
        {
            var diskSize = new DiskSize(driveInfo.AvailableFreeSpace);
            diskSize = diskSize.ConvertTo(unit);
            var messageBody = mode == TriggerMode.Accuracy
                ? $"Available - {diskSize}, limit - {new DiskSize(thresholdValueInBytes).ConvertTo(unit)}"
                : $"Available - {Math.Round((double)driveInfo.AvailableFreeSpace/driveInfo.TotalSize*100, 2)}%, limit - {thresholdValueInBytes*100}%";
            Console.WriteLine($"Warning. Not enough free memory for drive {driveInfo.Name}. \n{messageBody} \nMachine name{machineName}");
            return Task.CompletedTask;
        }
    }
}