using System.IO;
using System.Threading.Tasks;

namespace Monitor
{
    public interface INotificationChannel
    {
        Task NotifyAsync(TriggerMode mode, DriveInfo driveInfo, MeasurementUnit unit);
    }
}