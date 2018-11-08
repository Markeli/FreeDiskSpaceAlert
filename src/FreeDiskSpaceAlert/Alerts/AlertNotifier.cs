using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FreeDiskSpaceAlert.Alerts
{
    public class AlertNotifier : IAlertNotifier
    {
        private readonly ICollection<IAlertChannel> _channels;

        public bool IsAlertingEnabled { get; }
        
        public AlertNotifier(
            IAlertChannel channel,
            ILoggerFactory loggerFactory)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            var logger = loggerFactory.CreateLogger<AlertNotifier>();
            _channels = new List<IAlertChannel>
            {
                channel
            };
            IsAlertingEnabled = false;
            foreach (var notificationChannel in _channels)
            {
                String message;
                if (notificationChannel.IsEnabled)
                {
                    message = $"{channel.ChannelName} alert channel enabled";
                    IsAlertingEnabled = true;
                }
                else
                {
                    message = $"{channel.ChannelName} alert channel disabled. To enable add email settings to config file.";
                }
                logger.LogInformation(message);
            }
        }

        public AlertNotifier(ICollection<IAlertChannel> channels)
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