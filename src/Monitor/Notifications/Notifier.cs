using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Monitor.Notifications
{
    public class Notifier : INotifier
    {
        private readonly ICollection<INotificationChannel> _channels;
        
        public Notifier(ICollection<INotificationChannel> channels)
        {
            _channels = channels ?? throw new ArgumentNullException(nameof(channels));
        }
        
        public Task NotifyAsync(
            TriggerMode mode, 
            DriveInfo driveInfo, 
            MeasurementUnit unit, 
            double thresholdValueInBytes,
            string machineName,
            CancellationToken token)
        {
            var tasks = _channels.Select(x =>
                x.NotifyAsync(
                    mode, 
                    driveInfo, 
                    unit, 
                    thresholdValueInBytes, 
                    machineName, 
                    token));
            
            return Task.WhenAll(tasks);
        }
    }
}