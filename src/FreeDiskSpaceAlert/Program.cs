using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FreeDiskSpaceAlert.Configuration;
using FreeDiskSpaceAlert.Notifications;
using FreeDiskSpaceAlert.ServiceLifetime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace FreeDiskSpaceAlert
{
    class Program
    {
        private const string AppName = "FreeDiskSpaceAlert";
        private static readonly string[] HelpOptions = { "-h", "--help"};
        private static readonly string[] RunAsServiceOptions = { "-s", "--service"};
        
        static async Task Main(string[] args)
        {
            var argsSet = new HashSet<string>(args);
            if (argsSet.Contains(HelpOptions[0]) || argsSet.Contains(HelpOptions[1]))
            {
                ShowHelp();
                return;
            }

            var isService = argsSet.Contains(RunAsServiceOptions[0]) || argsSet.Contains(RunAsServiceOptions[1]);
            
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            var logger = LogManager.LoadConfiguration("NLog.config").GetCurrentClassLogger();

            try
            {
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

        private static void ShowHelp()
        {
            var help = $"{AppName} {Environment.NewLine}" +
                       $"{AppName} is cross platform tool for detecting lack of free disk space and alerting via email.{Environment.NewLine}" +
                       $"To configure tool change config.yml. All config sections are self-described. {Environment.NewLine}" +
                       "To get more details, please, visit https://github.com/Markeli/FreeDiskSpaceAlert";
            Console.WriteLine(help);
        }
    }
}