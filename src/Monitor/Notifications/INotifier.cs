using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Monitor
{
    public interface INotifier
    {
        Task NotifyAsync(TriggerMode mode, DriveInfo driveInfo, MeasurementUnit unit, CancellationToken token);
    }
}