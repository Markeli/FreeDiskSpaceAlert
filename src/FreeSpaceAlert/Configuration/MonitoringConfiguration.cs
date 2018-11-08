using System.Collections.Generic;

namespace FreeSpaceAlert.Configuration
{
    public class MonitoringConfiguration
    {
        public int CheckPeriodSec { get; set; }
        
        public string MachineName { get; set; }
        
        public ICollection<MonitoredDrive> Drives { get; set; } 
    }
}