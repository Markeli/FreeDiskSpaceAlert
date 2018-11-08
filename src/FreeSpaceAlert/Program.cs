using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FreeSpaceAlert.Configuration;
using FreeSpaceAlert.Notifications;
using FreeSpaceAlert.ServiceLifetime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace FreeSpaceAlert
{
    class Program
    {
        private static string AppName = "FreeSpaceAlert";
        
        static async Task Main(string[] args)
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            var logger = LogManager.LoadConfiguration("NLog.config").GetCurrentClassLogger();

            try
            {
                var isService = false;

                var builder = new HostBuilder();
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddYamlFile("config.yml");
                });
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(
                        new LoggerFactory().AddNLog(new NLogProviderOptions
                        {
                            CaptureMessageTemplates = true,
                            CaptureMessageProperties = true
                        }));
                    services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
                    services.AddSingleton(typeof(ILogger), c =>
                    {
                        var factory = c.GetRequiredService<ILoggerFactory>();
                        return factory.CreateLogger("main");
                    });
                    services.AddLogging(b =>
                    {
                        b.ClearProviders();
                        b.SetMinimumLevel(LogLevel.Trace);
                    });
                    services.AddSingleton(c =>
                    {
                        var config = c.GetService<IConfiguration>();
                        var emailConfigSection = config.GetChildren().FirstOrDefault(x => x.Key == "EmailConfiguration");
                        if (emailConfigSection == null) return null;

                        var emailConfiguration = new EmailConfiguration();
                        emailConfigSection.Bind(emailConfiguration);
                        return emailConfiguration;
                    });
                    services.AddSingleton(c =>
                    {
                        var config = c.GetService<IConfiguration>();
                        var monitoringConfiguration = new MonitoringConfiguration();
                        config.Bind(monitoringConfiguration);
                        return monitoringConfiguration;
                    });
                    services.AddSingleton<IAlertChannel, EmailAlertChannel>();
                    services.AddSingleton<IAlertNotifier, AlertNotifier>();
                    services.AddHostedService<MonitoringService>();
                });

                if (isService)
                {
                    await builder.RunAsServiceAsync();
                }
                else
                {
                    await builder.RunConsoleAsync();
                }
            }
            catch (AppMonkeyBusinessException e)
            {
                logger.Warn($"{AppName} stopped. {e.Message}");
            }
            catch (Exception e)
            {
                logger.Error(e, $"Critical error occured in {AppName}");
            }
            finally
            {
                LogManager.Shutdown();
            }
        }
    }
}