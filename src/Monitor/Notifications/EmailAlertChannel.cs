using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
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
    public class EmailAlertChannel : IAlertChannel
    {
        private readonly EmailConfiguration _config;
        private readonly ICollection<string> _emails;
        private readonly ILogger _logger;
        
        public bool IsEnabled => _config != null;

        public string ChannelName => "Email";
        
        public EmailAlertChannel(
            EmailConfiguration config,
            ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            if (config != null)
            {
                if (String.IsNullOrWhiteSpace(config.Host))
                    throw new ArgumentNullException($"{nameof(config.Host)} can not be null." +
                                                    "Check host in email configuration section in config file");

                if (String.IsNullOrWhiteSpace(config.Email))
                    throw new ArgumentNullException($"{nameof(config.Email)} can not be null." +
                                                    "Check email configuration section in config file");
                if (String.IsNullOrWhiteSpace(config.Password))
                    throw new ArgumentNullException($"{nameof(config.Password)} can not be null." +
                                                    "Check email configuration section in config file");

                if (config.Recipients == null)
                    throw new ArgumentNullException($"{nameof(config.Recipients)} can not be null." +
                                                    "Check email section in config file");

                var emails = new HashSet<string>();
                foreach (var email in config.Recipients)
                {
                    if (String.IsNullOrWhiteSpace(email))
                        throw new ArgumentException("Email can not be null. Check config file");
                }
                
                Email.DefaultSender = new SmtpSender(
                    new SmtpClient(config.Host,config.Port)
                    {
                        EnableSsl = config.EnableSsl,
                        Timeout = 5000,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(config.Email, config.Password)
                    });
                
                
                _emails = config.Recipients;
            }

            _logger = loggerFactory.CreateLogger<EmailAlertChannel>();
            
            _config = config;
            
           
        }
        
        public async Task NotifyAsync(
            TriggerMode mode, 
            DriveInfo driveInfo, 
            MeasurementUnit unit, 
            double thresholdValueInBytes, 
            string machineName,
            CancellationToken token)
        {
            if (!IsEnabled) return;
            
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