using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using FluentEmail.Core;
using FluentEmail.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Monitor.Configuration;

namespace Monitor.Notifications
{
    public class EmailNotificationChannel : INotificationChannel
    {
        private readonly EmailConfiguration _config;
        private readonly ICollection<string> _emails;
        private readonly ILogger _logger;

        public EmailNotificationChannel(
            MonitoringConfiguration config,
            ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (config.EmailConfiguration == null) 
                throw new ArgumentNullException($"{nameof(config.EmailConfiguration)} can not be null. " +
                                                "Check email configuration section in config file");
            if (String.IsNullOrWhiteSpace(config.EmailConfiguration.Host))
                throw new ArgumentNullException($"{nameof(config.EmailConfiguration.Host)} can not be null." +
                                                "Check host in email configuration section in config file");
            
            if (String.IsNullOrWhiteSpace(config.EmailConfiguration.Email))  
                throw new ArgumentNullException($"{nameof(config.EmailConfiguration.Email)} can not be null." +
                                                "Check email configuration section in config file");
            if (config.Emails == null)  
                throw new ArgumentNullException($"{nameof(config.Emails)} can not be null." +
                                                "Check email section in config file");
            
            foreach (var email in config.Emails)
            {
                if (String.IsNullOrWhiteSpace(email))
                    throw new ArgumentException("Email can not be null. Check config file");
            }

            _logger = loggerFactory.CreateLogger<EmailNotificationChannel>();
            
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
                    .Subject($"Alert triggered on machine {machineName}")
                    .Body(message);
                var response =  await email.SendAsync(token);
                if (!response.Successful)
                {
                    foreach (var responseErrorMessage in response.ErrorMessages)
                    {
                        _logger.LogWarning($"Error on sending email to {emailAddress}. " +
                                         $"Error message: {responseErrorMessage}");
                    }
                }
                else
                {
                    _logger.LogInformation($"Email successfully sent to {emailAddress}");
                }
                
            }
        }
    }
}