using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Monitor.Notifications
{
    public class AlertNotifier : IAlertNotifier
    {
        private readonly ICollection<INotificationChannel> _channels;
        private readonly ILogger _logger;

        public AlertNotifier(
            INotificationChannel channel,
            ILoggerFactory loggerFactory)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            _logger = loggerFactory.CreateLogger<AlertNotifier>();
            _channels = new List<INotificationChannel>
            {
                channel
            };
        }

        public AlertNotifier(ICollection<INotificationChannel> channels)
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