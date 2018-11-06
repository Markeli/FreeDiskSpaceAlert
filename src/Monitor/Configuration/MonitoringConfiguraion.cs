using System.Collections.Generic;

namespace Monitor
{
    public class MonitoringConfiguraion
    {
        public int CheckPeriodSec { get; set; }
        
        public EmailSettings EmailSettings { get; set; }
        
        public TelegramSettings TelegramSettings { get; set; }
        
        public ICollection<string> Emails { get; set; }
        
        public ICollection<string> TelegramAccounts { get; set; } 
        
        public ICollection<MonitoredDrive> Drives { get; set; } 
    }
}