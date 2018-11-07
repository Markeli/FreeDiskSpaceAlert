using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Monitor.Configuration;
using Monitor.Notifications;

namespace Monitor
{

    public class MonitoringService : BackgroundService
    {
        private readonly IAlertNotifier _alertNotifier;
        private Task _watchDogTask;
        private readonly ICollection<MonitoringTrigger> _triggers;
        private readonly TimeSpan _checkPeriod;
        private readonly string _machineName;
        
        public MonitoringService(
            MonitoringConfiguration configuration,
            IAlertNotifier alertNotifier)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (configuration.Drives == null) throw new ArgumentNullException(nameof(configuration.Drives));
            _alertNotifier = alertNotifier;

            _checkPeriod = TimeSpan.FromSeconds(configuration.CheckPeriodSec);
            _machineName = configuration.MachineName;
            
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
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    foreach (var drive in DriveInfo.GetDrives())
                    {
                        if (stoppingToken.IsCancellationRequested) break;
                        if (!drive.IsReady) continue;
                        
                        var raisedTriggers = _triggers
                            .Where(x => x.IsTriggered(drive));
                    
                        foreach (var raisedTrigger in raisedTriggers)
                        {
                            if (stoppingToken.IsCancellationRequested) break;

                            await _alertNotifier.NotifyAsync(
                                raisedTrigger.Mode, 
                                drive, 
                                raisedTrigger.EventUnit, 
                                raisedTrigger.ThresholdValueInBytes,
                                _machineName,
                                stoppingToken);
                        }
                    }
                    await Task.Delay(_checkPeriod, stoppingToken);
                }
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine(e);
            }
        }
    }
}