namespace Monitor.Configuration
{
    public class MonitoredDrive
    {
        public string DeviceName { get; set; }
        
        public double ThresholdValue { get; set; }
        
        public string MeasurementUnit { get; set; }
        
        public string TriggerMode { get; set; }
    }
}