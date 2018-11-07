using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using FluentEmail.Core;
using FluentEmail.Smtp;
using Microsoft.Extensions.Configuration;
using Monitor.Configuration;

namespace Monitor.Notifications
{
    public class EmailNotificationChannel : INotificationChannel
    {
        private readonly EmailConfiguration _config;
        private readonly ICollection<string> _emails;

        public EmailNotificationChannel(MonitoringConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (config.EmailConfiguration == null) 
                throw new ArgumentNullException($"{nameof(config.EmailConfiguration)} can not be null. " +
                                                "Check email configuration section in config file");
            if (String.IsNullOrWhiteSpace(config.EmailConfiguration.Host))
                throw new ArgumentNullException($"{nameof(config.EmailConfiguration.Host)} can not be null." +
                                                "Check host in email configuration section in config file");
            if (config.Emails == null)  
                throw new ArgumentNullException($"{nameof(config.Emails)} can not be null." +
                                                "Check email section in config file");
            foreach (var email in config.Emails)
            {
                if (String.IsNullOrWhiteSpace(email))
                    throw new ArgumentException("Email can not be null. Check config file");
            }
            
            Email.DefaultSender = new SmtpSender(
                new SmtpClient(
                    config.EmailConfiguration.Host, 
                    config.EmailConfiguration.Port));
            _config = config.EmailConfiguration;
            _emails = config.Emails;
        }
        
        public async Task NotifyAsync(
            TriggerMode mode, 
            DriveInfo driveInfo, 
            MeasurementUnit unit, 
            double thresholdValueInBytes, 
            string machineName,
            CancellationToken token)
        {
            var diskSize = new DiskSize(driveInfo.AvailableFreeSpace);
            diskSize = diskSize.ConvertTo(unit);
            var messageBody = mode == TriggerMode.Accuracy
                ? $"Available - {diskSize}, limit - {new DiskSize(thresholdValueInBytes).ConvertTo(unit)}"
                : $"Available - {Math.Round((double)driveInfo.AvailableFreeSpace/driveInfo.TotalSize*100, 2)}%, limit - {thresholdValueInBytes*100}%";
            var message = $"Warning. Not enough free memory for drive {driveInfo.Name}. \n{messageBody} \nMachine name{machineName}";

            foreach (var emailAddress in _emails)
            {
                var email = Email
                    .From(_config.Email)
                    .To(emailAddress)
                    .Subject(message);
                var response =  await email.SendAsync(token);
                if (!response.Successful)
                {
                    foreach (var responseErrorMessage in response.ErrorMessages)
                    {
                        Console.WriteLine(responseErrorMessage);
                    }
                }
                
            }
        }
    }
}