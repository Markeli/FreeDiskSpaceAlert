using System;
using System.IO;
using Topshelf;

namespace Monitor
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = HostFactory.New(x =>
            {
                x.Service<Bootstraper>();
                x.StartAutomatically();
                x.SetServiceName("DriveFreeSpaceMonitor");
                x.SetDisplayName("Drive free space monitoring service");

                x.EnableServiceRecovery(r =>
                {
                    r.RestartService(0);
                    r.OnCrashOnly();
                    r.SetResetPeriod(1);
                });
            });

            host.Run();
        }
    }
}