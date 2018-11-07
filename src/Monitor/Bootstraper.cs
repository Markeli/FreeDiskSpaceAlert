using System.IO;
using Microsoft.Extensions.Configuration;
using Monitor.Configuration;
using Monitor.Notifications;
using Topshelf;

namespace Monitor
{
    public class Bootstraper : ServiceControl
    {
        private WatchDogService _watchDog;

        public bool Start(HostControl hostControl)
        {
            var config = GetConfig();
            var emailChannel = new EmailNotificationChannel(config.EmailConfiguration, config.Emails);
            var notifier = new Notifier(new[]
            {
                emailChannel
            });
            _watchDog = new WatchDogService(config, notifier);
            _watchDog.StartAsync().GetAwaiter().GetResult();
            return true;
        }

        private MonitoringConfiguration GetConfig()
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.SetBasePath(Directory.GetCurrentDirectory());
            configBuilder.AddYamlFile("config.yml");
            var config = configBuilder.Build();
            var monitoringConfiguration = new MonitoringConfiguration();
            config.Bind(monitoringConfiguration);
            return monitoringConfiguration;
        }

        public bool Stop(HostControl hostControl)
        {
            _watchDog?.StopAsync().GetAwaiter().GetResult();
            return true;
        }
    }
}