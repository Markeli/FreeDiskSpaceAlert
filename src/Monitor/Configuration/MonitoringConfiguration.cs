using System.Collections.Generic;

namespace Monitor.Configuration
{
    public class MonitoringConfiguration
    {
        public int CheckPeriodSec { get; set; }
        
        public string MachineName { get; set; }
        
        public EmailSettings EmailSettings { get; set; }
        
        public TelegramSettings TelegramSettings { get; set; }
        
        public ICollection<string> Emails { get; set; }
        
        public ICollection<string> TelegramAccounts { get; set; } 
        
        public ICollection<MonitoredDrive> Drives { get; set; } 
    }
}