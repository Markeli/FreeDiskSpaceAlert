using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Monitor.Notifications
{
    public interface IAlertNotifier
    {
        Task NotifyAsync(
            TriggerMode mode, 
            DriveInfo driveInfo, 
            MeasurementUnit unit,
            double thresholdValueInBytes,
            string machineName,
            CancellationToken token);
    }
}