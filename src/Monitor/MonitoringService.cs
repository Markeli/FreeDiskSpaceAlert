using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Monitor.Configuration;
using Monitor.Notifications;

namespace Monitor
{

    public class MonitoringService : BackgroundService
    {
        private readonly IAlertNotifier _alertNotifier;
        private readonly ICollection<AlertTrigger> _triggers;
        private readonly TimeSpan _checkPeriod;
        private readonly string _machineName;
        private readonly ILogger _logger;

        public MonitoringService(
            MonitoringConfiguration configuration,
            IAlertNotifier alertNotifier,
            ILoggerFactory loggerFactory)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            if (configuration.Drives == null) 
                throw new ArgumentNullException($"{nameof(configuration.Drives)} can not be null. " +
                                                "Check drive section in config file");
            if (String.IsNullOrWhiteSpace(configuration.MachineName))
                throw new ArgumentNullException($"{nameof(configuration.MachineName)} can not be null." +
                                                "Check config file");
            
            _alertNotifier = alertNotifier;
            _logger = loggerFactory.CreateLogger<MonitoringService>();

            _checkPeriod = TimeSpan.FromSeconds(configuration.CheckPeriodSec);
            _machineName = configuration.MachineName;
            
            _triggers = new List<AlertTrigger>(configuration.Drives.Count);
            foreach (var drive in configuration.Drives)
            {
                _triggers.Add(
                    new AlertTrigger(
                        drive.DeviceName, 
                        drive.ThresholdValue, 
                        drive.TriggerMode, 
                        drive.MeasurementUnit));
            }
        }

        public override async Task StartAsync(CancellationToken token)
        {
            _logger.LogInformation("Starting monitoring service");
            
            if (!_alertNotifier.IsAlertingEnabled)
                throw new AppMonkeyBusinessException("No alert channels registered. Configure one or more channels in config to run app");
            
            await base.StartAsync(token);
            _logger.LogInformation("Monitoring service started");
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    
                    await Task.Delay(_checkPeriod, stoppingToken);
                    
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
                }
                catch (OperationCanceledException e)
                {
                    _logger.LogError(e, "Monitoring service was stopped");
                }
                catch (AggregateException e)
                {
                    foreach (var innerException in e.InnerExceptions)
                    {
                        _logger.LogError(e, $"Error on monitoring. {innerException.Message}");
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Critical error on monitoring service");
                }
                
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping monitoring service");
            await base.StopAsync(cancellationToken);
            _logger.LogInformation("Monitoring service stopped");
        }
    }
}