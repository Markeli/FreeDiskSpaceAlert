using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Monitor
{
    public interface IWatchDogService
    {
        Task StartAsync();

        Task StopAsync();
    }

    public class WatchDogService : IWatchDogService
    {
        private readonly INotifier _notifier;
        private CancellationTokenSource _cts;
        private Task _watchDogTask;
        private readonly ICollection<MonitoringTrigger> _triggers;
        private readonly TimeSpan _checkPeriod;
        
        public WatchDogService(
            MonitoringConfiguraion configuration,
            INotifier notifier)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (configuration.Drives == null) throw new ArgumentNullException(nameof(configuration.Drives));
            _notifier = notifier;

            _checkPeriod = TimeSpan.FromSeconds(configuration.CheckPeriodSec);
            
            _triggers = new List<MonitoringTrigger>(configuration.Drives.Count);
            foreach (var drive in configuration.Drives)
            {
                if(!Enum.TryParse<TriggerMode>(drive.TriggerMode, out var mode)) 
                    throw new ArgumentException("Incorrect value for trigger mode. Supported: Accuracy and Percentile");
                
                if(!Enum.TryParse<MeasurementUnit>(drive.MeasurementUnit, out var unit)) 
                    throw new ArgumentException("Incorrect value for measurement unit. Supported: Byte, KB, MB amd GB");
                
                _triggers.Add(new MonitoringTrigger(drive.DeviceName, drive.ThresholdValue, mode, unit));
            }
        }
        
        public Task StartAsync()
        {
            _cts = new CancellationTokenSource();
            var cancellationToken = _cts.Token;
            _watchDogTask = MonitorAsync(cancellationToken);
            return Task.CompletedTask;
        }

        private async Task MonitorAsync(CancellationToken token)
        {
            await Task.Yield();
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(_checkPeriod, token);
                foreach (var drive in DriveInfo.GetDrives())
                {
                    if (token.IsCancellationRequested) break;
                    
                    var raisedTriggers = _triggers
                        .Where(x => x.IsTriggered(drive));
                    
                    foreach (var raisedTrigger in raisedTriggers)
                    {
                        if (token.IsCancellationRequested) break;

                        await _notifier.NotifyAsync(raisedTrigger.Mode, drive, raisedTrigger.EventUnit, token);
                    }
                }
            }
        }

        public async Task StopAsync()
        {
            _cts?.Cancel();
            await _watchDogTask;
        }
    }
}