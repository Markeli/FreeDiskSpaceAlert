namespace FreeSpaceAlert.Configuration
{
    public class MonitoredDrive
    {
        public string DeviceName { get; set; }
        
        public double ThresholdValue { get; set; }
        
        public MeasurementUnit MeasurementUnit { get; set; }
        
        public TriggerMode TriggerMode { get; set; }
    }
}