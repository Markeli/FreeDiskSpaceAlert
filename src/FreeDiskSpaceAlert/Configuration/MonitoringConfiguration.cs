using System.Collections.Generic;

namespace FreeDiskSpaceAlert.Configuration
{
    public class MonitoringConfiguration
    {
        public int AlertPeriodMin { get; set; }
        
        public string MachineName { get; set; }
        
        public ICollection<MonitoredDrive> Drives { get; set; } 
    }
}