using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using FluentEmail.Core;
using FluentEmail.Smtp;
using Monitor.Configuration;

namespace Monitor.Notifications
{
    public class EmailNotificationChannel : INotificationChannel
    {
        private readonly EmailConfiguration _configuration;
        private readonly ICollection<string> _emails;

        public EmailNotificationChannel(EmailConfiguration configuration, ICollection<string> emails)
        {
            Email.DefaultSender = new SmtpSender(new SmtpClient(configuration.Host, configuration.Port));
            _configuration = configuration;
            _emails = emails;
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
                    .From(_configuration.Email)
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